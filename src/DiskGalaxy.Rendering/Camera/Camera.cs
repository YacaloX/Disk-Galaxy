using Silk.NET.Maths;

namespace DiskGalaxy.Rendering.Camera;

public sealed class Camera
{
    public Vector3D<float> Position { get; set; } = new(0, 6, 12);
    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; } = -30f;
    public float Fov { get; set; } = 50f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 10000f;
    public float MovementSpeed { get; set; } = 15f;
    public float Sensitivity { get; set; } = 0.15f;

    public Vector3D<float> Front { get; private set; } = -Vector3D<float>.UnitZ;
    public Vector3D<float> Right { get; private set; } = Vector3D<float>.UnitX;
    public Vector3D<float> Up { get; private set; } = Vector3D<float>.UnitY;

    private float _aspectRatio = 16f / 9f;

    private bool _isAnimating;
    private Vector3D<float> _animStartPos;
    private Vector3D<float> _animTargetPos;
    private float _animStartYaw;
    private float _animTargetYaw;
    private float _animStartPitch;
    private float _animTargetPitch;
    private float _animDuration;
    private float _animElapsed;

    public bool IsAnimating => _isAnimating;

    public void StartAnimation(Vector3D<float> targetPos, float targetYaw, float targetPitch, float duration = 0.8f)
    {
        _animStartPos = Position;
        _animTargetPos = targetPos;
        _animStartYaw = Yaw;
        _animTargetYaw = targetYaw;
        _animStartPitch = Pitch;
        _animTargetPitch = targetPitch;
        _animDuration = duration;
        _animElapsed = 0f;
        _isAnimating = true;
    }

    public void UpdateAnimation(float deltaTime)
    {
        if (!_isAnimating) return;

        _animElapsed += deltaTime;
        var t = Math.Clamp(_animElapsed / _animDuration, 0f, 1f);
        var smoothT = t * t * (3f - 2f * t);

        Position = Vector3D.Lerp(_animStartPos, _animTargetPos, smoothT);
        Yaw = _animStartYaw + (_animTargetYaw - _animStartYaw) * smoothT;
        Pitch = _animStartPitch + (_animTargetPitch - _animStartPitch) * smoothT;
        UpdateVectors();

        if (t >= 1f) _isAnimating = false;
    }

    public void SetAspectRatio(float width, float height)
    {
        _aspectRatio = width / height;
    }

    public Matrix4X4<float> GetViewMatrix()
    {
        return Matrix4X4.CreateLookAt(Position, Position + Front, Up);
    }

    public Matrix4X4<float> GetProjectionMatrix()
    {
        return Matrix4X4.CreatePerspectiveFieldOfView(
            Scalar.DegreesToRadians(Fov),
            _aspectRatio,
            NearPlane,
            FarPlane);
    }

    public void UpdateVectors()
    {
        var yawRad = Scalar.DegreesToRadians(Yaw);
        var pitchRad = Scalar.DegreesToRadians(Pitch);

        Front = new Vector3D<float>(
            MathF.Cos(yawRad) * MathF.Cos(pitchRad),
            MathF.Sin(pitchRad),
            MathF.Sin(yawRad) * MathF.Cos(pitchRad));

        Front = Vector3D.Normalize(Front);
        Right = Vector3D.Normalize(Vector3D.Cross(Front, Vector3D<float>.UnitY));
        Up = Vector3D.Normalize(Vector3D.Cross(Right, Front));
    }

    public void MoveForward(float deltaTime, bool fast)
    {
        var speed = fast ? MovementSpeed * 3f : MovementSpeed;
        Position += Front * speed * deltaTime;
    }

    public void MoveBackward(float deltaTime, bool fast)
    {
        var speed = fast ? MovementSpeed * 3f : MovementSpeed;
        Position -= Front * speed * deltaTime;
    }

    public void MoveLeft(float deltaTime, bool fast)
    {
        var speed = fast ? MovementSpeed * 3f : MovementSpeed;
        Position -= Right * speed * deltaTime;
    }

    public void MoveRight(float deltaTime, bool fast)
    {
        var speed = fast ? MovementSpeed * 3f : MovementSpeed;
        Position += Right * speed * deltaTime;
    }

    public void MoveUp(float deltaTime, bool fast)
    {
        var speed = fast ? MovementSpeed * 3f : MovementSpeed;
        Position += Vector3D<float>.UnitY * speed * deltaTime;
    }

    public void MoveDown(float deltaTime, bool fast)
    {
        var speed = fast ? MovementSpeed * 3f : MovementSpeed;
        Position -= Vector3D<float>.UnitY * speed * deltaTime;
    }

    public void Rotate(float deltaX, float deltaY)
    {
        Yaw += deltaX * Sensitivity;
        Pitch = Math.Clamp(Pitch + deltaY * Sensitivity, -89f, 89f);
        UpdateVectors();
    }

    public void Zoom(float offset)
    {
        Fov = Math.Clamp(Fov - offset, 1f, 120f);
    }

    public void FocusOn(Vector3D<float> target, float distance = 10f)
    {
        var dir = Vector3D.Normalize(target - Position);
        var targetPos = target - dir * distance;
        Position = targetPos;
        UpdateVectors();
    }

    public (Vector3D<float> Origin, Vector3D<float> Direction) UnprojectRay(
        float screenX, float screenY, float viewportWidth, float viewportHeight)
    {
        var ndcX = (2f * screenX) / viewportWidth - 1f;
        var ndcY = 1f - (2f * screenY) / viewportHeight;

        var viewProj = GetProjectionMatrix() * GetViewMatrix();
        Matrix4X4.Invert(viewProj, out var invViewProj);

        var near = Vector4D.Transform(new Vector4D<float>(ndcX, ndcY, -1f, 1f), invViewProj);
        var far = Vector4D.Transform(new Vector4D<float>(ndcX, ndcY, 1f, 1f), invViewProj);

        if (Math.Abs(near.W) > float.Epsilon) near /= near.W;
        if (Math.Abs(far.W) > float.Epsilon) far /= far.W;

        var origin = new Vector3D<float>(near.X, near.Y, near.Z);
        var dir = new Vector3D<float>(far.X - near.X, far.Y - near.Y, far.Z - near.Z);

        return (origin, Vector3D.Normalize(dir));
    }
}
