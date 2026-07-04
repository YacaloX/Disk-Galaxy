using System.Runtime.InteropServices;
using DiskGalaxy.Rendering.Engine;
using DiskGalaxy.Rendering.Scene;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DiskGalaxy.Rendering.Renderer;

public sealed class NodeRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly Engine.Shader _shader;
    private uint _vao, _vbo, _instanceVbo;
    private int _maxInstances;
    private int _instanceCount;

    private static readonly float[] QuadVertices =
    [
        -0.5f, -0.5f, 0.0f,
         0.5f, -0.5f, 0.0f,
         0.5f,  0.5f, 0.0f,
        -0.5f, -0.5f, 0.0f,
         0.5f,  0.5f, 0.0f,
        -0.5f,  0.5f, 0.0f,
    ];

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct InstanceData
    {
        public readonly float PosX, PosY, PosZ;
        public readonly float ColorR, ColorG, ColorB;
        public readonly float Size;
        public readonly float IsFolder;

        public InstanceData(Vector3D<float> pos, Vector3D<float> color, float size, bool isFolder)
        {
            PosX = pos.X; PosY = pos.Y; PosZ = pos.Z;
            ColorR = color.X; ColorG = color.Y; ColorB = color.Z;
            Size = size;
            IsFolder = isFolder ? 1f : 0f;
        }
    }

    public NodeRenderer(GL gl, Engine.Shader shader, int maxInstances = 100000)
    {
        _gl = gl;
        _shader = shader;
        _maxInstances = maxInstances;

        SetupGeometry();
        SetupInstancing();
    }

    private void SetupGeometry()
    {
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        var handle = GCHandle.Alloc(QuadVertices, GCHandleType.Pinned);
        var ptr = handle.AddrOfPinnedObject();
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(QuadVertices.Length * sizeof(float)), ref ptr, BufferUsageARB.StaticDraw);
        handle.Free();

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)(3 * sizeof(float)), IntPtr.Zero);
        _gl.EnableVertexAttribArray(0);
    }

    private void SetupInstancing()
    {
        _instanceVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVbo);

        var stride = (uint)Marshal.SizeOf<InstanceData>();

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

    public void UpdateInstances(List<SceneNode> nodes)
    {
        _instanceCount = Math.Min(nodes.Count, _maxInstances);
        var data = new InstanceData[_instanceCount];

        for (var i = 0; i < _instanceCount; i++)
        {
            var node = nodes[i];
            data[i] = new InstanceData(node.Position, node.Color, node.Size, node.IsFolder);
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVbo);

        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var ptr = handle.AddrOfPinnedObject();
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_instanceCount * Marshal.SizeOf<InstanceData>()), ref ptr, BufferUsageARB.DynamicDraw);
        handle.Free();
    }

    public void Render(Matrix4X4<float> viewProj)
    {
        if (_instanceCount == 0) return;

        _shader.Use();
        _shader.SetUniform("uViewProj", viewProj);

        _gl.BindVertexArray(_vao);
        _gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (uint)_instanceCount);
        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_instanceVbo);
        _gl.DeleteVertexArray(_vao);
        _shader.Dispose();
    }
}
