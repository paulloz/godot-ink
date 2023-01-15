using System;
using System.Collections.Generic;
using System.Linq;
using Ink.Runtime;

#nullable enable
namespace Godot.Ink
{
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

        private Choice inner;

        public InkChoice(Choice inner)
        {
            this.inner = inner;
        }
    }

    #if TOOLS
    [Tool]
    #endif
    public partial class InkStory : Resource
    {
        [ExportCategory("Internal"), ExportGroup("Internal")]
        [Export]
        private string rawStory
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
            InkStory story = new InkStory();
            story.compiledStory = rawStory;
            return story;
        }

        private void InitializeRuntimeStory()
        {
            runtimeStory = new Story(compiledStory);
        }

        public List<InkChoice> CurrentChoices => runtimeStory.currentChoices.Select(choice => new InkChoice(choice)).ToList();

        public string CurrentText => runtimeStory.currentText;

        public List<string> CurrentTags => runtimeStory.currentTags;

        public List<string> CurrentErrors => runtimeStory.currentErrors;

        public List<string> CurrentWarnings => runtimeStory.currentWarnings;

        public string CurrentFlowName => runtimeStory.currentFlowName;

        public bool CurrentFlowIsDefaultFlow => runtimeStory.currentFlowIsDefaultFlow;

        public List<string> AliveFlowNames => runtimeStory.aliveFlowNames;

        public bool HasError => runtimeStory.hasError;

        public bool HasWarning => runtimeStory.hasWarning;

        // TODO:
        public VariablesState VariablesState => throw new NotImplementedException();

        // TODO:
        public ListDefinitionsOrigin ListDefinitions => throw new NotImplementedException();

        // TODO:
        public StoryState State => throw new NotImplementedException();

        // TODO: onError, onDidContinue, onMakeChoice, onEvaluateFunction, onCompleteEvaluateFunction, onChoosePathString

        // TODO:
        public Profiler StartProfiling()
        {
            throw new NotImplementedException();
        }

        // TODO:
        public void EndProfiling()
        {
            throw new NotImplementedException();
        }

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

        // TODO:
        public SearchResult ContentAtPath(Path path) => throw new NotImplementedException();

        // TODO:
        public global::Ink.Runtime.Container KnotContainerWithName(string name) => throw new NotImplementedException();

        // TODO:
        public Pointer PointerAtPath(Path path) => throw new NotImplementedException();

        // TODO:
        public StoryState CopyStateForBackgroundThreadSave() => throw new NotImplementedException();

        public void BackgroundSaveComplete() => runtimeStory.BackgroundSaveComplete();

        public void ChoosePathString(string path, bool resetCallstack, params Variant[] variantArguments)
        {
            object[] arguments = new object[variantArguments.Length];
            for (int i = 0; i < variantArguments.Length; ++i)
                arguments[i] = variantArguments[i].Obj!;

            runtimeStory.ChoosePathString(path, resetCallstack, arguments);
        }

        public void ChoosePathString(string path, params Variant[] variantArguments)
        {
            ChoosePathString(path, true, variantArguments);
        }

        // TODO:
        public void ChoosePath(Path p, bool incrementTurnIndex = true)
        {
            throw new NotImplementedException();
        }

        public void ChooseChoiceIndex(int choiceIdx) => runtimeStory.ChooseChoiceIndex(choiceIdx);

        public bool HasFunction(string functionName) => runtimeStory.HasFunction(functionName);

        // TODO:
        public Variant EvaluateFunction(string functionName, params Variant[] variantArguments)
        {
            throw new NotImplementedException();
        }

        // TODO:
        public Variant EvaluateFunction(string functionName, out string textOutput, params Variant[] variantArguments)
        {
            throw new NotImplementedException();
        }

        // TODO:
        public global::Ink.Runtime.Object EvaluateExpression(global::Ink.Runtime.Container exprContainer)
        {
            throw new NotImplementedException();
        }

        public bool AllowExternalFunctionFallbacks => runtimeStory.allowExternalFunctionFallbacks;

        public void CallExternalFunction(string funcName, int numberOfArguments) => runtimeStory.CallExternalFunction(funcName, numberOfArguments);

        // TODO:
        public void BindExternalFunction(string funcName, Func<object> func, bool lookaheadSafe = false)
        {
            throw new NotImplementedException();
        }

        public void UnbindExternalFunction(string funcName) => runtimeStory.UnbindExternalFunction(funcName);

        public void ValidateExternalBindings() => runtimeStory.ValidateExternalBindings();

        // TODO:
        public void ObserveVariable(string variableName, Story.VariableObserver observer)
        {
            throw new NotImplementedException();
        }

        // TODO:
        public void ObserveVariables(IList<string> variableNames, Story.VariableObserver observer)
        {
            throw new NotImplementedException();
        }

        // TODO:
        public void RemoveVariableObserver(Story.VariableObserver? observer = null, string? specificVariableName = null)
        {
            throw new NotImplementedException();
        }

        public List<string> GlobalTags => runtimeStory.globalTags;

        public List<string> TagsForContentAtPath(string path) => runtimeStory.TagsForContentAtPath(path);

        public void Error(string message) => runtimeStory.Error(message);

        public void Error(string message, bool useEndLineNumber) => runtimeStory.Error(message, useEndLineNumber);

        public void Warning(string message) => runtimeStory.Warning(message);

        // TODO:
        public global::Ink.Runtime.Container MainContentContainter => throw new NotImplementedException();
    }
}
#nullable restore