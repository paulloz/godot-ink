#nullable enable

using Godot;
using Ink.Runtime;
using System.Collections.Generic;

namespace GodotInk;

#if TOOLS
[Tool]
#endif
public partial class InkChoice : GodotObject
{
    public string Text => inner.text;
    public string PathStringOnChoice => inner.pathStringOnChoice;
    public string SourcePath => inner.sourcePath;
    public int Index => inner.index;
    public List<string> Tags => inner.tags;

    private readonly Choice inner;

    private InkChoice()
    {
        inner = new Choice();
    }

    public InkChoice(Choice inner)
    {
        this.inner = inner;
    }
}
