using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace RenderTest;

static class X11
{
    const string LibX = "libX11.so.6";
    const string LibGlx = "libGLX.so.0";

    [DllImport(LibX)] public static extern nint XOpenDisplay(nint display);
    [DllImport(LibX)] public static extern int XDefaultScreen(nint display);
    [DllImport(LibX)] public static extern nint XRootWindow(nint display, int screen);
    [DllImport(LibX)] public static extern nint XCreateSimpleWindow(nint display, nint parent, int x, int y, int w, int h, int bw, int border, int bg);
    [DllImport(LibX)] public static extern int XMapWindow(nint display, nint window);
    [DllImport(LibX)] public static extern int XDestroyWindow(nint display, nint window);
    [DllImport(LibX)] public static extern int XCloseDisplay(nint display);

    [DllImport(LibGlx)] public static extern nint glXChooseVisual(nint display, int screen, int[] attribs);
    [DllImport(LibGlx)] public static extern nint glXCreateContext(nint display, nint visual, nint share, bool direct);
    [DllImport(LibGlx)] public static extern int glXMakeCurrent(nint display, nint window, nint ctx);
    [DllImport(LibGlx)] public static extern nint glXGetProcAddress(string name);
    [DllImport(LibGlx)] public static extern int glXDestroyContext(nint display, nint ctx);
    [DllImport(LibGlx)] public static extern void glXSwapBuffers(nint display, nint window);
}

unsafe class Program
{
    static void Main()
    {
        Console.WriteLine("=== RenderTest: OpenGL via GLX + X11 ===\n");

        var dpy = X11.XOpenDisplay(nint.Zero);
        if (dpy == nint.Zero) { Console.WriteLine("FAIL: XOpenDisplay"); return; }
        var screen = X11.XDefaultScreen(dpy);
        var root = X11.XRootWindow(dpy, screen);

        // Choose a GLX visual: GLX_RGBA=4, GLX_RED_SIZE=8, GLX_GREEN_SIZE=9, GLX_BLUE_SIZE=10, GLX_DEPTH_SIZE=12, None=0
        int[] visAttribs = [4, 8, 8, 9, 8, 10, 8, 12, 24, 0];
        var vi = X11.glXChooseVisual(dpy, screen, visAttribs);
        if (vi == nint.Zero) { Console.WriteLine("FAIL: glXChooseVisual"); X11.XCloseDisplay(dpy); return; }
        Console.WriteLine("  Visual OK");

        var ctx = X11.glXCreateContext(dpy, vi, nint.Zero, true);
        if (ctx == nint.Zero) { Console.WriteLine("FAIL: glXCreateContext"); X11.XCloseDisplay(dpy); return; }
        Console.WriteLine("  Context OK");

        // Create a 1x1 hidden window
        var win = X11.XCreateSimpleWindow(dpy, root, 0, 0, 256, 256, 0, 0, 0);
        // Don't map it (keep hidden)

        X11.glXMakeCurrent(dpy, win, ctx);
        Console.WriteLine("  Context made current\n");

        var gl = GL.GetApi(X11.glXGetProcAddress);
        Console.WriteLine($"OpenGL: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"GLSL:   {gl.GetStringS(StringName.ShadingLanguageVersion)}\n");

#pragma warning disable IDE0007 // pointers can't use var
        byte* pix = stackalloc byte[4];
#pragma warning restore IDE0007

        // ---- Test 1: Clear + Readback ----
        Console.WriteLine("-- Test 1: Clear + Readback --");
        gl.ClearColor(0.1f, 0.2f, 0.3f, 1f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.ReadPixels(0, 0, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pix);
        Console.WriteLine($"  Center pixel: ({pix[0]},{pix[1]},{pix[2]},{pix[3]})");
        var pass = Math.Abs(pix[0] - 26) <= 1 && Math.Abs(pix[1] - 51) <= 1 && Math.Abs(pix[2] - 77) <= 1;
        Console.WriteLine(pass ? "  PASS" : $"  FAIL (expected ~26,51,77, got {pix[0]},{pix[1]},{pix[2]})");

        // ---- Test 2: Full-screen triangle (gl_VertexID) ----
        Console.WriteLine("\n-- Test 2: Full-screen triangle (gl_VertexID) --");
        var fsTriVs = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(fsTriVs, """
            #version 330 core
            void main() {
                float x = (gl_VertexID == 0 ? -1.0 : (gl_VertexID == 1 ? 3.0 : -1.0));
                float y = (gl_VertexID == 1 ? -1.0 : (gl_VertexID == 2 ? 3.0 : -1.0));
                gl_Position = vec4(x, y, 0.0, 1.0);
            }
            """);
        gl.CompileShader(fsTriVs);
        gl.GetShader(fsTriVs, ShaderParameterName.CompileStatus, out var ok);
        if (ok == 0) { Console.WriteLine($"  FAIL: tri vertex shader compile\n  {gl.GetShaderInfoLog(fsTriVs)}"); return; }

        var fsTriFs = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fsTriFs, """
            #version 330 core
            out vec4 fragColor;
            void main() { fragColor = vec4(0.0, 1.0, 0.0, 1.0); }
            """);
        gl.CompileShader(fsTriFs);
        gl.GetShader(fsTriFs, ShaderParameterName.CompileStatus, out var ok2);
        var okFinal = ok2;
        if (okFinal == 0) { Console.WriteLine($"  FAIL: tri fragment shader compile\n  {gl.GetShaderInfoLog(fsTriFs)}"); return; }

        var triProg = gl.CreateProgram();
        gl.AttachShader(triProg, fsTriVs);
        gl.AttachShader(triProg, fsTriFs);
        gl.LinkProgram(triProg);
        gl.GetProgram(triProg, GLEnum.LinkStatus, out var ok3);
        if (ok3 == 0) { Console.WriteLine($"  FAIL: tri program link\n  {gl.GetProgramInfoLog(triProg)}"); return; }

        gl.UseProgram(triProg);
        uint vao; // IDE0007: can't infer from out parameter
        gl.GenVertexArrays(1, out vao);
        gl.BindVertexArray(vao);
        gl.ClearColor(0.04f, 0.04f, 0.06f, 1f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);

        gl.ReadPixels(128, 128, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pix);
        Console.WriteLine($"  Center pixel: ({pix[0]},{pix[1]},{pix[2]},{pix[3]})");
        Console.WriteLine(pix[1] > 200 ? "  PASS (green)" : "  FAIL (not green)");

        // ---- Test 3: MVP triangle with identity matrix ----
        Console.WriteLine("\n-- Test 3: MVP triangle (identity, NDC coords) --");
        var mvpVs = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(mvpVs, """
            #version 330 core
            layout(location=0) in vec2 aPos;
            uniform mat4 uMVP;
            void main() { gl_Position = uMVP * vec4(aPos, 0.0, 1.0); }
            """);
        gl.CompileShader(mvpVs);
        gl.GetShader(mvpVs, ShaderParameterName.CompileStatus, out var ok4);
        if (ok4 == 0) { Console.WriteLine($"  FAIL: mvp vertex shader compile\n  {gl.GetShaderInfoLog(mvpVs)}"); return; }

        var mvpFs = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(mvpFs, """
            #version 330 core
            out vec4 fragColor;
            void main() { fragColor = vec4(1.0, 0.0, 0.0, 1.0); }
            """);
        gl.CompileShader(mvpFs);
        gl.GetShader(mvpFs, ShaderParameterName.CompileStatus, out var ok5);
        if (ok5 == 0) { Console.WriteLine($"  FAIL: mvp fragment shader compile\n  {gl.GetShaderInfoLog(mvpFs)}"); return; }

        var mvpProg = gl.CreateProgram();
        gl.AttachShader(mvpProg, mvpVs);
        gl.AttachShader(mvpProg, mvpFs);
        gl.LinkProgram(mvpProg);
        gl.GetProgram(mvpProg, GLEnum.LinkStatus, out var ok6);
        if (ok6 == 0) { Console.WriteLine($"  FAIL: mvp program link\n  {gl.GetProgramInfoLog(mvpProg)}"); return; }

        var triVerts = new float[] { -0.5f, -0.5f, 0.5f, -0.5f, 0.0f, 0.5f };
        uint vbo;
        gl.GenBuffers(1, out vbo);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (float* p = triVerts) gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(triVerts.Length * 4), p, BufferUsageARB.StaticDraw);
        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, null);
        gl.EnableVertexAttribArray(0);

        gl.UseProgram(mvpProg);
        var uMvp = gl.GetUniformLocation(mvpProg, "uMVP");
        Console.WriteLine($"  uMVP location: {uMvp}");
        if (uMvp >= 0)
        {
            float[] identity = [1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1];
            fixed (float* p = identity) gl.UniformMatrix4(uMvp, 1, false, p);
        }

        gl.ClearColor(0.04f, 0.04f, 0.06f, 1f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);

        // Read center pixel (128,128), not (0,0) — triangle covers NDC y=-0.5 to 0.5
        gl.ReadPixels(128, 128, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pix);
        Console.WriteLine($"  Center pixel: ({pix[0]},{pix[1]},{pix[2]},{pix[3]})");
        Console.WriteLine(pix[0] > 200 ? "  PASS (red)" : "  FAIL (not red)");

        // ---- Summary ----
        Console.WriteLine("\n=== All tests complete ===");

        X11.glXDestroyContext(dpy, ctx);
        X11.XDestroyWindow(dpy, win);
        X11.XCloseDisplay(dpy);
    }
}
