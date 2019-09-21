tool
extends EditorImportPlugin

func get_importer_name():
    return "inkjson";

func get_visible_name():
    return "JSON ink story";

func get_recognized_extensions():
    return [ "json" ];

func get_save_extension():
    return "res";

func get_resource_type():
    return "TextFile";

func get_import_options(preset):
    return []

func get_preset_count():
    return 0

func import(source_file, save_path, options, r_platform_variants, r_gen_files):
    var file = File.new()
    var err = file.open(source_file, File.READ)
    if err != OK:
        return err
    var raw_content = file.get_as_text()
    file.close()

    var parsed_content = parse_json(raw_content)
    if !parsed_content.has("inkVersion"):
        return ERR_FILE_UNRECOGNIZED

    var resource = TextFile.new()
    resource.set_meta("content", raw_content);
    return ResourceSaver.save("%s.%s" % [save_path, get_save_extension()], resource)
