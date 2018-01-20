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
    [Export] public Boolean AutoLoadStory = false;
    [Export] public String InkFilePath = null;

    // All the public variables
    public String CurrentText = "";
    public String[] CurrentChoices = { };

    // All the properties
    public bool CanContinue { get { return this.story?.canContinue ?? false; } }
    public bool HasChoices { get { return this.story?.currentChoices.Count > 0; } }

    private Story story = null;
    private List<String> observedVariables = new List<String>();

    public override void _Ready()
    {
        // Register used signals
        AddUserSignal(Signals.Continued);
        AddUserSignal(Signals.Ended);
        AddUserSignal(Signals.Choices);

        if (this.AutoLoadStory)
            this.LoadStory();
    }

    public void LoadStory()
    {
        this.LoadStory(this.InkFilePath);
    }

    public void LoadStory(String inkFilePath)
    {
        try
        {
            if (inkFilePath == null)
                throw new System.IO.FileNotFoundException(String.Format("Unable to find {0}.", null));

            this.InkFilePath = inkFilePath;

            String path = String.Format("res://{0}", this.InkFilePath);
            File file = new File();
            if (file.FileExists(path))
            {
                // Load the story
                file.Open(path, (int)File.ModeFlags.Read);
                this.story = new Story(file.GetAsText());
                file.Close();
            }
            else
                throw new System.IO.FileNotFoundException(String.Format("Unable to find {0}.", path));
        }
        catch (System.IO.FileNotFoundException e)
        {
            GD.Printerr(e.ToString());
        }
    }

    public String Continue()
    {
        String text = null;

        // Continue if we can
        if (this.CanContinue)
        {
            this.story.Continue();
            this.CurrentText = this.story.currentText;
            text = this.CurrentText;

            // Check if we have choices after continuing
            if (this.HasChoices)
                this.CurrentChoices = this.story.currentChoices.ConvertAll<String>(choice => choice.text).ToArray();
            else
                this.CurrentChoices = new String[0];

            this.EmitSignal(Signals.Continued, this.CurrentText);
            if (this.CurrentChoices.Length > 0)
                this.EmitSignal(Signals.Choices, new object[] { this.CurrentChoices });
        }
        // If we can't continue and don't have any choice, we're at the end
        else if (!this.HasChoices)
            this.EmitSignal(Signals.Ended);

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
            GD.Printerr(e.ToString());
            return false;
        }

        return true;
    }

    public int VisitCountAtPathString(String pathString)
    {
        return this.story?.state.VisitCountAtPathString(pathString) ?? 0;
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
        String signalName = String.Format("{0}-{1}", Signals.VariableChanged, name);

        if (this.story != null && !this.observedVariables.Contains(name))
        {
            AddUserSignal(signalName);
            this.story.ObserveVariable(name, (String varName, object varValue) => {
                this.EmitSignal(signalName, varName, this.marshallVariableValue(varValue));
            });

            return signalName;
        }

        return null;
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
}
