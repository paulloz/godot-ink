// #if TOOLS
using Godot.Collections;
using Ink;

#nullable enable
namespace Godot.Ink
{
    [Tool]
    public partial class InkStoryImporter : EditorImportPlugin
    {
        public override string _GetImporterName() => "ink";

        public override string _GetVisibleName() => "Ink story";

        public override string[] _GetRecognizedExtensions() => new string[] { "ink" };

        public override string _GetResourceType() => "Resource";

        public override string _GetSaveExtension() => "res";

        public override double _GetPriority() => 1.0;

        public override long _GetPresetCount() => 0;

        public override long _GetImportOrder() => 0;

        public override Array<Dictionary> _GetImportOptions(string path, long presetIndex) => new Array<Dictionary>();

        public override long _Import(string sourceFile, string savePath,
                                     Dictionary options, Array<string> platformVariants, Array<string> genFiles)
        {
            return (long)ImportFromInk(sourceFile, savePath);
        }

        private Error ImportFromInk(string sourceFile, string savePath)
        {
            using (FileAccess file = FileAccess.Open(sourceFile, FileAccess.ModeFlags.Read))
            {
                if (file == null)
                    return FileAccess.GetOpenError();

                Compiler compiler = new Compiler(file.GetAsText(), new Compiler.Options
                {
                    errorHandler = InkCompilerErrorHandler(sourceFile)
                });

                try
                {
                    InkStory resource = InkStory.Create(compiler.Compile().ToJson());

                    return ResourceSaver.Save(resource, $"{savePath}.{_GetSaveExtension()}");
                }
                catch (InvalidInkException)
                {
                    return Error.CompilationFailed;
                }
            }
        }

        private ErrorHandler InkCompilerErrorHandler(string sourceFile)
        {
            System.Func<string, string> formatMessage = (string message) =>
            {
                string[] splittedMessage = message.Split(':');
                splittedMessage[1] = $" {sourceFile}{splittedMessage[1]}";
                return string.Join(':', splittedMessage);
            };

            return (string message, ErrorType errorType) =>
            {
                switch (errorType)
                {
                    case ErrorType.Warning:
                        GD.PushWarning(formatMessage(message));
                        break;
                    case ErrorType.Error:
                        GD.PushError(formatMessage(message));
                        throw new InvalidInkException();
                }
            };
        }

        private class InvalidInkException : System.Exception
        {
            public InvalidInkException() : base()
            {
            }

            public InvalidInkException(string message) : base(message)
            {
            }
        }
    }
}
#nullable restore
// #endif