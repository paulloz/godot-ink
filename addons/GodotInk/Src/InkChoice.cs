#nullable enable

using Godot;
using System.Collections.Generic;

namespace GodotInk;

[Tool]
#if GODOT4_1_OR_GREATER
[GlobalClass]
#endif
public partial class InkChoice : RefCounted
{
    public string Text => inner.text;
    public string PathStringOnChoice => inner.pathStringOnChoice;
    public string SourcePath => inner.sourcePath;
    public int Index => inner.index;
    public IReadOnlyList<string> Tags => inner.tags ?? (IReadOnlyList<string>)System.Array.Empty<string>();

    private readonly Ink.Runtime.Choice inner;

    private InkChoice()
    {
        inner = new Ink.Runtime.Choice();
    }

    public InkChoice(Ink.Runtime.Choice inner)
    {
        this.inner = inner;
    }
}
