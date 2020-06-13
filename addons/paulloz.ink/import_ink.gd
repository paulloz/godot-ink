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
    return "Resource";

func get_import_options(preset):
    return [
        {"name": "compress", "default_value": true}
    ]

func get_preset_count():
    return 0

func import(source_file, save_path, options, r_platform_variants, r_gen_files):
    match source_file.split(".")[-1].to_lower():
        "ink":
            return import_from_ink(source_file, save_path, options)
        "json":
            return import_from_json(source_file, save_path, options)

func import_from_ink(source_file, save_path, options):
    var setting = "ink/inklecate_path"
    if ProjectSettings.has_setting(setting) and ProjectSettings.property_can_revert(setting):
        var inklecate = ProjectSettings.get_setting(setting)
        var new_file = "%d.json" % int(randf() * 100000)
        var arguments = [
            inklecate, "-o",
            "%s/%s" % [OS.get_user_data_dir(), new_file],
            ProjectSettings.globalize_path(source_file)
        ]

        match OS.get_name():
            "X11", "OSX":
                var _err = OS.execute("mono", arguments, true)
            "Windows":
                arguments.pop_front()
                var _err = OS.execute(inklecate, arguments, true)
            _:
                return null

        new_file = "user://%s" % new_file
        if !File.new().file_exists(new_file):
            return ERR_FILE_UNRECOGNIZED
        var ret = import_from_json(new_file, save_path, options)

        Directory.new().remove(new_file)
        return ret
    else:
        printerr("Please update inklecate_path setting to be able to compile ink files.")
        return ERR_COMPILATION_FAILED

func import_from_json(source_file, save_path, options):
    var raw_content = get_source_file_content(source_file)

    var parsed_content = parse_json(raw_content)
    if !parsed_content.has("inkVersion"):
        return ERR_FILE_UNRECOGNIZED

    var resource = Resource.new()
    resource.set_meta("content", raw_content);

    var flags = ResourceSaver.FLAG_COMPRESS if options["compress"] else 0
    return ResourceSaver.save("%s.%s" % [save_path, get_save_extension()], resource, flags)

func get_source_file_content(source_file):
    var file = File.new()
    var err = file.open(source_file, File.READ)
    if err != OK:
        return err

    var raw_content = file.get_as_text()

    file.close()
    return raw_content
