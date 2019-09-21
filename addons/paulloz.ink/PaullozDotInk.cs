#if TOOLS
using Godot;
using System;

[Tool]
public class PaullozDotInk : EditorPlugin
{
    private const String addonBasePath = "res://addons/paulloz.ink";

    private NodePath customTypeScriptPath = $"{addonBasePath}/InkStory.cs";
    private NodePath customTypeIconPath = $"{addonBasePath}/icon.svg";

    private NodePath dockScene = $"{addonBasePath}/InkDock.tscn";
    private Control dock;

    private NodePath importPluginScriptPath = $"{addonBasePath}/import_ink.gd";
    private EditorImportPlugin importPlugin;

    public override void _EnterTree()
    {
        importPlugin = (ResourceLoader.Load(importPluginScriptPath) as GDScript).New() as EditorImportPlugin;
        AddImportPlugin(importPlugin);

        AddCustomType("Ink Story", "Node", ResourceLoader.Load(customTypeScriptPath) as Script, ResourceLoader.Load(customTypeIconPath) as Texture);

        dock = (ResourceLoader.Load(dockScene) as PackedScene).Instance() as Control;
        AddControlToBottomPanel(dock, "Ink");
    }

    public override void _ExitTree()
    {
        RemoveControlFromBottomPanel(dock);
        dock.Free();

        RemoveCustomType("Ink Story");

        RemoveImportPlugin(importPlugin);
    }
}
#endif