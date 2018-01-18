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

    public void Continue()
    {
        // Continue if we can
        if (this.story.canContinue)
        {
            this.story.Continue();
            this.CurrentText = this.story.currentText;
            this.EmitSignal(Signals.Continued, this.CurrentText);

            // Check if we have choices after continuing
            if (this.story.currentChoices.Count > 0)
            {
                this.CurrentChoices = this.story.currentChoices.ConvertAll<String>(choice => choice.text).ToArray();
                this.EmitSignal(Signals.Choices, this.CurrentChoices);
            }
        }
        // If we can't continue and don't have any choice, we're at the end
        else if (this.story.currentChoices.Count <= 0)
            this.EmitSignal(Signals.Ended);
    }

    public void ChooseChoiceIndex(int index)
    {
        if (index >= 0 && index < this.story.currentChoices.Count)
        {
            this.story.ChooseChoiceIndex(index);

            this.Continue();
        }
    }

    public object GetVariable(String name)
    {
        object value_ = this.story.variablesState[name];
        if (value_.GetType() == typeof(Ink.Runtime.InkList))
        {

        }
        return value_;
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
                this.EmitSignal(signalName, varName, varValue);
            });
        }
        return signalName;
    }
}
