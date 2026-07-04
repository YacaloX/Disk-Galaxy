using Silk.NET.Maths;

namespace DiskGalaxy.Rendering.Camera;

public sealed class Camera
{
    public Vector3D<float> Position { get; set; } = new(0, 5, 20);
    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; } = -15f;
    public float Fov { get; set; } = 60f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 10000f;
    public float MovementSpeed { get; set; } = 15f;
    public float Sensitivity { get; set; } = 0.15f;

    public Vector3D<float> Front { get; private set; } = -Vector3D<float>.UnitZ;
    public Vector3D<float> Right { get; private set; } = Vector3D<float>.UnitX;
    public Vector3D<float> Up { get; private set; } = Vector3D<float>.UnitY;

    private float _aspectRatio = 16f / 9f;

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
}
