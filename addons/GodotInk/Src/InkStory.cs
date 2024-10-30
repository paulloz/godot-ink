#nullable enable

using Godot;
using System;
using System.Collections.Generic;

using static GodotInk.MarshalUtils;

using PropertyList = Godot.Collections.Array<Godot.Collections.Dictionary>;

namespace GodotInk;

[Tool]
#if GODOT4_1_OR_GREATER
[GlobalClass, Icon("../GodotInk.svg")]
#endif
public partial class InkStory : Resource
{
    [Signal]
    public delegate void ContinuedEventHandler();

    [Signal]
    public delegate void MadeChoiceEventHandler(InkChoice choice);

    [Signal]
    public delegate void AuthorErrorEventHandler(string message);

    [Signal]
    public delegate void WarningErrorEventHandler(string message);

    [Signal]
    public delegate void ErrorErrorEventHandler(string message);

    protected virtual string RawStory
    {
        get => rawStory;
        set
        {
            rawStory = value;
#if TOOLS
            // There's really no need to instantiate Ink.Runtime in the editor itself.
            // if (Engine.IsEditorHint()) return; <- Commenting for now, because it prevents
            //                                       the InkDock from running stories.
#endif
            InitializeRuntimeStory();
        }
    }

    private string rawStory = string.Empty;
    private Ink.Runtime.Story runtimeStory = null!;

    private readonly Dictionary<string, HashSet<Callable>> observers = new();
    private readonly Dictionary<string, Ink.Runtime.Story.VariableObserver> internalObservers = new();

    public static InkStory Create(string rawStory)
    {
        return new InkStory()
        {
            rawStory = rawStory
        };
    }

    private void InitializeRuntimeStory()
    {
        if (runtimeStory != null)
        {
            runtimeStory.onDidContinue -= OnContinued;
            runtimeStory.onMakeChoice -= OnMadeChoice;
            runtimeStory.onError -= OnError;
        }

        runtimeStory = new Ink.Runtime.Story(rawStory);

        runtimeStory.onDidContinue += OnContinued;
        runtimeStory.onMakeChoice += OnMadeChoice;
        runtimeStory.onError += OnError;
    }

    public string CurrentText => runtimeStory.currentText;

    public IReadOnlyList<InkChoice> CurrentChoices => ToVariants(runtimeStory.currentChoices);

    public IReadOnlyList<string> CurrentTags => runtimeStory.currentTags;

    public bool HasWarning => runtimeStory.hasWarning;

    public IReadOnlyList<string> CurrentWarnings => runtimeStory.currentWarnings;

    public bool HasError => runtimeStory.hasError;

    public IReadOnlyList<string> CurrentErrors => runtimeStory.currentErrors;

    /// <summary>
    /// Check whether more content is available if you were to call <c>Continue()</c> - i.e.
    /// are we mid-story rather than at a choice point or at the end.
    /// </summary>
    public bool CanContinue => runtimeStory.canContinue;

    /// <summary>
    /// Continue the story for one line of content, if possible.
    /// If you're not sure if there's more content available, for example if you
    /// want to check whether you're at a choice point or at the end of the story,
    /// you should call <c>canContinue</c> before calling this function.
    /// </summary>
    /// <returns>The line of text content.</returns>
    public string Continue()
    {
        return runtimeStory.Continue();
    }

    /// <summary>
    /// Continue the story until the next choice point or until it runs out of content.
    /// This is as opposed to the Continue() method which only evaluates one line of
    /// output at a time.
    /// </summary>
    /// <returns>The resulting text evaluated by the ink engine, concatenated together.</returns>
    public string ContinueMaximally()
    {
        return runtimeStory.ContinueMaximally();
    }

    /// <summary>
    /// Chooses the Choice from the currentChoices list with the given
    /// index. Internally, this sets the current content path to that
    /// pointed to by the Choice, ready to continue story evaluation.
    /// </summary>
    /// <param name="choiceIdx">The index of the choice to choose.</param>
    public void ChooseChoiceIndex(int choiceIdx)
    {
        runtimeStory.ChooseChoiceIndex(choiceIdx);
    }

    public void ChoosePathString(string path, bool resetCallstack = true, params Variant[] arguments)
    {
        runtimeStory.ChoosePathString(path, resetCallstack, FromVariants(arguments));
    }

    /// <summary>
    /// Unwinds the callstack. Useful to reset the Story's evaluation
    /// without actually changing any meaningful state, for example if
    /// you want to exit a section of story prematurely and tell it to
    /// go elsewhere with a call to ChoosePathString(...).
    /// Doing so without calling ResetCallstack() could cause unexpected
    /// issues if, for example, the Story was in a tunnel already.
    /// </summary>
    public void ResetCallstack()
    {
        runtimeStory.ResetCallstack();
    }

    /// <summary>
    /// Reset the Story back to its initial state as it was when it was
    /// first constructed.
    /// </summary>
    public void ResetState()
    {
        runtimeStory.ResetState();
    }

    /// <summary>
    /// Get any global tags associated with the story. These are defined as
    /// hashtags defined at the very top of the story.
    /// </summary>
    public IReadOnlyList<string> GlobalTags => runtimeStory.globalTags;

    /// <summary>
    /// Gets any tags associated with a particular knot or knot.stitch.
    /// These are defined as hash tags defined at the very top of a knot or stitch.
    /// </summary>
    /// <param name="path">The path of the knot or stitch, in the form "knot" or "knot.stitch".</param>
    /// <returns>The list of tags.</returns>
    public IReadOnlyList<string> TagsForContentAtPath(string path)
    {
        return runtimeStory.TagsForContentAtPath(path);
    }

    public string CurrentFlowName => runtimeStory.currentFlowName;

    public bool CurrentFlowIsDefaultFlow => runtimeStory.currentFlowIsDefaultFlow;

    public IReadOnlyList<string> AliveFlowNames => runtimeStory.aliveFlowNames;

    /// <summary>
    ///
    /// </summary>
    /// <param name="flowName"></param>
    public void RemoveFlow(string flowName)
    {
        runtimeStory.RemoveFlow(flowName);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="flowName"></param>
    public void SwitchFlow(string flowName)
    {
        runtimeStory.SwitchFlow(flowName);
    }

    /// <summary>
    ///
    /// </summary>
    public void SwitchToDefaultFlow()
    {
        runtimeStory.SwitchToDefaultFlow();
    }

    public int VisitCountAtPathString(string pathString)
    {
        return runtimeStory.state.VisitCountAtPathString(pathString);
    }

    public Variant FetchVariable(string variableName)
    {
        return ToVariant(runtimeStory.variablesState[variableName]);
    }

    public T FetchVariable<[MustBeVariant] T>(string variableName)
    {
        return FetchVariable(variableName).As<T>();
    }

    public void StoreVariable(string variableName, Variant value)
    {
        runtimeStory.variablesState[variableName] = FromVariant(value);
    }

    public void StoreVariable<[MustBeVariant] T>(string variableName, T value)
    {
        StoreVariable(variableName, Variant.From(value));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="observer"></param>
    public void ObserveVariable(string variableName, Callable observer)
    {
        if (!internalObservers.ContainsKey(variableName))
        {
            Ink.Runtime.Story.VariableObserver internalObserver = BuildObserver();
            runtimeStory.ObserveVariable(variableName, internalObserver);
            internalObservers[variableName] = internalObserver;
        }

        if (observers.TryGetValue(variableName, out HashSet<Callable>? observerSet))
            observerSet.Add(observer);
        else
            observers[variableName] = new HashSet<Callable>() { observer };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="variableNames"></param>
    /// <param name="observer"></param>
    public void ObserveVariable(string[] variableNames, Callable observer)
    {
        foreach (string variableName in variableNames)
            ObserveVariable(variableName, observer);
    }

    private Ink.Runtime.Story.VariableObserver BuildObserver()
    {
        return delegate (string name, object? value)
        {
            if (!observers.TryGetValue(name, out HashSet<Callable>? observerSet)) return;

            Variant variant = ToVariant(value);
            foreach (Callable callable in observerSet)
                _ = callable.Call(name, variant);
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="callable"></param>
    public void RemoveVariableObserver(Callable callable)
    {
        foreach (string variableName in observers.Keys)
            RemoveVariableObserver(callable, variableName);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="specificVariableName"></param>
    public void RemoveVariableObserver(string specificVariableName)
    {
        runtimeStory.RemoveVariableObserver(null, specificVariableName);
        _ = internalObservers.Remove(specificVariableName);
        _ = observers.Remove(specificVariableName);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="callable"></param>
    /// <param name="specificVariableName"></param>
    public void RemoveVariableObserver(Callable callable, string specificVariableName)
    {
        var callables = observers[specificVariableName];
        if (!callables.Contains(callable)) return;

        _ = callables.Remove(callable);
        if (callables.Count > 0) return;

        runtimeStory.RemoveVariableObserver(null, specificVariableName);
        _ = internalObservers.Remove(specificVariableName);
    }

    /// <summary>
    /// An ink file can provide a fallback functions for when an EXTERNAL has been left
    /// unbound by the client, and the fallback function will be called instead. Useful when
    /// testing a story in play mode, when it's not possible to write a client-side C# external
    /// function, but you don't want it to fail to run.
    /// </summary>
    public bool AllowExternalFunctionFallbacks => runtimeStory.allowExternalFunctionFallbacks;

    /// <summary>
    /// Checks if a function exists.
    /// </summary>
    /// <param name="functionName">The name of the function as declared in ink.</param>
    /// <returns>True if the function exists, else false.</returns>
    public bool HasFunction(string functionName)
    {
        return runtimeStory.HasFunction(functionName);
    }

    /// <summary>
    /// Evaluates a function defined in ink.
    /// </summary>
    /// <param name="functionName">The name of the function as declared in ink.</param>
    /// <param name="arguments">
    /// The arguments that the ink function takes, if any. Note that we don't (can't) do any
    /// validation on the number of arguments right now, so make sure you get it right!
    /// </param>
    /// <returns>
    /// The return value as returned from the ink function with `~ return myValue`, or a nil
    /// variant if nothing is returned.
    /// </returns>
    public Variant EvaluateFunction(string functionName, params Variant[] arguments)
    {
        object? result = runtimeStory.EvaluateFunction(functionName, FromVariants(arguments));
        return ToVariant(result);
    }

    /// <summary>
    /// Evaluates a function defined in ink.
    /// </summary>
    /// <param name="functionName">The name of the function as declared in ink.</param>
    /// <param name="arguments">
    /// The arguments that the ink function takes, if any. Note that we don't (can't) do any
    /// validation on the number of arguments right now, so make sure you get it right!
    /// </param>
    /// <returns>
    /// The return value as returned from the ink function with `~ return myValue`, or a nil
    /// variant if nothing is returned.
    /// </returns>
    public Variant EvaluateFunction(string functionName, Godot.Collections.Array<Variant> arguments)
    {
        object? result = runtimeStory.EvaluateFunction(functionName, FromVariants(arguments));
        return ToVariant(result);
    }

    /// <summary>
    /// Evaluates a function defined in ink, and gathers the possibly multi-line text as generated
    /// by the function. This text output is any text written as normal content within the function,
    /// as opposed to the return value, as returned with `~ return`.
    /// </summary>
    /// <param name="functionName">The name of the function as declared in ink.</param>
    /// <param name="textOutput">The text content produced by the function via normal ink, if any.</param>
    /// <param name="arguments">
    /// The arguments that the ink function takes, if any. Note that we don't (can't) do any
    /// validation on the number of arguments right now, so make sure you get it right!
    /// </param>
    /// <returns>
    /// The return value as returned from the ink function with `~ return myValue`, or a nil
    /// variant if nothing is returned.
    /// </returns>
    public Variant EvaluateFunction(string functionName, out string textOutput, params Variant[] arguments)
    {
        object? result = runtimeStory.EvaluateFunction(functionName, out textOutput, FromVariants(arguments));
        return ToVariant(result);
    }

    /// <summary>
    /// Bind a C# function to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="callable">The Godot Callable to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction(string funcName, Callable callable, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunctionGeneral(funcName, Trampoline, lookaheadSafe);

        return;

        object? Trampoline(object?[] arguments) => FromVariant(callable.Call(ToVariants(arguments)));
    }

    /// <summary>
    /// Bind a C# function to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="func">The C# function to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction(string funcName, Func<Variant> func, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, Trampoline, lookaheadSafe);

        return;

        object? Trampoline() => FromVariant(func.Invoke());
    }

    /// <summary>
    /// Bind a C# function to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="func">The C# function to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T>(string funcName, Func<T, Variant> func, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, (Func<T, object?>)Trampoline, lookaheadSafe);

        return;

        object? Trampoline(T a) => FromVariant(func.Invoke(a));
    }

    /// <summary>
    /// Bind a C# function to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="func">The C# function to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T1, T2>(string funcName, Func<T1, T2, Variant> func, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, (Func<T1, T2, object?>)Trampoline, lookaheadSafe);

        return;

        object? Trampoline(T1 a, T2 b) => FromVariant(func.Invoke(a, b));
    }

    /// <summary>
    /// Bind a C# function to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="func">The C# function to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T1, T2, T3>(string funcName, Func<T1, T2, T3, Variant> func, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, (Func<T1, T2, T3, object?>)Trampoline, lookaheadSafe);

        return;

        object? Trampoline(T1 a, T2 b, T3 c) => FromVariant(func.Invoke(a, b, c));
    }

    /// <summary>
    /// Bind a C# function to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="func">The C# function to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T1, T2, T3, T4>(string funcName, Func<T1, T2, T3, T4, Variant> func, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, (Func<T1, T2, T3, T4, object?>)Trampoline, lookaheadSafe);

        return;

        object? Trampoline(T1 a, T2 b, T3 c, T4 d) => FromVariant(func.Invoke(a, b, c, d));
    }

    /// <summary>
    /// Bind a C# Action to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="action">The C# action to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction(string funcName, Action action, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, action, lookaheadSafe);
    }

    /// <summary>
    /// Bind a C# Action to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="action">The C# action to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T>(string funcName, Action<T> action, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, action, lookaheadSafe);
    }

    /// <summary>
    /// Bind a C# Action to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="action">The C# action to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T1, T2>(string funcName, Action<T1, T2> action, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, action, lookaheadSafe);
    }

    /// <summary>
    /// Bind a C# Action to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="action">The C# action to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T1, T2, T3>(string funcName, Action<T1, T2, T3> action, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, action, lookaheadSafe);
    }

    /// <summary>
    /// Bind a C# Action to an ink EXTERNAL function declaration.
    /// </summary>
    /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
    /// <param name="action">The C# action to bind.</param>
    /// <param name="lookaheadSafe">The ink engine often evaluates further
    /// than you might expect beyond the current line just in case it sees
    /// glue that will cause the two lines to become one. In this case it's
    /// possible that a function can appear to be called twice instead of
    /// just once, and earlier than you expect. If it's safe for your
    /// function to be called in this way (since the result and side effect
    /// of the function will not change), then you can pass 'true'.
    /// Usually, you want to pass 'false', especially if you want some action
    /// to be performed in game code when this function is called.</param>
    public void BindExternalFunction<T1, T2, T3, T4>(string funcName, Action<T1, T2, T3, T4> action, bool lookaheadSafe = false)
    {
        runtimeStory.BindExternalFunction(funcName, action, lookaheadSafe);
    }

    /// <summary>
    /// Remove a binding for a named EXTERNAL ink function.
    /// </summary>
    /// <param name="funcName">The name of the EXTERNAL ink function to unbind.</param>
    public void UnbindExternalFunction(string funcName)
    {
        runtimeStory.UnbindExternalFunction(funcName);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public void Error(string message)
    {
        runtimeStory.Error(message);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <param name="useEndLineNumber"></param>
    public void Error(string message, bool useEndLineNumber)
    {
        runtimeStory.Error(message, useEndLineNumber);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public void Warning(string message)
    {
        runtimeStory.Warning(message);
    }

    /// <summary>
    /// Save the current story state a JSON string.
    /// </summary>
    /// <returns>The current state serialized into a JSON string.</returns>
    public string SaveState()
    {
        return runtimeStory.state.ToJson();
    }

    /// <summary>
    /// Save the current story state to a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the file we will be writing to.</param>
    public void SaveStateFile(string filePath)
    {
        using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(SaveState());
    }

    /// <summary>
    /// Load a JSON string as the current story state.
    /// </summary>
    /// <param name="jsonState">The JSON string to load.</param>
    public void LoadState(string jsonState)
    {
        runtimeStory.state.LoadJson(jsonState);
    }

    /// <summary>
    /// Load the content of a JSON file as the current story state.
    /// </summary>
    /// <param name="filePath">The path to the file we will be reading from.</param>
    public void LoadStateFile(string filePath)
    {
        using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        LoadState(file.GetAsText());
    }

    private void OnContinued()
    {
        _ = EmitSignal(SignalName.Continued);
    }

    private void OnMadeChoice(Ink.Runtime.Choice choice)
    {
        _ = EmitSignal(SignalName.MadeChoice, new InkChoice(choice));
    }

    private void OnError(string message, Ink.ErrorType type)
    {
        switch (type)
        {
            case Ink.ErrorType.Author:
                _ = EmitSignal(SignalName.AuthorError, message);
                break;
            case Ink.ErrorType.Warning:
                _ = EmitSignal(SignalName.WarningError, message);
                break;
            case Ink.ErrorType.Error:
                _ = EmitSignal(SignalName.ErrorError, message);
                break;
            default:
                return;
        }
    }

    public override PropertyList _GetPropertyList()
    {
        PropertyList properties = base._GetPropertyList() ?? new PropertyList();

        properties.Add(new Godot.Collections.Dictionary()
        {
            { "name", PropertyName.RawStory },
            { "type", Variant.From(Variant.Type.Object) },
            { "usage", Variant.From(PropertyUsageFlags.NoEditor) },
        });

        return properties;
    }
}
