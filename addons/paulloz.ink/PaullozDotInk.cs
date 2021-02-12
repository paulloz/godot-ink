#if TOOLS
using Godot;
using Godot.Collections;
using System;

[Tool]
public class PaullozDotInk : EditorPlugin
{
    private Dictionary settings = new Dictionary() {
        {"inklecate_path", new Dictionary() {
            { "type", Variant.Type.String },
            { "hint", PropertyHint.GlobalFile },
            { "hint_string", OS.GetName() == "OSX" ? "" : ".exe" },
            { "default", "" }
        }},
        {"marshall_state_variables", new Dictionary() {
            { "type", Variant.Type.Bool },
            { "hint_string", "Enable this if you're going to use state variables from GDScript." },
            { "default", false }
        }}
    };
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
            String propertyName = $"ink/{key}";
            Dictionary setting = settings[key] as Dictionary;
            if (!ProjectSettings.HasSetting(propertyName))
                ProjectSettings.SetSetting(propertyName, setting["default"]);
            ProjectSettings.AddPropertyInfo(new Dictionary() {
                { "name", propertyName },
                { "type", setting["type"] },
                { "hint", setting.Contains("hint") ? setting["hint"] : null },
                { "hint_string", setting.Contains("hint_string") ? setting["hint_string"] : null },
            });
            ProjectSettings.SetInitialValue(propertyName, setting["default"]);
        }
        ProjectSettings.Save();

        // Resources
        importPlugin = GD.Load<GDScript>(importPluginScriptPath).New() as EditorImportPlugin;
        AddImportPlugin(importPlugin);

        // Custom types
        AddCustomType("Ink Story", "Node", GD.Load<Script>(customTypeScriptPath), GD.Load<Texture>(customTypeIconPath));

        // Editor
        dock = GD.Load<PackedScene>(dockScene).Instance() as Control;
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
