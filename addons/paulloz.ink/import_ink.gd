tool
extends EditorImportPlugin

func get_importer_name() -> String:
	return "ink";

func get_visible_name() -> String:
	return "Ink story";

func get_recognized_extensions() -> Array:
	return [ "json", "ink" ];

func get_save_extension() -> String:
	return "res";

func get_resource_type() -> String:
	return "Resource";

func get_import_options(preset: int) -> Array:
	return [
		{"name": "is_master_file", "default_value": true},
		{"name": "compress", "default_value": true}
	]

func get_option_visibility(option: String, options: Dictionary) -> bool:
	return true

func get_preset_count() -> int:
	return 0

func import(source_file: String, save_path: String, options: Dictionary, r_platform_variants: Array, r_gen_files: Array) -> int:
	match source_file.split(".")[-1].to_lower():
		"ink":
			return import_from_ink(source_file, save_path, options)
		"json":
			return import_from_json(source_file, save_path, options)
	return OK

func import_from_ink(source_file: String, save_path: String, options: Dictionary) -> int:
	var inklecate: String = _fetch_inklecate()

	if inklecate == "":
		printerr("Please update inklecate_path setting to be able to compile ink files.")
		return ERR_COMPILATION_FAILED
	if !File.new().file_exists(inklecate):
		printerr("File does not exist: '%s'." % inklecate)
		return ERR_COMPILATION_FAILED

	var new_file: String = "%d.json" % int(randf() * 100000)
	var arguments: Array = [
		"-o",
		"%s/%s" % [OS.get_user_data_dir(), new_file],
		ProjectSettings.globalize_path(source_file)
	]

	if not options["is_master_file"]:
		return _save_resource(save_path, Resource.new(), options)

	var _err: int = OK
	var _output: Array = []
	match OS.get_name():
		"OSX", "Windows", "X11":
			_err = OS.execute(inklecate, arguments, true, _output)
		_:
			return ERR_COMPILATION_FAILED
	if _err != OK:
		printerr(_output[0])
		return ERR_COMPILATION_FAILED

	new_file = "user://%s" % new_file
	if !File.new().file_exists(new_file):
		return ERR_FILE_UNRECOGNIZED
	var ret: int = import_from_json(new_file, save_path, options)

	Directory.new().remove(new_file)
	return ret

func import_from_json(source_file: String, save_path: String, options: Dictionary) -> int:
	var raw_content: String = get_source_file_content(source_file)

	var parsed_content: Dictionary = parse_json(raw_content)
	if !parsed_content.has("inkVersion"):
		return ERR_FILE_UNRECOGNIZED

	var resource: Resource = Resource.new()
	resource.set_meta("content", raw_content)

	return _save_resource(save_path, resource, options)

func get_source_file_content(source_file: String) -> String:
	var file: File = File.new()
	var err: int = file.open(source_file, File.READ)
	if err != OK:
		return ""

	var raw_content: String = file.get_as_text()

	file.close()
	return raw_content

func _save_resource(save_path: String, resource: Resource, options: Dictionary):
	var flags: int = ResourceSaver.FLAG_COMPRESS if options["compress"] else 0
	return ResourceSaver.save("%s.%s" % [save_path, get_save_extension()], resource, flags)

func _fetch_inklecate() -> String:
	var inklecate_setting: String = "ink/inklecate_path"
	var override_setting: String = "res://addons/paulloz.ink/override.cfg"
	var inklecate: String = ""

	if ProjectSettings.has_setting(inklecate_setting) and ProjectSettings.property_can_revert(inklecate_setting):
		inklecate = ProjectSettings.globalize_path(String(ProjectSettings.get_setting(inklecate_setting)))

	
	var override: ConfigFile = ConfigFile.new()
	if override.load(override_setting) == OK:
		inklecate = override.get_value("", inklecate_setting.split("/")[1], inklecate)

	return inklecate
