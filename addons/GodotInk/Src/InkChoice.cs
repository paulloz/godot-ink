#nullable enable

using Godot;
using Ink.Runtime;
using System.Collections.Generic;

namespace GodotInk;

#if TOOLS
[Tool]
#endif
public partial class InkChoice : Godot.Object
{
    public string Text => inner.text;
    public string PathStringOnChoice => inner.pathStringOnChoice;
    public string SourcePath => inner.sourcePath;
    public int Index => inner.index;
    public List<string> Tags => inner.tags;

    private readonly Choice inner;

    public InkChoice(Choice inner)
    {
        this.inner = inner;
    }
}
