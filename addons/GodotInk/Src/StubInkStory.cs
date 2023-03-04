#nullable enable

using Godot;

namespace GodotInk;

public partial class StubInkStory : InkStory
{
    protected override string RawStory
    {
        get => base.RawStory;
        set
        {
#if TOOLS
            if (Engine.IsEditorHint()) return;
#endif
            throw new InvalidInkException(
                "To load this story directly, please import it with 'is_main_file' set to true."
            );
        }
    }
}
