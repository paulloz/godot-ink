@tool
extends EditorPlugin


var _importer: EditorImportPlugin
var _dock: Control


func _enter_tree() -> void:
	_importer = preload("res://addons/GodotInk/Src/InkStoryImporter.cs").new() as EditorImportPlugin
	add_import_plugin(_importer)

	_dock = preload("res://addons/GodotInk/Src/InkDock.tscn").instantiate() as Control
	add_control_to_bottom_panel(_dock, "Ink preview")
	
	get_editor_interface().get_resource_filesystem().resources_reimported.connect(_when_resources_reimported)


func _exit_tree() -> void:
	get_editor_interface().get_resource_filesystem().resources_reimported.disconnect(_when_resources_reimported)

	remove_control_from_bottom_panel(_dock)
	_dock.free()
	_dock = null

	remove_import_plugin(_importer)
	_importer = null


func _when_resources_reimported(resources : PackedStringArray) -> void:
	if _dock == null:
		return

	for resource in resources:
		if resource.get_extension() != "ink":
			continue

		_dock.call("WhenInkResourceReimported", resource)
