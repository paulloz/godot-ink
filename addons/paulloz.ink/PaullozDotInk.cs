#if TOOLS
using Godot;
using Godot.Collections;
using System;

[Tool]
public class PaullozDotInk : EditorPlugin
{
    private Dictionary settings = new Dictionary() { {"inklecate_path", "---"} };
    private const String addonBasePath = "res://addons/paulloz.ink";

    private NodePath customTypeScriptPath = $"{addonBasePath}/InkStory.cs";
    private NodePath customTypeIconPath = $"{addonBasePath}/icon.svg";

    private NodePath dockScene = $"{addonBasePath}/InkDock.tscn";
    private Control dock;

    private NodePath importPluginScriptPath = $"{addonBasePath}/import_ink.gd";
    private EditorImportPlugin importPlugin;

    public override void _EnterTree()
    {
        // Settings
        foreach (String key in settings.Keys)
        {
            String property_name = $"ink/{key}";
            if (!ProjectSettings.HasSetting(property_name))
            {
                ProjectSettings.SetSetting(property_name, settings[key]);
                ProjectSettings.SetInitialValue(property_name, settings[key]);
            }
        }
        ProjectSettings.Save();

        // Resources
        importPlugin = (ResourceLoader.Load(importPluginScriptPath) as GDScript).New() as EditorImportPlugin;
        AddImportPlugin(importPlugin);

        // Custom types
        AddCustomType("Ink Story", "Node", ResourceLoader.Load(customTypeScriptPath) as Script, ResourceLoader.Load(customTypeIconPath) as Texture);

        // Editor
        dock = (ResourceLoader.Load(dockScene) as PackedScene).Instance() as Control;
        AddControlToBottomPanel(dock, "Ink");
    }

    public override void _ExitTree()
    {
        // Editor
        RemoveControlFromBottomPanel(dock);
        dock.Free();

        // Custom types
        RemoveCustomType("Ink Story");

        // Resources
        RemoveImportPlugin(importPlugin);

        // Settings
        foreach (String key in settings.Keys)
        {
            String property_name = $"ink/{key}";
            if (ProjectSettings.HasSetting(property_name))
                ProjectSettings.SetSetting(property_name, null);
        }
    }
}
#endif