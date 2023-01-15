#nullable enable

using Godot;
using Ink.Runtime;
using System;
using System.Collections.Generic;

namespace GodotInk;

#if TOOLS
[Tool]
#endif
public partial class InkStory : Resource
{
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
        runtimeStory = new Story(compiledStory);
    }

    public List<InkChoice> CurrentChoices => WrapChoices(runtimeStory.currentChoices);

    public string CurrentText => runtimeStory.currentText;

    public List<string> CurrentTags => runtimeStory.currentTags;

    public List<string> CurrentErrors => runtimeStory.currentErrors;

    public List<string> CurrentWarnings => runtimeStory.currentWarnings;

    public string CurrentFlowName => runtimeStory.currentFlowName;

    public bool CurrentFlowIsDefaultFlow => runtimeStory.currentFlowIsDefaultFlow;

    public List<string> AliveFlowNames => runtimeStory.aliveFlowNames;

    public bool HasError => runtimeStory.hasError;

    public bool HasWarning => runtimeStory.hasWarning;

    // TODO: Implement.
    public VariablesState VariablesState => throw new NotImplementedException();

    // TODO: Implement.
    public ListDefinitionsOrigin ListDefinitions => throw new NotImplementedException();

    // TODO: Implement.
    public StoryState State => throw new NotImplementedException();

    // TODO: Implement onError, onDidContinue, onMakeChoice, onEvaluateFunction, onCompleteEvaluateFunction,
    // onChoosePathString.

    // TODO: Implement.
    public Profiler StartProfiling() => throw new NotImplementedException();

    // TODO: Implement.
    public void EndProfiling() => throw new NotImplementedException();

    public void ResetState() => runtimeStory.ResetState();

    public void ResetCallstack() => runtimeStory.ResetCallstack();

    public void SwitchFlow(string flowName) => runtimeStory.SwitchFlow(flowName);

    public void RemoveFlow(string flowName) => runtimeStory.RemoveFlow(flowName);

    public void SwitchToDefaultFlow() => runtimeStory.SwitchToDefaultFlow();

    public string Continue() => runtimeStory.Continue();

    public bool CanContinue => runtimeStory.canContinue;

    public bool AsyncContinueComplete => runtimeStory.asyncContinueComplete;

    public void ContinueAsync(float millisecsLimitAsync) => runtimeStory.ContinueAsync(millisecsLimitAsync);

    public string ContinueMaximally() => runtimeStory.ContinueMaximally();

    // TODO: Implement.
    public SearchResult ContentAtPath(Path path) => throw new NotImplementedException();

    // TODO: Implement.
    public Ink.Runtime.Container KnotContainerWithName(string name) => throw new NotImplementedException();

    // TODO: Implement.
    public Pointer PointerAtPath(Path path) => throw new NotImplementedException();

    // TODO: Implement.
    public StoryState CopyStateForBackgroundThreadSave() => throw new NotImplementedException();

    public void BackgroundSaveComplete() => runtimeStory.BackgroundSaveComplete();

    public void ChoosePathString(string path, bool resetCallstack, params Variant[] arguments)
    {
        runtimeStory.ChoosePathString(path, resetCallstack, VariantsToInkObjects(arguments));
    }

    public void ChoosePathString(string path, params Variant[] arguments)
    {
        ChoosePathString(path, true, arguments);
    }

    // TODO: Implement.
    public void ChoosePath(Path p, bool incrementTurnIndex = true) => throw new NotImplementedException();

    public void ChooseChoiceIndex(int choiceIdx) => runtimeStory.ChooseChoiceIndex(choiceIdx);

    public bool HasFunction(string functionName) => runtimeStory.HasFunction(functionName);

    public Variant EvaluateFunction(string functionName, params Variant[] arguments)
    {
        return InkObjectToVariant(runtimeStory.EvaluateFunction(functionName, VariantsToInkObjects(arguments)));
    }

    public Variant EvaluateFunction(string functionName, out string textOutput, params Variant[] arguments)
    {
        return InkObjectToVariant(runtimeStory.EvaluateFunction(functionName, out textOutput, VariantsToInkObjects(arguments)));
    }

    // TODO: Implement.
    public Ink.Runtime.Object EvaluateExpression(Ink.Runtime.Container exprContainer) => throw new NotImplementedException();

    public bool AllowExternalFunctionFallbacks => runtimeStory.allowExternalFunctionFallbacks;

    public void CallExternalFunction(string funcName, int numberOfArguments) => runtimeStory.CallExternalFunction(funcName, numberOfArguments);

    // TODO: Implement.
    public void BindExternalFunction(string funcName, Func<object> func, bool lookaheadSafe = false) => throw new NotImplementedException();

    public void UnbindExternalFunction(string funcName) => runtimeStory.UnbindExternalFunction(funcName);

    public void ValidateExternalBindings() => runtimeStory.ValidateExternalBindings();

    // TODO: Implement.
    public void ObserveVariable(string variableName, Story.VariableObserver observer) => throw new NotImplementedException();

    // TODO: Implement.
    public void ObserveVariables(IList<string> variableNames, Story.VariableObserver observer) => throw new NotImplementedException();

    // TODO: Implement.
    public void RemoveVariableObserver(Story.VariableObserver? observer = null, string? specificVariableName = null) => throw new NotImplementedException();

    public List<string> GlobalTags => runtimeStory.globalTags;

    public List<string> TagsForContentAtPath(string path) => runtimeStory.TagsForContentAtPath(path);

    public void Error(string message) => runtimeStory.Error(message);

    public void Error(string message, bool useEndLineNumber) => runtimeStory.Error(message, useEndLineNumber);

    public void Warning(string message) => runtimeStory.Warning(message);

    // TODO: Implement.
    public Ink.Runtime.Container MainContentContainter => throw new NotImplementedException();

    private static Variant InkObjectToVariant(object? obj)
    {
        return obj switch
        {
            bool b => Variant.CreateFrom(b),

            int n => Variant.CreateFrom(n),
            float n => Variant.CreateFrom(n),

            string str => Variant.CreateFrom(str),

            null => new Variant(),

            _ => throw new ArgumentException($"Argument of type {obj.GetType()} is not valid."),
        };
    }

    private static object? VariantToInkObject(Variant variant)
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

    private static Variant[] InkObjectsToVariants(object?[] objects)
    {
        Variant[] variants = new Variant[objects.Length];
        for (int i = 0; i < variants.Length; ++i)
            variants[i] = InkObjectToVariant(objects[i]);
        return variants;
    }

    private static object?[] VariantsToInkObjects(Variant[] variants)
    {
        object?[] objects = new object[variants.Length];
        for (int i = 0; i < variants.Length; ++i)
            objects[i] = VariantToInkObject(variants[i]);
        return objects;
    }

    private static List<InkChoice> WrapChoices(List<Choice> choices)
    {
        List<InkChoice> inkChoices = new();
        foreach (Choice choice in choices)
            inkChoices.Add(new InkChoice(choice));
        return inkChoices;
    }
}
