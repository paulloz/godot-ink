@tool
extends EditorPlugin


var _importer: EditorImportPlugin
var _dock: Control


func _enter_tree() -> void:
	_importer = preload("res://addons/GodotInk/Src/InkStoryImporter.cs").new() as EditorImportPlugin
	add_import_plugin(_importer)

	_dock = preload("res://addons/GodotInk/Src/InkDock.tscn").instantiate() as Control
	add_control_to_bottom_panel(_dock, "Ink preview")


func _exit_tree() -> void:
	remove_control_from_bottom_panel(_dock)
	_dock.free()

	remove_import_plugin(_importer)
	_importer = null
