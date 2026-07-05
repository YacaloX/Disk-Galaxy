using DiskGalaxy.Rendering.Engine;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GlShader = DiskGalaxy.Rendering.Engine.Shader;

namespace DiskGalaxy.Rendering.Renderer;

public sealed class SkyboxRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly GlShader _shader;
    private uint _vao, _vbo;
    private readonly int _starCount;

    private static readonly Random Rng = new(42);

    public SkyboxRenderer(GL gl, GlShader shader, int starCount = 3000)
    {
        _gl = gl;
        _shader = shader;
        _starCount = starCount;

        SetupStars();
    }

    private void SetupStars()
    {
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        var data = new float[_starCount * 4]; // pos(3) + brightness(1)

        for (var i = 0; i < _starCount; i++)
        {
            var theta = Rng.NextDouble() * Math.PI * 2;
            var phi = Math.Acos(2 * Rng.NextDouble() - 1);
            var r = 500 + Rng.NextDouble() * 1500;

            data[i * 4] = (float)(r * Math.Sin(phi) * Math.Cos(theta));
            data[i * 4 + 1] = (float)(r * Math.Cos(phi));
            data[i * 4 + 2] = (float)(r * Math.Sin(phi) * Math.Sin(theta));
            data[i * 4 + 3] = (float)(0.2 + Rng.NextDouble() * 0.8);
        }

        unsafe
        {
            fixed (float* ptr = data)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(data.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }
        }

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 4 * sizeof(float), IntPtr.Zero);
        _gl.EnableVertexAttribArray(0);

        _gl.VertexAttribPointer(1, 1, GLEnum.Float, false, 4 * sizeof(float), (IntPtr)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }

    public void Render(Matrix4X4<float> viewProj)
    {
        _shader.Use();
        _shader.SetUniform("uViewProj", viewProj);

        _gl.Disable(EnableCap.DepthTest);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Points, 0, (uint)_starCount);
        _gl.BindVertexArray(0);

        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Enable(EnableCap.DepthTest);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
        _shader.Dispose();
    }
}
