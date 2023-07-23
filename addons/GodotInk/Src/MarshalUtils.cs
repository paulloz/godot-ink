#nullable enable

using Godot;
using System;
using System.Collections.Generic;

namespace GodotInk;

public static class MarshalUtils
{
    /// <summary>
    /// Convert from an ink variable type to a Godot Variant.
    /// </summary>
    /// <param name="obj">The ink variable to convert.</param>
    /// <returns>The converted Godot Variant.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Variant ToVariant(object? obj)
    {
        return obj switch
        {
            bool b => Variant.CreateFrom(b),

            int n => Variant.CreateFrom(n),
            float n => Variant.CreateFrom(n),

            string str => Variant.CreateFrom(str),

            Ink.Runtime.Choice choice => new InkChoice(choice),
            Ink.Runtime.InkList list => new InkList(list),

            null => new Variant(),

            _ => throw new ArgumentException($"Argument of type {obj.GetType()} is not valid."),
        };
    }

    /// <summary>
    /// Convert from a Godot Variant to an ink variable.
    /// </summary>
    /// <param name="variant">The Godot Variant to convert.</param>
    /// <returns>The converted ink variable.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static object? FromVariant(Variant variant)
    {
        return variant.VariantType switch
        {
            Variant.Type.Bool => variant.AsBool(),

            Variant.Type.Int => variant.AsInt32(),
            Variant.Type.Float => variant.AsSingle(),

            Variant.Type.String => variant.AsString(),

            Variant.Type.Nil => null,

            _ => throw new ArgumentException($"Argument of type {variant.Obj?.GetType()} is not valid."),
        };
    }

    /// <summary>
    /// Convert from a collection of Godot Variants to a collection of ink variables. The
    /// collection doesn't need to be homogeneous in type.
    /// </summary>
    /// <param name="objects">The collection of Godot Variants to convert.</param>
    /// <returns>The collection of converted ink variables.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Variant[] ToVariants(IReadOnlyList<object?> objects)
    {
        Variant[] variants = new Variant[objects.Count];
        for (int i = 0; i < objects.Count; ++i)
            variants[i] = ToVariant(objects[i]);
        return variants;
    }

    /// <summary>
    /// Convert from a collection of ink variables to a collection of Godot Variants. The
    /// collection doesn't need to be homogeneous in type.
    /// </summary>
    /// <param name="objects">The collection of ink variables to convert.</param>
    /// <returns>The collection of converted Godot Variants.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static object?[] FromVariants(IReadOnlyList<Variant> variants)
    {
        object?[] objects = new object[variants.Count];
        for (int i = 0; i < variants.Count; ++i)
            objects[i] = FromVariant(variants[i]);
        return objects;
    }

    // Specific case for List<Choice>, we want to stay true to the ink implementation and return
    // a List<T> instead of a T[]. So we're converting to a List<InkChoice>.
    public static List<InkChoice> ToVariants(IReadOnlyList<Ink.Runtime.Choice> choices)
    {
        List<InkChoice> inkChoices = new();
        foreach (Ink.Runtime.Choice choice in choices)
            inkChoices.Add((InkChoice)ToVariant(choice));
        return inkChoices;
    }
}
