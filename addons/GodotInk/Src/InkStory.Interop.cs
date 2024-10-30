#nullable enable

using Godot;
using System;
using System.ComponentModel;
using System.Linq;

using InkChoiceGArray = Godot.Collections.Array<GodotInk.InkChoice>;
using StringGArray = Godot.Collections.Array<string>;
using VariantGArray = Godot.Collections.Array<Godot.Variant>;

namespace GodotInk;

public partial class InkStory
{
#pragma warning disable IDE0022
    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CanContinue" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetCanContinue() => CanContinue;

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentChoices" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public InkChoiceGArray GetCurrentChoices() => new(CurrentChoices);

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentTags" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringGArray GetCurrentTags() => new(CurrentTags);

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="GlobalTags" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringGArray GetGlobalTags() => new(GlobalTags);

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="TagsForContentAtPath" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringGArray GetTagsForContentAtPath(string path) => new(TagsForContentAtPath(path));

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentText" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetCurrentText() => CurrentText;

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentWarnings" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringGArray GetCurrentWarnings() => new(CurrentWarnings);

    /// <summary>
    /// This method is here for GDScript compatibility. Use <see cref="CurrentErrors" /> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringGArray GetCurrentErrors() => new(CurrentErrors);
#pragma warning restore IDE0022

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void ChoosePathString(string path)
    {
        ChoosePathString(path, true, Array.Empty<Variant>());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void ChoosePathString(string path, bool resetCallstack)
    {
        ChoosePathString(path, resetCallstack, Array.Empty<Variant>());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void ChoosePathString(string path, VariantGArray arguments)
    {
        ChoosePathString(path, true, arguments.ToArray());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void ChoosePathString(string path, bool resetCallstack, VariantGArray arguments)
    {
        ChoosePathString(path, resetCallstack, arguments.ToArray());
    }

    /// <summary>
    /// Evaluates a function defined in ink.
    /// </summary>
    /// <param name="functionName">The name of the function as declared in ink.</param>
    /// <returns>
    /// The return value as returned from the ink function with `~ return myValue`, or a nil
    /// variant if nothing is returned.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Variant EvaluateFunction(string functionName)
    {
        return EvaluateFunction(functionName, Array.Empty<Variant>());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void BindExternalFunction(string funcName, Callable callable)
    {
        BindExternalFunction(funcName, callable, false);
    }
}
