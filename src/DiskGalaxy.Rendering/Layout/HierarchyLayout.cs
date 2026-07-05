using DiskGalaxy.Rendering.Scene;
using Silk.NET.Maths;

namespace DiskGalaxy.Rendering.Layout;

public sealed class HierarchyLayout
{
    public float DepthSpacing { get; set; } = 2.5f;
    public float BaseRadius { get; set; } = 2f;
    public float RadiusScale { get; set; } = 1.2f;
    public float FolderSizeMultiplier { get; set; } = 1.5f;
    public float FileSizeBase { get; set; } = 0.3f;
    public float MaxNodeSize { get; set; } = 4f;

    private static readonly Random Rng = new(42);

    public void Layout(SceneGraph graph)
    {
        PlaceNode(graph.Root, Vector3D<float>.Zero, 0, BaseRadius);
        graph.Flatten();
        graph.RebuildVisible();
    }

    private void PlaceNode(SceneNode node, Vector3D<float> position, int depth, float radius)
    {
        node.Position = position;
        node.Depth = depth;

        node.Size = node.IsFolder
            ? Math.Min(FolderSizeMultiplier + MathF.Log10(Math.Max(node.ByteSize, 1) + 1) * 0.5f, MaxNodeSize)
            : Math.Min(FileSizeBase + MathF.Log10(Math.Max(node.ByteSize, 1) + 1) * 0.2f, MaxNodeSize * 0.6f);

        if (!node.IsExpanded || node.Children.Count == 0)
            return;

        var childRadius = radius * RadiusScale;
        var childCount = node.Children.Count;

        if (childCount == 1)
        {
            var child = node.Children[0];
            var childPos = new Vector3D<float>(
                position.X + (float)(Rng.NextDouble() - 0.5) * 0.5f,
                position.Y - DepthSpacing,
                position.Z + (float)(Rng.NextDouble() - 0.5) * 0.5f);
            PlaceNode(child, childPos, depth + 1, childRadius);
            return;
        }

        var angleStep = 2f * MathF.PI / childCount;

        for (var i = 0; i < childCount; i++)
        {
            var child = node.Children[i];
            var jitter = (float)(Rng.NextDouble() - 0.5) * 0.3f;
            var angle = i * angleStep + jitter * 0.3f;
            var spreadRadius = childRadius * (0.5f + 0.5f * (float)Rng.NextDouble());

            var childPos = new Vector3D<float>(
                position.X + spreadRadius * MathF.Cos(angle),
                position.Y - DepthSpacing,
                position.Z + spreadRadius * MathF.Sin(angle));

            var childNodeRadius = childRadius * (0.6f + 0.1f * childCount);
            PlaceNode(child, childPos, depth + 1, childNodeRadius);
        }
    }

    public void ToggleExpand(SceneNode node)
    {
        node.IsExpanded = !node.IsExpanded;
    }

    public void ExpandSubtree(SceneNode node)
    {
        node.IsExpanded = true;
        foreach (var child in node.Children)
            ExpandSubtree(child);
    }

    public void CollapseSubtree(SceneNode node)
    {
        foreach (var child in node.Children)
            CollapseSubtree(child);

        if (node.Children.Count > 0)
            node.IsExpanded = false;
    }
}
