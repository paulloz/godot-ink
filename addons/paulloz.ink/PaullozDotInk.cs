#if TOOLS
using Godot;
using System;

[Tool]
public class PaullozDotInk : EditorPlugin
{
    private String path = "res://addons/paulloz.ink";

    public override void _EnterTree()
    {
        AddCustomType("Story", "Node", ResourceLoader.Load($"{path}/Story.cs") as Script, ResourceLoader.Load($"{path}/icon.svg") as Texture);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("Story");
    }
}
#endif