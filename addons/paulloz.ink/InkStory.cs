using Godot;
using System;
using System.Collections.Generic;

#if TOOLS
[Tool]
#endif
public class InkStory : Node
{
    // All the signals we'll need
    [Signal] public delegate void InkContinued(String text, String[] tags);
    [Signal] public delegate void InkEnded();
    [Signal] public delegate void InkChoices(String[] choices);
    public delegate void InkVariableChanged(String variableName, object variableValue);

    private String ObservedVariableSignalName(String name)
    {
        return $"{nameof(InkVariableChanged)}-{name}";
    }

    // All the exported variables
    [Export] public Boolean AutoLoadStory = false;
    [Export] public TextFile InkFile = null;

    // All the public variables
    public String CurrentText { get { return this.story?.currentText ?? ""; } }
    public String[] CurrentTags { get { return this.story?.currentTags.ToArray() ?? new String[0]; } }
    public String[] CurrentChoices { get { return this.story?.currentChoices.ConvertAll<String>(choice => choice.text).ToArray() ?? new String[0]; } }

    // All the properties
    public bool CanContinue { get { return this.story?.canContinue ?? false; } }
    public bool HasChoices { get { return this.story?.currentChoices.Count > 0; } }
    public String[] GlobalTags { get { return this.story?.globalTags.ToArray() ?? new String[0]; } }

    private Ink.Runtime.Story story = null;
    // private List<String> observedVariables = new List<String>();
    private List<String> observedVariables = new List<String>();
    private Ink.Runtime.Story.VariableObserver observer;

    private void reset()
    {
        if (this.story == null)
            return;

        foreach (String varName in this.observedVariables)
            this.RemoveVariableObserver(varName, false);
        this.observedVariables.Clear();

        this.story =  null;
    }

    public override void _Ready()
    {
        this.observer = (String varName, object varValue) => {
            if (this.observedVariables.Contains(varName))
                this.EmitSignal(this.ObservedVariableSignalName(varName), varName, this.marshallVariableValue(varValue));
        };

        if (this.AutoLoadStory)
            this.LoadStory();
    }

    public Boolean LoadStory()
    {
        this.reset();

        if (!this.isJSONFileValid())
            return false;

        this.story = new Ink.Runtime.Story(this.InkFile.GetMeta("content") as String);
        return true;
    }

    public Boolean LoadStory(String state)
    {
        if (this.LoadStory())
            this.SetState(state);
        else
            return false;
        return true;
    }

    public String Continue()
    {
        String text = null;

        // Continue if we can
        if (this.CanContinue)
        {
            this.story.Continue();
            text = this.CurrentText;

            this.EmitSignal(nameof(InkContinued), new object[] { this.CurrentText, this.CurrentTags });
            if (this.HasChoices) // Check if we have choices after continuing
                this.EmitSignal(nameof(InkChoices), new object[] { this.CurrentChoices });
        }
        else if (!this.HasChoices) // If we can't continue and don't have any choice, we're at the end
            this.EmitSignal(nameof(InkEnded));

        return text;
    }

    public void ChooseChoiceIndex(int index)
    {
        if (index >= 0 && index < this.story?.currentChoices.Count)
        {
            this.story.ChooseChoiceIndex(index);
            this.Continue();
        }
    }

    public bool ChoosePathString(String pathString)
    {
        try
        {
            if (this.story != null)
                this.story.ChoosePathString(pathString);
            else
                return false;
        }
        catch (Ink.Runtime.StoryException e)
        {
            GD.PrintErr(e.ToString());
            return false;
        }

        return true;
    }

    public int VisitCountAtPathString(String pathString)
    {
        return this.story?.state.VisitCountAtPathString(pathString) ?? 0;
    }

    public String[] TagsForContentAtPath(String pathString)
    {
        return this.story?.TagsForContentAtPath(pathString).ToArray() ?? new String[0];
    }

    public object GetVariable(String name)
    {
        return this.marshallVariableValue(this.story?.variablesState[name]);
    }

    public void SetVariable(String name, object value_)
    {
        if (this.story != null)
            this.story.variablesState[name] = value_;
    }

    public String ObserveVariable(String name)
    {
        if (this.story != null)
        {
            String signalName = this.ObservedVariableSignalName(name);

            if (!this.observedVariables.Contains(name))
            {
                if (!this.HasUserSignal(signalName))
                    AddUserSignal(signalName);

                this.observedVariables.Add(name);
                this.story.ObserveVariable(name, this.observer);
            }

            return signalName;
        }

        return null;
    }

    public void RemoveVariableObserver(String name)
    {
        this.RemoveVariableObserver(name, true);
    }

    private void RemoveVariableObserver(String name, Boolean clear)
    {
        if (this.story != null)
        {
            if (this.observedVariables.Contains(name))
            {
                String signalName = this.ObservedVariableSignalName(name);
                if (this.HasUserSignal(signalName))
                {
                    Godot.Collections.Array connections = this.GetSignalConnectionList(signalName);
                    foreach (Godot.Collections.Dictionary connection in connections)
                        this.Disconnect(signalName, connection["target"] as Godot.Object, connection["method"] as String);
                    // Seems like there's no way to undo `AddUserSignal` so we're just going to unbind everything :/
                }

                this.story.RemoveVariableObserver(null, name);

                if (clear)
                    this.observedVariables.Remove(name);
            }
        }
    }

    public void BindExternalFunction(String inkFuncName, Func<object> func)
    {
        this.story?.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction<T>(String inkFuncName, Func<T, object> func)
    {
        this.story?.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction<T1, T2>(String inkFuncName, Func<T1, T2, object> func)
    {
        this.story?.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction<T1, T2, T3>(String inkFuncName, Func<T1, T2, T3, object> func)
    {
        this.story?.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction(String inkFuncName, Node node, String funcName)
    {
        this.story?.BindExternalFunctionGeneral(inkFuncName, (object[] foo) => node.Call(funcName, foo));
    }

    private object marshallVariableValue(object value_)
    {
        if (value_ != null && value_.GetType() == typeof(Ink.Runtime.InkList))
            value_ = null;
        return value_;
    }

    public String GetState()
    {
        return this.story.state.ToJson();
    }

    public void SaveStateOnDisk(String path)
    {
        if (!path.StartsWith("res://") && !path.StartsWith("user://"))
            path = $"user://{path}";
        File file = new File();
        file.Open(path, (int)File.ModeFlags.Write);
        this.SaveStateOnDisk(file);
        file.Close();
    }

    public void SaveStateOnDisk(File file)
    {
        if (file.IsOpen())
            file.StoreString(this.GetState());
    }

    public void SetState(String state)
    {
        this.story.state.LoadJson(state);
    }

    public void LoadStateFromDisk(String path)
    {
        if (!path.StartsWith("res://") && !path.StartsWith("user://"))
            path = $"user://{path}";
        File file = new File();
        file.Open(path, (int)File.ModeFlags.Read);
        this.LoadStateFromDisk(file);
        file.Close();
    }

    public void LoadStateFromDisk(File file)
    {
        if (file.IsOpen())
        {
            file.Seek(0);
            if (file.GetLen() > 0)
                this.story.state.LoadJson(file.GetAsText());
        }
    }

    private Boolean isJSONFileValid()
    {
        return this.InkFile != null && this.InkFile.HasMeta("content");
    }
}
