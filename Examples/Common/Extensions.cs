using Godot;
using GodotInk.Examples.TeenyTiny;
using System;
using System.Linq;

namespace GodotInk.Examples;

public static class Extensions
{
    // Node

    public static T GetComponent<T>(this Node node, NodePath path)
    where T : class
    {
        return (node.GetNodeOrNull(path) as T)
            ?? throw new Exception($"Component unknown: {typeof(T)} with path {path}");
    }

    public static T GetComponent<T>(this Node node)
    where T : class
    {
        return node.GetChildren().OfType<T>().FirstOrDefault()
            ?? throw new Exception($"Component unknown: {typeof(T)}");
    }

    // TileMap

    public static Interactable? GetInteractableAt(this TileMap map, Vector2I mapPosition)
    {
        Vector2 globalPosition = map.ToGlobal(map.MapToLocal(mapPosition));
        return map.GetChildren().OfType<Interactable>().FirstOrDefault(
            (interactable) => interactable.GlobalPosition == globalPosition
        );
    }

    public static Vector2I GlobalToMap(this TileMap map, Vector2 globalPosition)
    {
        return map.LocalToMap(map.ToLocal(globalPosition));
    }
}
