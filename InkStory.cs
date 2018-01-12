using Godot;
using System;
using Ink.Runtime;

public class InkStory : Node
{
    [Export]
    public String InkFilePath = null;
    
    private Story story = null;
    
    public override void _Ready()
    {
        // Register used signals
        AddUserSignal("StoryContinued");
        AddUserSignal("NewChoices");
        AddUserSignal("StoryEnded");

        String path = String.Format("res://{0}", this.InkFilePath);
        File file = new File();
        if (file.FileExists(path))
        {
            // Load the story
            file.Open(path, (int)File.ModeFlags.Read);
            this.story = new Story(file.GetAsText());
            file.Close();
            
            // Start
            this.continueStory();
        }
        else
        {
            throw new System.IO.FileNotFoundException(String.Format("Unable to find {0}.", path));
        }
    }

    private void continueStory()
    {
        if (this.story.canContinue)
        {
            this.EmitSignal("StoryContinued", this.story.currentText);

            if (this.story.currentChoices.Count > 0)
            {
                foreach (Choice choice in this.story.currentChoices)
                {
                }
            }
        }
        else if (this.story.currentChoices.Count <= 0)
        {
            this.EmitSignal("StoryEnded");
        }
    }

    private void hasChoices()
    {
    }

    public void ChooseChoiceIndex(int index)
    {
        if (index > 0 && index < this.story.currentChoices.Count)
        {
            this.story.ChooseChoiceIndex(index);
            
            this.continueStory();
        }
    }

    public override void _Process(float delta)
    {
    }
}
