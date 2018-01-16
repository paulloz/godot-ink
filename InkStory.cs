using Godot;
using System;
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
    }

    // All the exported variables
    [Export] public String InkFilePath = null;
    [Export] public String CurrentText = "";
    
    private Story story = null;
    
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
            //if (this.story.currentChoices.Count > 0)
                //this.EmitSignal(Signals.Choices, this.story.currentChoices.ToArray());
        }
        // If we can't continue and don't have any choice, we're at the end
        else if (this.story.currentChoices.Count <= 0)
            this.EmitSignal(Signals.Ended);
    }

    public void ChooseChoiceIndex(int index)
    {
        if (index > 0 && index < this.story.currentChoices.Count)
        {
            this.story.ChooseChoiceIndex(index);
            
            this.Continue();
        }
    }
}
