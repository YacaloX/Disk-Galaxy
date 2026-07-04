using Silk.NET.Maths;

namespace DiskGalaxy.Rendering.Scene;

public sealed class SceneGraph
{
    public SceneNode Root { get; }
    public List<SceneNode> AllNodes { get; } = [];
    public List<SceneNode> VisibleNodes { get; } = [];
    public List<(Vector3D<float> Start, Vector3D<float> End)> Edges { get; } = [];

    public SceneGraph(SceneNode root)
    {
        Root = root;
    }

    public void Flatten()
    {
        AllNodes.Clear();
        FlattenNode(Root);
    }

    public void RebuildVisible()
    {
        VisibleNodes.Clear();
        Edges.Clear();
        CollectVisible(Root, null);
    }

    public SceneNode? FindNode(string path)
    {
        return AllNodes.Find(n => n.FullPath == path);
    }

    public SceneNode? PickNode(Vector3D<float> rayOrigin, Vector3D<float> rayDir, float maxDist = 100f)
    {
        SceneNode? closest = null;
        var closestDist = maxDist;

        foreach (var node in VisibleNodes)
        {
            var diff = node.Position - rayOrigin;
            var t = Vector3D.Dot(diff, rayDir);
            if (t < 0) continue;

            var proj = rayOrigin + rayDir * t;
            var dist = Vector3D.Distance(proj, node.Position);
            var hitRadius = node.Size * 1.5f;

            if (dist < hitRadius && t < closestDist)
            {
                closest = node;
                closestDist = t;
            }
        }

        return closest;
    }

    private void FlattenNode(SceneNode node)
    {
        node.Index = AllNodes.Count;
        AllNodes.Add(node);
        foreach (var child in node.Children)
            FlattenNode(child);
    }

    private void CollectVisible(SceneNode node, SceneNode? parent)
    {
        VisibleNodes.Add(node);

        if (parent is not null)
        {
            Edges.Add((parent.Position, node.Position));
        }

        if (node.IsExpanded)
        {
            foreach (var child in node.Children)
                CollectVisible(child, node);
        }
    }
}
