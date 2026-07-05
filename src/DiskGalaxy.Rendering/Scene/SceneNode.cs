using Silk.NET.Maths;

namespace DiskGalaxy.Rendering.Scene;

public sealed class SceneNode
{
    public required string Label { get; init; }
    public required string FullPath { get; init; }
    public Vector3D<float> Position { get; set; }
    public Vector3D<float> Color { get; set; } = new(0.5f, 0.5f, 0.5f);
    public float Size { get; set; } = 0.5f;
    public bool IsFolder { get; set; }
    public bool IsExpanded { get; set; } = true;
    public int Depth { get; set; }
    public long ByteSize { get; set; }
    public SceneNode? Parent { get; set; }
    public List<SceneNode> Children { get; init; } = [];
    public object? Data { get; set; }
    public int Index { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsHovered { get; set; }
    public bool IsSelected { get; set; }
    public Vector3D<float> BaseColor { get; set; }

    public int SubtreeSize
    {
        get
        {
            var count = 1;
            foreach (var child in Children)
                count += child.SubtreeSize;
            return count;
        }
    }
}
