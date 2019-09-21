tool
extends EditorImportPlugin

func get_importer_name():
    return "ink";

func get_visible_name():
    return "Ink story";

func get_recognized_extensions():
    return [ "json", "ink" ];

func get_save_extension():
    return "res";

func get_resource_type():
    return "TextFile";

func get_import_options(preset):
    return []

func get_preset_count():
    return 0

func import(source_file, save_path, options, r_platform_variants, r_gen_files):
    match source_file.split(".")[-1].to_lower():
        "ink":
            return import_from_ink(source_file, save_path)
        "json":
            return import_from_json(source_file, save_path)

func import_from_ink(source_file, save_path):
    if ProjectSettings.has_setting("ink/inklecate_path"):
        var inklecate = ProjectSettings.get_setting("ink/inklecate_path")
        if inklecate != "---":
            var new_file = "%d.json" % int(randf() * 100000)

            var err = OS.execute(inklecate, [
                "-o", "%s/%s" % [OS.get_user_data_dir(), new_file],
                ProjectSettings.globalize_path(source_file)
            ], true)

            new_file = "user://%s" % new_file
            if !File.new().file_exists(new_file):
                return ERR_FILE_UNRECOGNIZED
            var ret = import_from_json(new_file, save_path)

            Directory.new().remove(new_file)
            return ret

func import_from_json(source_file, save_path):
    var raw_content = get_source_file_content(source_file)

    var parsed_content = parse_json(raw_content)
    if !parsed_content.has("inkVersion"):
        return ERR_FILE_UNRECOGNIZED

    var resource = TextFile.new()
    resource.set_meta("content", raw_content);

    return ResourceSaver.save("%s.%s" % [save_path, get_save_extension()], resource)

func get_source_file_content(source_file):
    var file = File.new()
    var err = file.open(source_file, File.READ)
    if err != OK:
        return err

    var raw_content = file.get_as_text()

    file.close()
    return raw_content