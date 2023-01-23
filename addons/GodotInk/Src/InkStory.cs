#nullable enable

using Godot;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using static GodotInk.MarshalUtils;

namespace GodotInk;

#if TOOLS
[Tool]
#endif
public partial class InkStory : Resource
{
    [Signal]
    public delegate void ContinuedEventHandler();

    [Signal]
    public delegate void MadeChoiceEventHandler(InkChoice choice);

    [ExportCategory("Internal"), ExportGroup("Internal")]

    [Export]
    private string RawStory
    {
        get => compiledStory;
        set
        {
            compiledStory = value;
            InitializeRuntimeStory();
        }
    }

    private string compiledStory = string.Empty;
    private Story runtimeStory = null!;

    public static InkStory Create(string rawStory)
    {
        return new InkStory()
        {
            compiledStory = rawStory
        };
    }

    private void InitializeRuntimeStory()
    {
        if (runtimeStory != null)
        {
            runtimeStory.onDidContinue -= OnContinued;
            runtimeStory.onMakeChoice -= OnMadeChoice;
        }

        runtimeStory = new Story(compiledStory);

        runtimeStory.onDidContinue += OnContinued;
        runtimeStory.onMakeChoice += OnMadeChoice;
    }

    public List<InkChoice> CurrentChoices => ToVariants(runtimeStory.currentChoices);

    public string CurrentText => runtimeStory.currentText;

    public List<string> CurrentTags => runtimeStory.currentTags;

    public List<string> CurrentErrors => runtimeStory.currentErrors;

    public List<string> CurrentWarnings => runtimeStory.currentWarnings;

    public string CurrentFlowName => runtimeStory.currentFlowName;

    public bool CurrentFlowIsDefaultFlow => runtimeStory.currentFlowIsDefaultFlow;

    public List<string> AliveFlowNames => runtimeStory.aliveFlowNames;

    public bool HasError => runtimeStory.hasError;

    public bool HasWarning => runtimeStory.hasWarning;

    /// <summary>
    /// Reset the Story back to its initial state as it was when it was
    /// first constructed.
    /// </summary>
    public void ResetState()
    {
        runtimeStory.ResetState();
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
    /// <param name="flowName"></param>
    public void RemoveFlow(string flowName)
    {
        runtimeStory.RemoveFlow(flowName);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SwitchToDefaultFlow()
    {
        runtimeStory.SwitchToDefaultFlow();
    }

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
    /// Check whether more content is available if you were to call <c>Continue()</c> - i.e.
    /// are we mid story rather than at a choice point or at the end.
    /// </summary>
    public bool CanContinue => runtimeStory.canContinue;

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

    public void ChoosePathString(string path, bool resetCallstack, params Variant[] arguments)
    {
        runtimeStory.ChoosePathString(path, resetCallstack, FromVariants(arguments));
    }

    public void ChoosePathString(string path, params Variant[] arguments)
    {
        ChoosePathString(path, true, arguments);
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
    /// An ink file can provide a fallback functions for when when an EXTERNAL has been left
    /// unbound by the client, and the fallback function will be called instead. Useful when
    /// testing a story in playmode, when it's not possible to write a client-side C# external
    /// function, but you don't want it to fail to run.
    /// </summary>
    public bool AllowExternalFunctionFallbacks => runtimeStory.allowExternalFunctionFallbacks;

    // TODO: Implement.
    public void BindExternalFunction(string funcName, Func<object> func, bool lookaheadSafe = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove a binding for a named EXTERNAL ink function.
    /// </summary>
    /// <param name="funcName">The name of the EXTERNAL ink function to unbind.</param>
    public void UnbindExternalFunction(string funcName)
    {
        runtimeStory.UnbindExternalFunction(funcName);
    }

    // TODO: Implement.
    public void ObserveVariable(string variableName, Story.VariableObserver observer)
    {
        throw new NotImplementedException();
    }

    // TODO: Implement.
    public void ObserveVariables(IList<string> variableNames, Story.VariableObserver observer)
    {
        throw new NotImplementedException();
    }

    // TODO: Implement.
    public void RemoveVariableObserver(Story.VariableObserver? observer = null, string? specificVariableName = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get any global tags associated with the story. These are defined as
    /// hash tags defined at the very top of the story.
    /// </summary>
    public List<string> GlobalTags => runtimeStory.globalTags;

    /// <summary>
    /// Gets any tags associated with a particular knot or knot.stitch.
    /// These are defined as hash tags defined at the very top of a knot or stitch.
    /// </summary>
    /// <param name="path">The path of the knot or stitch, in the form "knot" or "knot.stitch".</param>
    /// <returns>The list of tags.</returns>
    public List<string> TagsForContentAtPath(string path)
    {
        return runtimeStory.TagsForContentAtPath(path);
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

    private void OnContinued()
    {
        _ = EmitSignal(SignalName.Continued);
    }

    private void OnMadeChoice(Choice choice)
    {
        _ = EmitSignal(SignalName.MadeChoice, new InkChoice(choice));
    }
}
