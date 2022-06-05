#if TOOLS
using Godot;
using Godot.Collections;

[Tool]
public class PaullozDotInk : EditorPlugin
{
    private readonly Dictionary settings = new Dictionary() {
        {"inklecate_path", new Dictionary() {
            { "type", Variant.Type.String },
            { "hint", PropertyHint.File },
            { "hint_string", OS.GetName() == "OSX" ? "" : "*.exe" },
            { "default", "" }
        }},
        {"marshall_state_variables", new Dictionary() {
            { "type", Variant.Type.Bool },
            { "hint_string", "Enable this if you're going to use state variables from GDScript." },
            { "default", false }
        }}
    };
    private const string addonBasePath = "res://addons/paulloz.ink";

    private readonly NodePath customTypeScriptPath = $"{addonBasePath}/InkPlayer.cs";
    private readonly NodePath customTypeIconPath = $"{addonBasePath}/icon.svg";

    private readonly NodePath dockScene = $"{addonBasePath}/InkDock.tscn";
    private Control dock;

    private readonly NodePath importPluginScriptPath = $"{addonBasePath}/import_ink.gd";
    private EditorImportPlugin importPlugin;

    public override void _EnterTree()
    {
        // Settings
        foreach (string key in settings.Keys)
        {
            string propertyName = $"ink/{key}";
            Dictionary setting = settings[key] as Dictionary;
            if (!ProjectSettings.HasSetting(propertyName))
                ProjectSettings.SetSetting(propertyName, setting["default"]);
            ProjectSettings.AddPropertyInfo(new Dictionary()
            {
                { "name", propertyName },
                { "type", setting["type"] },
                { "hint", setting.Contains("hint") ? setting["hint"] : null },
                { "hint_string", setting.Contains("hint_string") ? setting["hint_string"] : null },
            });
            ProjectSettings.SetInitialValue(propertyName, setting["default"]);
        }
        ProjectSettings.Save();

        // Custom types
        Texture icon = GD.Load<Texture>(customTypeIconPath);
        CSharpScript customTypeScript = GD.Load<CSharpScript>(customTypeScriptPath);
        AddCustomType("InkPlayer", "Node", customTypeScript, icon);

        // Resources
        importPlugin = GD.Load<GDScript>(importPluginScriptPath).New() as EditorImportPlugin;
        AddImportPlugin(importPlugin);

        // Editor
        dock = GD.Load<PackedScene>(dockScene).Instance() as Control;
        AddControlToBottomPanel(dock, "Ink preview");
    }

    public override void _ExitTree()
    {
        // Editor
        RemoveControlFromBottomPanel(dock);
        dock.Free();

        // Resources
        RemoveImportPlugin(importPlugin);
        importPlugin.Free();

        // Custom types
        RemoveCustomType("InkPlayer");

        // Settings
        foreach (string key in settings.Keys)
        {
            string property_name = $"ink/{key}";
            if (ProjectSettings.HasSetting(property_name))
                ProjectSettings.SetSetting(property_name, null);
        }
    }
}
#endif
