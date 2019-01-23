#if TOOLS
using Godot;
using System;

[Tool]
public class PaullozDotInk : EditorPlugin
{
    private const String addonBasePath = "res://addons/paulloz.ink";

    private NodePath customTypeScriptPath = $"{addonBasePath}/Story.cs";
    private NodePath customTypeIconPath = $"{addonBasePath}/icon.svg";

    private NodePath dockScene = $"{addonBasePath}/InkDock.tscn";
    private Control dock;

    public override void _EnterTree()
    {
        AddCustomType("Story", "Node", ResourceLoader.Load(customTypeScriptPath) as Script, ResourceLoader.Load(customTypeIconPath) as Texture);

        dock = (ResourceLoader.Load(dockScene) as PackedScene).Instance() as Control;
        AddControlToDock(EditorPlugin.DockSlot.RightUl, dock);
    }

    public override void _ExitTree()
    {
        RemoveControlFromDocks(dock);

        RemoveCustomType("Story");
    }
}
#endif