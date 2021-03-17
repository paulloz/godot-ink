using Godot;
using System;
using System.Collections.Generic;

#if TOOLS
[Tool]
#endif
public class InkStory : Node
{
#region Settings
    private Boolean shouldMarshallVariables = false;
#endregion

#region Signals
    [Signal] public delegate void InkContinued(String text, String[] tags);
    [Signal] public delegate void InkEnded();
    [Signal] public delegate void InkChoices(String[] choices);
    public delegate void InkVariableChanged(String variableName, object variableValue);

    private String ObservedVariableSignalName(String name)
    {
        return $"{nameof(InkVariableChanged)}-{name}";
    }

    private List<String> observedVariables = new List<String>();
    private Ink.Runtime.Story.VariableObserver observer;
#endregion

#region Exports
    [Export] public Boolean AutoLoadStory = false;
    [Export] public Resource InkFile = null;
#endregion

#region Public ink properties
    public String CurrentText { get { return story?.currentText ?? default(String); } }
    public String[] CurrentTags { get { return story?.currentTags.ToArray() ?? new String[0]; } }
    public String[] CurrentChoices { get { return story?.currentChoices.ConvertAll<String>(choice => choice.text).ToArray() ?? new String[0]; } }
    public bool CanContinue { get { return story?.canContinue ?? false; } }
    public bool HasChoices { get { return story?.currentChoices.Count > 0; } }
    public String[] GlobalTags { get { return story?.globalTags?.ToArray() ?? new String[0]; } }
#endregion

#region Initialisation
    private Ink.Runtime.Story story = null;

    public override void _Ready()
    {
        shouldMarshallVariables = ProjectSettings.HasSetting("ink/marshall_state_variables");
        
        observer = (String varName, object varValue) => {
            if (observedVariables.Contains(varName)) {
                EmitSignal(ObservedVariableSignalName(varName), varName, marshallVariableValue(varValue));
            }
        };

        if (AutoLoadStory) {
            LoadStory();
        }
    }

    private void reset()
    {
        if (story == null)
            return;

        foreach (String varName in observedVariables) {
            RemoveVariableObserver(varName, false);
        }
        observedVariables.Clear();

        story =  null;
    }
#endregion

#region Story loading
    public Boolean LoadStory()
    {
        reset();

        if (!isJSONFileValid())
        {
            GD.PrintErr("The story you're trying to load is not valid.");
            return false;
        }

        story = new Ink.Runtime.Story(InkFile.GetMeta("content") as String);
        return true;
    }

    public Boolean LoadStoryFromString(String story)
    {
        InkFile = new Resource();
        InkFile.SetMeta("content", story);
        return LoadStory();
    }

    public Boolean LoadStoryAndSetState(String state)
    {
        if (!LoadStory()) {
            return false;
        }
        SetState(state);
        return true;
    }

    private Boolean isJSONFileValid()
    {
        return InkFile != null && InkFile.HasMeta("content");
    }
#endregion

#region Story flow
    public String Continue()
    {
        String text = null;

        // Continue if we can
        if (CanContinue)
        {
            story.Continue();
            text = CurrentText;

            EmitSignal(nameof(InkContinued), new object[] { CurrentText, CurrentTags });
            if (HasChoices) {  // Check if we have choices after continuing
                EmitSignal(nameof(InkChoices), new object[] { CurrentChoices });
            }
        }
        else if (!HasChoices) {  // If we can't continue and don't have any choice, we're at the end
            EmitSignal(nameof(InkEnded));
        }

        return text;
    }

    public void ChooseChoiceIndex(int index)
    {
        if (index >= 0 && index < story?.currentChoices.Count) {
            story.ChooseChoiceIndex(index);
        }
    }

    public String ChooseChoiceIndexAndContinue(int index)
    {
        ChooseChoiceIndex(index);
        return Continue();
    }

    public bool ChoosePathString(String pathString)
    {
        if (story != null) {
            try {
                story.ChoosePathString(pathString);

                return true;
            } catch (Exception e) {
                GD.PrintErr(e.ToString());
            }
        }

        return false;
    }
#endregion

#region Ink variables
    public object GetVariable(String name)
    {
        return marshallVariableValue(story?.variablesState[name]);
    }

    public void SetVariable(String name, object value_)
    {
        if (story != null) {
            story.variablesState[name] = value_;
        }
    }

    public String ObserveVariable(String name)
    {
        if (story != null)
        {
            String signalName = ObservedVariableSignalName(name);

            if (!observedVariables.Contains(name))
            {
                if (!HasUserSignal(signalName)) {
                    AddUserSignal(signalName);
                }

                observedVariables.Add(name);
                story.ObserveVariable(name, observer);
            }

            return signalName;
        }

        return null;
    }

    private void RemoveVariableObserver(String name, Boolean clear)
    {
        if (story != null)
        {
            if (observedVariables.Contains(name))
            {
                String signalName = ObservedVariableSignalName(name);
                if (HasUserSignal(signalName))
                {
                    Godot.Collections.Array connections = GetSignalConnectionList(signalName);
                    foreach (Godot.Collections.Dictionary connection in connections) {
                        Disconnect(signalName, connection["target"] as Godot.Object, connection["method"] as String);
                    }
                    // Seems like there's no way to undo `AddUserSignal` so we're just going to unbind everything :/
                }

                story.RemoveVariableObserver(null, name);

                if (clear) {
                    observedVariables.Remove(name);
                }
            }
        }
    }

    public void RemoveVariableObserver(String name)
    {
        RemoveVariableObserver(name, true);
    }

    public int VisitCountAtPathString(String pathString)
    {
        return story?.state.VisitCountAtPathString(pathString) ?? 0;
    }
#endregion

#region Ink external functions
    public void BindExternalFunction(String inkFuncName, Func<object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction(String inkFuncName, Func<object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T>(String inkFuncName, Func<T, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T>(String inkFuncName, Func<T, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2>(String inkFuncName, Func<T1, T2, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2>(String inkFuncName, Func<T1, T2, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2, T3>(String inkFuncName, Func<T1, T2, T3, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2, T3>(String inkFuncName, Func<T1, T2, T3, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2, T3, T4>(String inkFuncName, Func<T1, T2, T3, T4, object> func, bool lookaheadSafe)
    {
        story?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2, T3, T4>(String inkFuncName, Func<T1, T2, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction(String inkFuncName, Node node, String funcName, bool lookaheadSafe)
    {
        story?.BindExternalFunctionGeneral(inkFuncName, (object[] foo) => node.Call(funcName, foo), lookaheadSafe);
    }

    public void BindExternalFunction(String inkFuncName, Node node, String funcName)
    {
        BindExternalFunction(inkFuncName, node, funcName, false);
    }

    public object EvaluateFunction(String functionName, Boolean returnTextOutput, params object [] arguments)
    {
        if (returnTextOutput)
        {
            String textOutput = null;
            object returnValue = story?.EvaluateFunction(functionName, out textOutput, arguments);
            return new object[] { returnValue, textOutput };
        }
        return story?.EvaluateFunction(functionName, arguments);
    }

    private object marshallVariableValue(object value_)
    {
        if (!shouldMarshallVariables) {
            return value_;
        }

        if (value_ != null && value_.GetType() == typeof(Ink.Runtime.InkList)) {
            value_ = null;
        }
        return value_;
    }
#endregion

#region Ink states
    public String GetState()
    {
        return story.state.ToJson();
    }

    public void SetState(String state)
    {
        story.state.LoadJson(state);
    }

    public void SaveStateOnDisk(String path)
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
        if (file.IsOpen()) {
            file.StoreString(GetState());
        }
    }

    public void LoadStateFromDisk(String path)
    {
        if (!path.StartsWith("res://") && !path.StartsWith("user://")) {
            path = $"user://{path}";
        }
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
            if (file.GetLen() > 0) {
                story.state.LoadJson(file.GetAsText());
            }
        }
    }
#endregion

#region Ink tags
    public String[] TagsForContentAtPath(String pathString)
    {
        return story?.TagsForContentAtPath(pathString).ToArray() ?? new String[0];
    }
#endregion
}
