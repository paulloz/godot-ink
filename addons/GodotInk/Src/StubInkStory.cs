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
            if (Engine.IsEditorHint())
                return;
#endif
            throw new Ink.Runtime.StoryException("Not imported as main file.");
        }
    }
}
