#if TOOLS

#nullable enable

using Godot;
using Godot.Collections;
using Ink;
using System.IO;

namespace GodotInk;

[Tool]
public partial class InkStoryImporter : EditorImportPlugin
{
    private const string OPT_MAIN_FILE = "is_main_file";
    private const string OPT_COMPRESS = "compress";

#pragma warning disable IDE0022
    public override string _GetImporterName() => "ink";

    public override string _GetVisibleName() => "Ink story";

    public override string[] _GetRecognizedExtensions() => new string[] { "ink" };

    public override string _GetResourceType() => "Resource";

    public override string _GetSaveExtension() => "res";

    public override float _GetPriority() => 1.0f;

    public override int _GetPresetCount() => 0;

    public override int _GetImportOrder() => 0;

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => new()
    {
        new() { { "name", OPT_MAIN_FILE }, { "default_value", false } },
        new() { { "name", OPT_COMPRESS }, { "default_value", true } }
    };

    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
#pragma warning restore IDE0022

    public override Error _Import(string sourceFile, string savePath,
                                Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        string destFile = $"{savePath}.{_GetSaveExtension()}";

        if (!options[OPT_MAIN_FILE].AsBool())
            return ResourceSaver.Save(new StubInkStory(), destFile);

        return ImportFromInk(sourceFile, destFile, options[OPT_COMPRESS].AsBool());
    }

    private static Error ImportFromInk(string sourceFile, string destFile, bool shouldCompress)
    {
        using Godot.FileAccess file = Godot.FileAccess.Open(sourceFile, Godot.FileAccess.ModeFlags.Read);

        if (file == null)
            return Godot.FileAccess.GetOpenError();

        Compiler compiler = new(file.GetAsText(), new Compiler.Options
        {
            sourceFilename = sourceFile,
            errorHandler = InkCompilerErrorHandler,
            fileHandler = new FileHandler(
                    Path.GetDirectoryName(file.GetPathAbsolute()) ?? ProjectSettings.GlobalizePath("res://")),
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
