#nullable enable

using Godot;
using System;
using System.ComponentModel;

namespace GodotInk;

public partial class InkStory
{
    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CanContinue" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetCanContinue() => CanContinue;

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentChoices" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Godot.Collections.Array<InkChoice> GetCurrentChoices() => new(CurrentChoices);

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentTags" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Godot.Collections.Array<string> GetCurrentTags() => new(CurrentTags);

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentText" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetCurrentText() => CurrentText;


    /// <summary>
    /// Evaluates a function defined in ink.
    /// </summary>
    /// <param name="functionName">The name of the function as declared in ink.</param>
    /// <returns>
    /// The return value as returned from the ink function with `~ return myValue`, or a nil
    /// variant if nothing is returned.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Variant EvaluateFunction(string functionName) => EvaluateFunction(functionName, Array.Empty<Variant>());
}
