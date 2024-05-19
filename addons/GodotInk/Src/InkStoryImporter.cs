#if TOOLS

#nullable enable

using Godot;
using Ink;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using DependenciesCache = System.Collections.Generic.Dictionary<string, string[]>;
using GArrayGictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;
using GArrayString = Godot.Collections.Array<string>;
using Gictionary = Godot.Collections.Dictionary;

namespace GodotInk;

[Tool]
public partial class InkStoryImporter : EditorImportPlugin
{
    private const string OPT_MAIN_FILE = "is_main_file";
    private const string OPT_COMPRESS = "compress";

    private static readonly Regex INCLUDE_REGEX = new(@"^\s*INCLUDE\s*(?<Path>.*)\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

#pragma warning disable IDE0022
    public override string _GetImporterName() => "ink";

    public override string _GetVisibleName() => "Ink story";

    public override string[] _GetRecognizedExtensions() => new string[] { "ink" };

    public override string _GetResourceType() => nameof(Resource);

    public override string _GetSaveExtension() => "res";

    public override float _GetPriority() => 1.0f;

    public override int _GetPresetCount() => 0;

    public override int _GetImportOrder() => 0;

    public override GArrayGictionary _GetImportOptions(string path, int presetIndex) => new()
    {
        new() { { "name", OPT_MAIN_FILE }, { "default_value", false } },
        new() { { "name", OPT_COMPRESS }, { "default_value", true } }
    };

    public override bool _GetOptionVisibility(string path, StringName optionName, Gictionary options) => true;
#pragma warning restore IDE0022

    public override Error _Import(string sourceFile, string savePath, Gictionary options, GArrayString _, GArrayString __)
    {
        UpdateCache(sourceFile, ExtractIncludes(sourceFile));

        string destFile = $"{savePath}.{_GetSaveExtension()}";
        Error returnValue = options[OPT_MAIN_FILE].AsBool()
            ? ImportFromInk(sourceFile, destFile, options[OPT_COMPRESS].AsBool())
            : ResourceSaver.Save(new StubInkStory(), destFile);

        string[] additionalFiles = GetCache().Where(kvp => kvp.Value.Contains(sourceFile)).Select(kvp => kvp.Key).ToArray();
        foreach (string additionalFile in additionalFiles)
            AppendImportExternalResource(additionalFile);

        return returnValue;
    }

    private static Error ImportFromInk(string sourceFile, string destFile, bool shouldCompress)
    {
        using Godot.FileAccess file = Godot.FileAccess.Open(sourceFile, Godot.FileAccess.ModeFlags.Read);

        if (file == null)
            return Godot.FileAccess.GetOpenError();

        Compiler compiler = new(file.GetAsText(), new Compiler.Options
        {
            countAllVisits = true,
            sourceFilename = sourceFile,
            errorHandler = InkCompilerErrorHandler,
            fileHandler = new FileHandler(
                Path.GetDirectoryName(file.GetPathAbsolute()) ?? ProjectSettings.GlobalizePath("res://")
            ),
        });

        try
        {
            string storyContent = compiler.Compile().ToJson();
            InkStory resource = InkStory.Create(storyContent);
            ResourceSaver.SaverFlags flags = shouldCompress ? ResourceSaver.SaverFlags.Compress
                                                            : ResourceSaver.SaverFlags.None;
            return ResourceSaver.Save(resource, destFile, flags);
        }
        catch (InvalidInkException)
        {
            return Error.CompilationFailed;
        }
    }

    private static List<string> ExtractIncludes(string sourceFile)
    {
        using Godot.FileAccess file = Godot.FileAccess.Open(sourceFile, Godot.FileAccess.ModeFlags.Read);
        return INCLUDE_REGEX.Matches(file.GetAsText())
                           .Select(match => sourceFile.GetBaseDir().PathJoin(match.Groups["Path"].Value.TrimEnd('\r')))
                           .ToList();
    }

    private static void InkCompilerErrorHandler(string message, ErrorType errorType)
    {
        switch (errorType)
        {
            case ErrorType.Warning:
                GD.PushWarning(message);
                break;
            case ErrorType.Error:
                GD.PushError(message);
                throw new InvalidInkException();
            case ErrorType.Author:
            default:
                break;
        }
    }

    private const string CACHE_FILE = "user://ink_cache.json";

    public static void UpdateCache(string sourceFile, List<string> dependencies)
    {
        DependenciesCache cache = GetCache();
        cache[sourceFile] = dependencies.ToArray();
        cache = cache.Where(kvp => kvp.Value.Length > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        using Godot.FileAccess file = Godot.FileAccess.Open(CACHE_FILE, Godot.FileAccess.ModeFlags.Write);
        file?.StoreString(JsonSerializer.Serialize(cache));
    }

    public static DependenciesCache GetCache()
    {
        using Godot.FileAccess file = Godot.FileAccess.Open(CACHE_FILE, Godot.FileAccess.ModeFlags.Read);
        try
        {
            return JsonSerializer.Deserialize<DependenciesCache>(file?.GetAsText() ?? "{}")
                ?? new DependenciesCache();
        }
        catch (JsonException)
        {
            return new DependenciesCache();
        }
    }

    private class FileHandler : IFileHandler
    {
        private readonly string rootDir;

        public FileHandler(string rootDir)
        {
            this.rootDir = rootDir;
        }

        public string ResolveInkFilename(string includeName)
        {
            return Path.Combine(rootDir, includeName);
        }

        public string LoadInkFileContents(string fullFilename)
        {
            return File.ReadAllText(fullFilename);
        }
    }
}

#endif
