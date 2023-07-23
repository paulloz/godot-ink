#nullable enable

using System.ComponentModel;

namespace GodotInk;

public partial class InkChoice
{
#pragma warning disable IDE0022
    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CanContinue" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetText() => Text;

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CanContinue" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetPathStringOnChoice() => PathStringOnChoice;

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CanContinue" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetSourcePath() => SourcePath;

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CanContinue" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetIndex() => Index;

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CanContinue" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Godot.Collections.Array<string> GetTags() => new(Tags);
#pragma warning restore IDE0022
}
