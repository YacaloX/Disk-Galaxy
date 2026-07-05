using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using DiskGalaxy.Rendering.Engine;
using DiskGalaxy.Rendering.Scene;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GlInterface = Avalonia.OpenGL.GlInterface;

namespace DiskGalaxy.UI.Controls;

public sealed class OpenGlHost : OpenGlControlBase
{
    private RenderEngine? _engine;
    private GL? _gl;
    private bool _initialized;
    private bool _needsSceneUpdate = true;
    private ILogger _logger = null!;

    private long _lastFrameTime;
    private long _lastClickTime;
    private Point _lastMousePos;

    public event Action<SceneNode?>? SelectionChanged;
    public event Action<SceneNode?>? NodeDoubleClicked;

    static OpenGlHost()
    {
        AffectsRender<OpenGlHost>(SceneGraphProperty);
    }

    public static readonly StyledProperty<SceneGraph?> SceneGraphProperty =
        AvaloniaProperty.Register<OpenGlHost, SceneGraph?>(nameof(SceneGraph));

    public SceneGraph? SceneGraph
    {
        get => GetValue(SceneGraphProperty);
        set => SetValue(SceneGraphProperty, value);
    }

    public RenderEngine? Engine => _engine;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SceneGraphProperty)
        {
            _needsSceneUpdate = true;
            _logger.Information("SceneGraph changed: {HasValue}", SceneGraph is not null);
            if (SceneGraph is not null)
                _logger.Information("SceneGraph: {Nodes} nodes, {Edges} edges",
                    SceneGraph.VisibleNodes.Count, SceneGraph.Edges.Count);
            RequestNextFrameRendering();
        }
    }

    protected override void OnOpenGlInit(GlInterface glInterface)
    {
        base.OnOpenGlInit(glInterface);

        _logger = Log.ForContext<OpenGlHost>();

        _gl = GL.GetApi(glInterface.GetProcAddress);
        _engine = new RenderEngine(_gl);
        _initialized = true;

        _logger.Information("OpenGlHost initialized (GL {Version})", _gl.GetStringS(StringName.Version));

        _lastFrameTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        RequestNextFrameRendering();
    }

    protected override void OnOpenGlDeinit(GlInterface glInterface)
    {
        if (_initialized)
        {
            _engine?.Dispose();
            _gl?.Dispose();
            _initialized = false;
        }
        base.OnOpenGlDeinit(glInterface);
    }

    protected override void OnOpenGlRender(GlInterface glInterface, int fb)
    {
        if (!_initialized || _engine is null) return;

        try
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var deltaTime = (now - _lastFrameTime) / 1000f;
            _lastFrameTime = now;
            deltaTime = Math.Clamp(deltaTime, 0.001f, 0.05f);

            if (_needsSceneUpdate)
            {
                _engine.SetScene(SceneGraph);
                _needsSceneUpdate = false;
            }

            _engine.Update(deltaTime);

            var w = Math.Max(1, (int)Bounds.Width);
            var h = Math.Max(1, (int)Bounds.Height);
            _engine.Render(w, h);

            Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during OpenGL render");
        }
    }

    protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _lastMousePos = e.GetPosition(this);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _engine?.Controller.MouseDown(new Vector2D<float>((float)_lastMousePos.X, (float)_lastMousePos.Y));

            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var clickElapsed = now - _lastClickTime;
            _lastClickTime = now;

            var hit = _engine?.HitTest((float)_lastMousePos.X, (float)_lastMousePos.Y);

            if (hit is not null)
            {
                _engine?.SetSelectedNode(hit);
                SelectionChanged?.Invoke(hit);

                if (clickElapsed < 500 && clickElapsed > 0)
                {
                    NodeDoubleClicked?.Invoke(hit);
                }
            }
            else
            {
                _engine?.SetSelectedNode(null);
                SelectionChanged?.Invoke(null);
            }
        }
    }

    protected override void OnPointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _engine?.Controller.MouseUp();
    }

    protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_engine is not null)
        {
            var pos = e.GetPosition(this);
            _engine.Controller.MouseMove(new Vector2D<float>((float)pos.X, (float)pos.Y));
            var hit = _engine.HitTest((float)pos.X, (float)pos.Y);
            _engine.SetHoveredNode(hit);
        }
    }

    protected override void OnPointerWheelChanged(Avalonia.Input.PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        _engine?.Controller.Scroll((float)e.Delta.Y);
    }

    protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var key = MapKey(e.Key);
        if (key.HasValue)
            _engine?.Controller.KeyDown(key.Value);
    }

    protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
    {
        base.OnKeyUp(e);
        var key = MapKey(e.Key);
        if (key.HasValue)
            _engine?.Controller.KeyUp(key.Value);
    }

    private static Key? MapKey(Avalonia.Input.Key key) => key switch
    {
        Avalonia.Input.Key.W => Key.W,
        Avalonia.Input.Key.A => Key.A,
        Avalonia.Input.Key.S => Key.S,
        Avalonia.Input.Key.D => Key.D,
        Avalonia.Input.Key.Space => Key.Space,
        Avalonia.Input.Key.LeftCtrl => Key.ControlLeft,
        Avalonia.Input.Key.RightCtrl => Key.ControlRight,
        Avalonia.Input.Key.LeftShift => Key.ShiftLeft,
        Avalonia.Input.Key.RightShift => Key.ShiftRight,
        Avalonia.Input.Key.Up => Key.Up,
        Avalonia.Input.Key.Down => Key.Down,
        Avalonia.Input.Key.Left => Key.Left,
        Avalonia.Input.Key.Right => Key.Right,
        _ => null,
    };
}
