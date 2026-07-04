using System.Runtime.InteropServices;
using DiskGalaxy.Rendering.Engine;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DiskGalaxy.Rendering.Renderer;

public sealed class EdgeRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly Engine.Shader _shader;
    private uint _vao, _vbo;
    private int _lineCount;

    public EdgeRenderer(GL gl, Engine.Shader shader)
    {
        _gl = gl;
        _shader = shader;

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        var stride = (uint)(6 * sizeof(float));

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, stride, IntPtr.Zero);
        _gl.EnableVertexAttribArray(0);

        _gl.VertexAttribPointer(1, 3, GLEnum.Float, false, stride, (IntPtr)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }

    public void UpdateLines(List<(Vector3D<float> Start, Vector3D<float> End)> edges)
    {
        _lineCount = edges.Count * 2;
        var verts = new float[_lineCount * 6];
        var idx = 0;

        foreach (var (start, end) in edges)
        {
            var dist = Vector3D.Distance(start, end);
            var alpha = Math.Clamp(1f - dist / 200f, 0.1f, 0.5f);

            verts[idx++] = start.X; verts[idx++] = start.Y; verts[idx++] = start.Z;
            verts[idx++] = alpha; verts[idx++] = alpha; verts[idx++] = alpha;

            verts[idx++] = end.X; verts[idx++] = end.Y; verts[idx++] = end.Z;
            verts[idx++] = alpha; verts[idx++] = alpha; verts[idx++] = alpha;
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        var handle = GCHandle.Alloc(verts, GCHandleType.Pinned);
        var ptr = handle.AddrOfPinnedObject();
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(verts.Length * sizeof(float)), ref ptr, BufferUsageARB.DynamicDraw);
        handle.Free();
    }

    public void Render(Matrix4X4<float> viewProj)
    {
        if (_lineCount == 0) return;

        _shader.Use();
        _shader.SetUniform("uViewProj", viewProj);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Lines, 0, (uint)_lineCount);
        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
        _shader.Dispose();
    }
}
