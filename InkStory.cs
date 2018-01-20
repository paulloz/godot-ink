using Godot;
using System;
using System.Collections.Generic;
using Ink.Runtime;

public class InkStory : Node
{
    // All the signals we'll need
    public sealed class Signals
    {
        private Signals() { }
        public static readonly String Continued = "ink-continued";
        public static readonly String Ended = "ink-ended";
        public static readonly String Choices = "ink-choices";
        public static readonly String VariableChanged = "ink-variablechanged";
    }

    // All the exported variables
    [Export] public String InkFilePath = null;

    // All the public variables
    public String CurrentText = "";
    public String[] CurrentChoices = { };

    // All the properties
    public bool CanContinue { get { return this.story.canContinue; } }
    public bool HasChoices { get { return this.story.currentChoices.Count > 0; } }

    private Story story = null;
    private List<String> observedVariables = new List<String>();

    public override void _Ready()
    {
        // Register used signals
        AddUserSignal(Signals.Continued);
        AddUserSignal(Signals.Ended);
        AddUserSignal(Signals.Choices);

        String path = String.Format("res://{0}", this.InkFilePath);
        File file = new File();
        try
        {
            if (file.FileExists(path))
            {
                // Load the story
                file.Open(path, (int)File.ModeFlags.Read);
                this.story = new Story(file.GetAsText());
                file.Close();
            }
            else {
                throw new System.IO.FileNotFoundException(String.Format("Unable to find {0}.", path));
            }
        }
        catch (System.IO.FileNotFoundException e)
        {
            // Quit game if the JSON Ink file is not found
            GD.Printerr(e.ToString());
            this.GetTree().Quit();
        }
    }

    public String Continue()
    {
        String text = null;

        // Continue if we can
        if (this.story.canContinue)
        {
            this.story.Continue();
            this.CurrentText = this.story.currentText;
            text = this.CurrentText;

            // Check if we have choices after continuing
            if (this.story.currentChoices.Count > 0)
                this.CurrentChoices = this.story.currentChoices.ConvertAll<String>(choice => choice.text).ToArray();
            else
                this.CurrentChoices = new String[0];

            this.EmitSignal(Signals.Continued, this.CurrentText);
            // this.EmitSignal("InkContinued", "lolilol");
            if (this.CurrentChoices.Length > 0)
                this.EmitSignal(Signals.Choices, new object[] { this.CurrentChoices });
        }
        // If we can't continue and don't have any choice, we're at the end
        else if (this.story.currentChoices.Count <= 0)
            this.EmitSignal(Signals.Ended);
        else {
            this.ChooseChoiceIndex(0);
        }

        return text;
    }

    public void ChooseChoiceIndex(int index)
    {
        if (index >= 0 && index < this.story.currentChoices.Count)
        {
            this.story.ChooseChoiceIndex(index);

            this.Continue();
        }
    }

    public bool ChoosePathString(String pathString)
    {
        try
        {
            this.story.ChoosePathString(pathString);
        }
        catch (Ink.Runtime.StoryException e)
        {
            GD.Printerr(e.ToString());
            return false;
        }

        return true;
    }

    public object GetVariable(String name)
    {
        return this.marshallVariableValue(this.story.variablesState[name]);
    }

    public void SetVariable(String name, object value_)
    {
        this.story.variablesState[name] = value_;
    }

    public String ObserveVariable(String name)
    {
        String signalName = String.Format("{0}-{1}", Signals.VariableChanged, name);

        if (!this.observedVariables.Contains(name))
        {
            AddUserSignal(signalName);
            this.story.ObserveVariable(name, (String varName, object varValue) => {
                this.EmitSignal(signalName, varName, this.marshallVariableValue(varValue));
            });
        }
        return signalName;
    }

    public void BindExternalFunction(String inkFuncName, Func<object> func)
    {
        this.story.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction<T>(String inkFuncName, Func<T, object> func)
    {
        this.story.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction<T1, T2>(String inkFuncName, Func<T1, T2, object> func)
    {
        this.story.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction<T1, T2, T3>(String inkFuncName, Func<T1, T2, T3, object> func)
    {
        this.story.BindExternalFunction(inkFuncName, func);
    }

    public void BindExternalFunction(String inkFuncName, Node node, String funcName)
    {
        this.story.BindExternalFunctionGeneral(inkFuncName, (object[] foo) => node.Call(funcName, foo));
    }

    private object marshallVariableValue(object value_)
    {
        if (value_ != null && value_.GetType() == typeof(Ink.Runtime.InkList))
            value_ = null;
        return value_;
    }
}
