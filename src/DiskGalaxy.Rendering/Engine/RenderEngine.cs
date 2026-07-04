using DiskGalaxy.Rendering.Camera;
using DiskGalaxy.Rendering.Renderer;
using DiskGalaxy.Rendering.Scene;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Serilog;

namespace DiskGalaxy.Rendering.Engine;

public sealed class RenderEngine : IDisposable
{
    private readonly GL _gl;
    private readonly Camera.Camera _camera;
    private readonly CameraController _controller;
    private readonly NodeRenderer _nodeRenderer;
    private readonly EdgeRenderer _edgeRenderer;
    private SceneGraph? _sceneGraph;
    private readonly ILogger _logger;

    public Camera.Camera Camera => _camera;
    public CameraController Controller => _controller;

    public RenderEngine(GL gl, int maxNodes = 100000)
    {
        _gl = gl;
        _logger = Log.ForContext<RenderEngine>();
        _camera = new Camera.Camera();
        _controller = new CameraController(_camera);

        var nodeShader = new Shader(_gl, ShaderSources.NodeVertex, ShaderSources.NodeFragment);
        var edgeShader = new Shader(_gl, ShaderSources.EdgeVertex, ShaderSources.EdgeFragment);

        _nodeRenderer = new NodeRenderer(_gl, nodeShader, maxNodes);
        _edgeRenderer = new EdgeRenderer(_gl, edgeShader);

        _camera.UpdateVectors();

        _gl.ClearColor(0.04f, 0.04f, 0.06f, 1.0f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Enable(EnableCap.ProgramPointSize);

        _logger.Information("RenderEngine initialized (OpenGL {Version})", _gl.GetStringS(StringName.Version));
    }

    public void SetScene(SceneGraph? graph)
    {
        _sceneGraph = graph;
        if (graph is not null)
        {
            _nodeRenderer.UpdateInstances(graph.VisibleNodes);
            _edgeRenderer.UpdateLines(graph.Edges);
        }
    }

    public void Update(float deltaTime)
    {
        _controller.Update(deltaTime);
    }

    public void Render(int viewportWidth, int viewportHeight)
    {
        _gl.Viewport(0, 0, (uint)viewportWidth, (uint)viewportHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _camera.SetAspectRatio(viewportWidth, viewportHeight);
        var viewProj = _camera.GetViewMatrix() * _camera.GetProjectionMatrix();

        if (_sceneGraph is not null)
        {
            _edgeRenderer.Render(viewProj);
            _nodeRenderer.Render(viewProj);
        }
    }

    public void ResizeScene(SceneGraph? graph)
    {
        SetScene(graph);
    }

    public void Dispose()
    {
        _nodeRenderer.Dispose();
        _edgeRenderer.Dispose();
    }
}
