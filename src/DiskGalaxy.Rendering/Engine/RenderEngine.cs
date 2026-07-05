using System.Diagnostics;
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
    private readonly GalaxyClusterRenderer _clusterRenderer;
    private SceneGraph? _sceneGraph;
    private readonly ILogger _logger;

    private int _frameCount;
    private long _lastStatsTime;
    private SceneNode? _hoveredNode;
    private SceneNode? _selectedNode;
    private float _viewportWidth;
    private float _viewportHeight;

    public Camera.Camera Camera => _camera;
    public CameraController Controller => _controller;
    public SceneNode? HoveredNode => _hoveredNode;
    public SceneNode? SelectedNode => _selectedNode;

    public RenderEngine(GL gl, int maxNodes = 500000)
    {
        _gl = gl;
        _logger = Log.ForContext<RenderEngine>();
        _camera = new Camera.Camera();
        _controller = new CameraController(_camera);

        // Clear any stale GL errors from Avalonia's context initialization
        while (_gl.GetError() != GLEnum.NoError) { }

        _logger.Information("Creating node shader...");
        var nodeShader = new Shader(_gl, ShaderSources.NodeVertex, ShaderSources.NodeFragment);

        _logger.Information("Creating edge shader...");
        var edgeShader = new Shader(_gl, ShaderSources.EdgeVertex, ShaderSources.EdgeFragment);

        _logger.Information("Creating cluster shader...");
        var clusterShader = new Shader(_gl, ShaderSources.ClusterVertex, ShaderSources.ClusterFragment);

        _logger.Information("Creating node renderer...");
        _nodeRenderer = new NodeRenderer(_gl, nodeShader, maxNodes);

        _logger.Information("Creating edge renderer...");
        _edgeRenderer = new EdgeRenderer(_gl, edgeShader);

        _logger.Information("Creating galaxy cluster renderer...");
        _clusterRenderer = new GalaxyClusterRenderer(_gl, clusterShader);

        _camera.UpdateVectors();
        _gl.ClearColor(0.04f, 0.04f, 0.06f, 1.0f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _lastStatsTime = Stopwatch.GetTimestamp();

        _logger.Information("RenderEngine initialized (OpenGL {Version})", _gl.GetStringS(StringName.Version));
    }

    public void SetScene(SceneGraph? graph)
    {
        _sceneGraph = graph;
        if (graph is not null)
        {
            _logger.Information("SetScene: {Nodes} nodes, {Edges} edges", graph.VisibleNodes.Count, graph.Edges.Count);
            _nodeRenderer.UpdateInstances(graph.VisibleNodes);
            _edgeRenderer.UpdateLines(graph.Edges);
            UpdateClusterData(graph);
        }
        else
        {
            _logger.Information("SetScene: null");
            _clusterRenderer.UpdateClusters([]);
        }
    }

    public void Update(float deltaTime)
    {
        _camera.UpdateAnimation(deltaTime);
        if (!_camera.IsAnimating)
            _controller.Update(deltaTime);
    }

    public void Render(int viewportWidth, int viewportHeight)
    {
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
        _gl.Viewport(0, 0, (uint)viewportWidth, (uint)viewportHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.Clear(ClearBufferMask.DepthBufferBit);

        _camera.SetAspectRatio(viewportWidth, viewportHeight);
        var viewProj = _camera.GetProjectionMatrix() * _camera.GetViewMatrix();

        if (_sceneGraph is not null)
        {
            _edgeRenderer.Render(viewProj);
            _clusterRenderer.Render(viewProj);
            _nodeRenderer.Render(viewProj, _camera.Position);
        }

        _frameCount++;
        var now = Stopwatch.GetTimestamp();
        var elapsed = (now - _lastStatsTime) / (double)Stopwatch.Frequency;
        if (elapsed >= 5.0)
        {
            var fps = _frameCount / elapsed;
            _logger.Information("FPS={Fps:F0} Nodes={Nodes}", fps, _nodeRenderer.InstanceCount);
            _frameCount = 0;
            _lastStatsTime = now;
        }
    }

    public SceneNode? HitTest(float screenX, float screenY)
    {
        if (_sceneGraph is null) return null;
        var (origin, dir) = _camera.UnprojectRay(screenX, screenY, _viewportWidth, _viewportHeight);
        return _sceneGraph.PickNode(origin, dir);
    }

    public void SetHoveredNode(SceneNode? node)
    {
        if (_hoveredNode == node) return;

        if (_hoveredNode is not null)
        {
            _hoveredNode.IsHovered = false;
            _hoveredNode.Color = _hoveredNode.BaseColor;
        }

        _hoveredNode = node;

        if (node is not null)
        {
            node.IsHovered = true;
            node.Color = node.BaseColor * 1.4f;
        }

        if (_sceneGraph is not null)
            _nodeRenderer.UpdateInstances(_sceneGraph.VisibleNodes);
    }

    public void FlyToNode(SceneNode node)
    {
        var distance = Math.Max(node.Size * 5f, 5f);
        var dir = Vector3D.Normalize(_camera.Position - node.Position);
        var targetPos = node.Position + dir * distance;

        var yaw = _camera.Yaw;
        var pitch = _camera.Pitch;

        _camera.StartAnimation(targetPos, yaw, pitch, 0.8f);
    }

    public void SetSelectedNode(SceneNode? node)
    {
        if (_selectedNode is not null)
        {
            _selectedNode.IsSelected = false;
            _selectedNode.Color = _selectedNode.BaseColor;
        }

        _selectedNode = node;

        if (node is not null)
        {
            node.IsSelected = true;
            node.Color = node.BaseColor * 1.8f + new Vector3D<float>(0.3f, 0.3f, 0.3f);
        }

        if (_sceneGraph is not null)
            _nodeRenderer.UpdateInstances(_sceneGraph.VisibleNodes);
    }

    private void UpdateClusterData(SceneGraph graph)
    {
        var folderNodes = graph.AllNodes.Where(n => n.IsFolder);
        var clusterData = new List<ClusterRenderData>();

        foreach (var node in folderNodes)
        {
            var radius = Math.Max(0.5f, MathF.Log10(Math.Max(node.ByteSize, 1) + 1) * 0.6f);
            clusterData.Add(new ClusterRenderData(node.Position, node.Color, radius));
        }

        _clusterRenderer.UpdateClusters(clusterData);
    }

    public void Dispose()
    {
        _nodeRenderer.Dispose();
        _edgeRenderer.Dispose();
        _clusterRenderer.Dispose();
    }
}
