using Godot;
using System;
using System.Collections.Generic;

#if TOOLS
[Tool]
#endif
public class InkStory : Node
{
    private bool shouldMarshallVariables = false;

    [Signal] public delegate void InkContinued(string text, string[] tags);
    [Signal] public delegate void InkEnded();
    [Signal] public delegate void InkChoices(string[] choices);
    [Signal] public delegate void InkError(string message, bool isWarning);
    public delegate void InkVariableChanged(string variableName, object variableValue);

    private string ObservedVariableSignalName(string name)
    {
        return $"{nameof(InkVariableChanged)}-{name}";
    }

    private readonly List<string> observedVariables = new List<string>();
    private Ink.Runtime.Story.VariableObserver observer;

    [Export] public bool AutoLoadStory = false;
    [Export] public Resource InkFile = null;

    public string CurrentText => story?.currentText ?? default;
    public string[] CurrentTags => story?.currentTags.ToArray() ?? Array.Empty<string>();
    public string[] CurrentChoices => story?.currentChoices.ConvertAll(choice => choice.text).ToArray() ?? Array.Empty<string>();
    public bool CanContinue => story?.canContinue ?? false;
    public bool HasChoices => story?.currentChoices.Count > 0;
    public string[] GlobalTags => story?.globalTags?.ToArray() ?? Array.Empty<string>();

    private Ink.Runtime.Story story = null;

    public override void _Ready()
    {
        shouldMarshallVariables = ProjectSettings.HasSetting("ink/marshall_state_variables");

        observer = (string varName, object varValue) =>
        {
            if (observedVariables.Contains(varName))
                EmitSignal(ObservedVariableSignalName(varName), varName, MarshallVariableValue(varValue));
        };

        if (!AutoLoadStory) return;

        LoadStory();
    }

    private void Reset()
    {
        if (story == null) return;

        foreach (string varName in observedVariables)
            RemoveVariableObserver(varName, false);
        observedVariables.Clear();

        story = null;
    }

    /// <summary>
    /// Actually load the story content from <see cref="InkFile"/>.
    /// This method is automatically called in <see cref="_Ready"/> if <see cref="AutoLoadStory"/> is true.
    /// </summary>
    /// <returns>False if there was an error while loading, true otherwise.</returns>
    public bool LoadStory()
    {
        Reset();

        if (!IsJSONFileValid())
        {
            GD.PrintErr("The story you're trying to load is not valid.");
            return false;
        }

        story = new Ink.Runtime.Story(InkFile.GetMeta("content") as string);
        story.onError += OnStoryError;
        return true;
    }

    /// <summary>
    /// Load the story content from <paramref name="story"/>.
    /// </summary>
    /// <param name="story">A string containing the json representation of the story.</param>
    /// <returns>False if there was an error while loading, true otherwise.</returns>
    public bool LoadStoryFromString(string story)
    {
        InkFile = new Resource();
        InkFile.SetMeta("content", story);
        return LoadStory();
    }

    /// <summary>
    /// Call <see cref="LoadStory"/> and set its state to <paramref name="state"/>.
    /// </summary>
    /// <param name="state">A string containing the json representation of a story state.</param>
    /// <returns>False if there was an error while loading, true otherwise.</returns>
    public bool LoadStoryAndSetState(string state)
    {
        if (!LoadStory())
            return false;

        SetState(state);
        return true;
    }

    private bool IsJSONFileValid()
    {
        return InkFile != null && InkFile.HasMeta("content");
    }

    /// <summary>
    /// Continue the story for one line of content.
    /// </summary>
    public string Continue()
    {
        string text = null;

        // Continue if we can
        if (CanContinue)
        {
            story.Continue();
            text = CurrentText;

            EmitSignal(nameof(InkContinued), new object[] { CurrentText, CurrentTags });
            // Check if we have choices after continuing
            if (HasChoices)
                EmitSignal(nameof(InkChoices), new object[] { CurrentChoices });
        }
        // If we can't continue and don't have any choice, we're at the end
        else if (!HasChoices)
            EmitSignal(nameof(InkEnded));

        return text;
    }

    /// <summary>
    /// Choose a choice from the CurrentChoices.
    /// </summary>
    /// <param name="index">The index of the choice to choose from CurrentChoices.</param>
    public void ChooseChoiceIndex(int index)
    {
        if (index < 0 || index >= story?.currentChoices.Count) return;
        story.ChooseChoiceIndex(index);
    }

    /// <summary>
    /// Choose a choice from the CurrentChoices and automatically continue the story for one line of content.
    /// </summary>
    /// <param name="index">The index of the choice to choose from CurrentChoices.</param>
    public string ChooseChoiceIndexAndContinue(int index)
    {
        ChooseChoiceIndex(index);
        return Continue();
    }

    /// <summary>
    /// Change the current position of the story to the given <paramref name="pathString"/>.
    /// </summary>
    /// <param name="pathString">A dot-separated path string.</param>
    /// <returns>False if there was an error during the change, true otherwise.</returns>
    public bool ChoosePathString(string pathString)
    {
        if (story == null)
        {
            try
            {
                story.ChoosePathString(pathString);

                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr(e.ToString());
            }
        }

        return false;
    }

    public void SwitchFlow(string flowName)
    {
        story?.SwitchFlow(flowName);
    }

    public void SwitchToDefaultFlow()
    {
        story?.SwitchToDefaultFlow();
    }

    public void RemoveFlow(string flowName)
    {
        story?.RemoveFlow(flowName);
    }

    public object GetVariable(string name)
    {
        return MarshallVariableValue(story?.variablesState[name]);
    }

    public void SetVariable(string name, object value_)
    {
        if (story == null) return;

        story.variablesState[name] = value_;
    }

    public string ObserveVariable(string name)
    {
        if (story == null) return null;

        string signalName = ObservedVariableSignalName(name);

        if (!observedVariables.Contains(name))
        {
            if (!HasUserSignal(signalName))
                AddUserSignal(signalName);

            observedVariables.Add(name);
            story.ObserveVariable(name, observer);
        }

        return signalName;
    }

    private void RemoveVariableObserver(string name, bool clear)
    {
        if (story == null) return;
        if (!observedVariables.Contains(name)) return;

        string signalName = ObservedVariableSignalName(name);
        if (HasUserSignal(signalName))
        {
            Godot.Collections.Array connections = GetSignalConnectionList(signalName);
            foreach (Godot.Collections.Dictionary connection in connections)
                Disconnect(signalName, connection["target"] as Godot.Object, connection["method"] as string);
            // Seems like there's no way to undo `AddUserSignal` so we're just going to unbind everything :/
        }

        story.RemoveVariableObserver(null, name);

        if (!clear) return;

        observedVariables.Remove(name);
    }

    public void RemoveVariableObserver(string name)
    {
        RemoveVariableObserver(name, true);
    }

    public int VisitCountAtPathString(string pathString)
    {
        return story?.state.VisitCountAtPathString(pathString) ?? 0;
    }

    public void BindExternalFunction(string inkFuncName, Node node, string funcName)
    {
        BindExternalFunction(inkFuncName, node, funcName, false);
    }

    public void BindExternalFunction(string inkFuncName, Node node, string funcName, bool lookaheadSafe)
    {
        story?.BindExternalFunctionGeneral(inkFuncName, (object[] foo) => node.Call(funcName, foo), lookaheadSafe);
    }

    public void BindExternalFunction(string inkFuncName, Func<object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction(string inkFuncName, Func<object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T>(string inkFuncName, Func<T, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T>(string inkFuncName, Func<T, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2>(string inkFuncName, Func<T1, T2, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2>(string inkFuncName, Func<T1, T2, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2, T3>(string inkFuncName, Func<T1, T2, T3, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2, T3>(string inkFuncName, Func<T1, T2, T3, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2, T3, T4>(string inkFuncName, Func<T1, T2, T3, T4, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2, T3, T4>(string inkFuncName, Func<T1, T2, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public object EvaluateFunction(string functionName, bool returnTextOutput, params object[] arguments)
    {
        if (!returnTextOutput)
            return story?.EvaluateFunction(functionName, arguments);

        string textOutput = null;
        object returnValue = story?.EvaluateFunction(functionName, out textOutput, arguments);
        return new object[] { returnValue, textOutput };
    }

    private object MarshallVariableValue(object value_)
    {
        if (!shouldMarshallVariables)
            return value_;

        if (value_ != null && value_.GetType() == typeof(Ink.Runtime.InkList))
            value_ = null;
        return value_;
    }

    public string GetState()
    {
        return story.state.ToJson();
    }

    public void SetState(string state)
    {
        story.state.LoadJson(state);
    }

    public void SaveStateOnDisk(string path)
    {
        if (!path.StartsWith("res://") && !path.StartsWith("user://"))
            path = $"user://{path}";
        File file = new File();
        file.Open(path, File.ModeFlags.Write);
        SaveStateOnDisk(file);
        file.Close();
    }

    public void SaveStateOnDisk(File file)
    {
        if (file.IsOpen())
            file.StoreString(GetState());
    }

    public void LoadStateFromDisk(string path)
    {
        if (!path.StartsWith("res://") && !path.StartsWith("user://"))
            path = $"user://{path}";
        File file = new File();
        file.Open(path, File.ModeFlags.Read);
        LoadStateFromDisk(file);
        file.Close();
    }

    public void LoadStateFromDisk(File file)
    {
        if (file.IsOpen())
        {
            file.Seek(0);
            if (file.GetLen() > 0)
                story.state.LoadJson(file.GetAsText());
        }
    }

    public string[] TagsForContentAtPath(string pathString)
    {
        return story?.TagsForContentAtPath(pathString)?.ToArray() ?? Array.Empty<string>();
    }

    private void OnStoryError(string message, Ink.ErrorType errorType)
    {
        if (errorType == Ink.ErrorType.Author) return;  // This should never happen but eh? What's the cost of checking.
        if (GetSignalConnectionList(nameof(InkError)).Count > 0)
            EmitSignal(nameof(InkError), message, errorType == Ink.ErrorType.Warning);
        else
            GD.PrintErr($"Ink had an error. It is strongly suggested that you connect an error handler to InkError. {message}");
    }
}
