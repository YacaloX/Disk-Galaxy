using Silk.NET.Input;
using Silk.NET.Maths;

namespace DiskGalaxy.Rendering.Camera;

public sealed class CameraController
{
    private readonly Camera _camera;
    private bool _isDragging;
    private Vector2D<float> _lastMousePos;
    private readonly HashSet<Key> _pressedKeys = [];

    private static readonly HashSet<Key> ForwardKeys = [Key.W, Key.Up];
    private static readonly HashSet<Key> BackwardKeys = [Key.S, Key.Down];
    private static readonly HashSet<Key> LeftKeys = [Key.A, Key.Left];
    private static readonly HashSet<Key> RightKeys = [Key.D, Key.Right];
    private static readonly Key UpKey = Key.Space;
    private static readonly Key DownKey = Key.ControlLeft;

    public CameraController(Camera camera)
    {
        _camera = camera;
    }

    public void KeyDown(Key key) => _pressedKeys.Add(key);
    public void KeyUp(Key key) => _pressedKeys.Remove(key);

    public void MouseDown(Vector2D<float> pos)
    {
        _isDragging = true;
        _lastMousePos = pos;
    }

    public void MouseUp()
    {
        _isDragging = false;
    }

    public void MouseMove(Vector2D<float> pos)
    {
        if (!_isDragging) return;

        var deltaX = pos.X - _lastMousePos.X;
        var deltaY = pos.Y - _lastMousePos.Y;

        _camera.Rotate(deltaX, deltaY);
        _lastMousePos = pos;
    }

    public void Scroll(float delta)
    {
        _camera.Zoom(delta * 5f);
    }

    public void Update(float deltaTime)
    {
        var fast = _pressedKeys.Contains(Key.ShiftLeft) || _pressedKeys.Contains(Key.ShiftRight);

        if (_pressedKeys.Overlaps(ForwardKeys))
            _camera.MoveForward(deltaTime, fast);
        if (_pressedKeys.Overlaps(BackwardKeys))
            _camera.MoveBackward(deltaTime, fast);
        if (_pressedKeys.Overlaps(LeftKeys))
            _camera.MoveLeft(deltaTime, fast);
        if (_pressedKeys.Overlaps(RightKeys))
            _camera.MoveRight(deltaTime, fast);
        if (_pressedKeys.Contains(UpKey))
            _camera.MoveUp(deltaTime, fast);
        if (_pressedKeys.Contains(DownKey))
            _camera.MoveDown(deltaTime, fast);
    }
}
