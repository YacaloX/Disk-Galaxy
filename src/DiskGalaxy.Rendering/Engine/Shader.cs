using Silk.NET.OpenGL;
using Serilog;

namespace DiskGalaxy.Rendering.Engine;

public sealed class Shader : IDisposable
{
    private readonly uint _handle;
    private readonly GL _gl;
    private readonly ILogger _logger;

    public Shader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;
        _logger = Log.ForContext<Shader>();

        var vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        var fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);
        _handle = _gl.CreateProgram();

        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);

        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            var info = _gl.GetProgramInfoLog(_handle);
            _gl.DeleteProgram(_handle);
            throw new InvalidOperationException($"Program link failure: {info}");
        }

        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);

        _logger.Debug("Shader program {Handle} compiled and linked", _handle);
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public void SetUniform(string name, int value)
    {
        _gl.Uniform1(GetLocation(name), value);
    }

    public void SetUniform(string name, float value)
    {
        _gl.Uniform1(GetLocation(name), value);
    }

    public void SetUniform(string name, Silk.NET.Maths.Vector3D<float> value)
    {
        _gl.Uniform3(GetLocation(name), value.X, value.Y, value.Z);
    }

    public void SetUniform(string name, Silk.NET.Maths.Matrix4X4<float> value)
    {
        unsafe
        {
            _gl.UniformMatrix4(GetLocation(name), 1, false, (float*)&value);
        }
    }

    private int GetLocation(string name)
    {
        var loc = _gl.GetUniformLocation(_handle, name);
        if (loc == -1)
        {
            _logger.Warning("Uniform '{Name}' not found in shader", name);
        }
        return loc;
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        var handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, source);
        _gl.CompileShader(handle);

        _gl.GetShader(handle, ShaderParameterName.CompileStatus, out var status);
        if (status == 0)
        {
            var info = _gl.GetShaderInfoLog(handle);
            _gl.DeleteShader(handle);
            throw new InvalidOperationException($"{type} compile failure: {info}");
        }

        return handle;
    }
}
