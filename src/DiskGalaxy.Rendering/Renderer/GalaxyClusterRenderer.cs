using DiskGalaxy.Rendering.Engine;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GlShader = DiskGalaxy.Rendering.Engine.Shader;

namespace DiskGalaxy.Rendering.Renderer;

public readonly struct ClusterRenderData
{
    public Vector3D<float> Center { get; }
    public Vector3D<float> Color { get; }
    public float Radius { get; }
    public float Alpha { get; }

    public ClusterRenderData(Vector3D<float> center, Vector3D<float> color, float radius, float alpha = 1f)
    {
        Center = center;
        Color = color;
        Radius = radius;
        Alpha = alpha;
    }
}

public sealed class GalaxyClusterRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly GlShader _shader;
    private uint _vao, _particleVbo, _clusterVbo;
    private readonly int _particlesPerCluster;
    private int _clusterCount;

    private static readonly Random Rng = new(42);

    public GalaxyClusterRenderer(GL gl, GlShader shader, int particlesPerCluster = 80)
    {
        _gl = gl;
        _shader = shader;
        _particlesPerCluster = particlesPerCluster;

        SetupParticlePositions();
        SetupInstancing();
    }

    private void SetupParticlePositions()
    {
        _vao = _gl.GenVertexArray();
        _particleVbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _particleVbo);

        var positions = new float[_particlesPerCluster * 3];
        for (var i = 0; i < _particlesPerCluster; i++)
        {
            var theta = Rng.NextDouble() * Math.PI * 2;
            var phi = Math.Acos(2 * Rng.NextDouble() - 1);
            var r = 0.5 + Rng.NextDouble() * 0.5;

            positions[i * 3] = (float)(r * Math.Sin(phi) * Math.Cos(theta));
            positions[i * 3 + 1] = (float)(r * Math.Sin(phi) * Math.Sin(theta));
            positions[i * 3 + 2] = (float)(r * Math.Cos(phi));
        }

        unsafe
        {
            fixed (float* ptr = positions)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(positions.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }
        }

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), IntPtr.Zero);
        _gl.EnableVertexAttribArray(0);
    }

    private void SetupInstancing()
    {
        _clusterVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _clusterVbo);

        var stride = (uint)(3 + 3 + 1 + 1) * sizeof(float);

        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 3, GLEnum.Float, false, stride, IntPtr.Zero);
        _gl.VertexAttribDivisor(1, 1);

        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 3, GLEnum.Float, false, stride, (IntPtr)(3 * sizeof(float)));
        _gl.VertexAttribDivisor(2, 1);

        _gl.EnableVertexAttribArray(3);
        _gl.VertexAttribPointer(3, 1, GLEnum.Float, false, stride, (IntPtr)(6 * sizeof(float)));
        _gl.VertexAttribDivisor(3, 1);

        _gl.EnableVertexAttribArray(4);
        _gl.VertexAttribPointer(4, 1, GLEnum.Float, false, stride, (IntPtr)(7 * sizeof(float)));
        _gl.VertexAttribDivisor(4, 1);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }

    public void UpdateClusters(List<ClusterRenderData> clusters)
    {
        _clusterCount = clusters.Count;
        if (_clusterCount == 0) return;

        var data = new float[_clusterCount * 8];

        for (var i = 0; i < _clusterCount; i++)
        {
            var cd = clusters[i];
            var idx = i * 8;
            data[idx] = cd.Center.X;
            data[idx + 1] = cd.Center.Y;
            data[idx + 2] = cd.Center.Z;
            data[idx + 3] = cd.Color.X;
            data[idx + 4] = cd.Color.Y;
            data[idx + 5] = cd.Color.Z;
            data[idx + 6] = cd.Radius;
            data[idx + 7] = cd.Alpha;
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _clusterVbo);
        unsafe
        {
            fixed (float* ptr = data)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(data.Length * sizeof(float)), ptr, BufferUsageARB.DynamicDraw);
            }
        }
    }

    public void Render(Matrix4X4<float> viewProj)
    {
        if (_clusterCount == 0) return;

        _shader.Use();
        _shader.SetUniform("uViewProj", viewProj);

        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        _gl.BindVertexArray(_vao);
        _gl.DrawArraysInstanced(PrimitiveType.Points, 0, (uint)_particlesPerCluster, (uint)_clusterCount);
        _gl.BindVertexArray(0);

        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_particleVbo);
        _gl.DeleteBuffer(_clusterVbo);
        _gl.DeleteVertexArray(_vao);
        _shader.Dispose();
    }
}
