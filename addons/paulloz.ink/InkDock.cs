#if TOOLS
using Godot;
using System;

[Tool]
public class InkDock : Control
{
    private Label fileName;
    private Button resetButton;
    private Button loadButton;
    private FileDialog fileDialog;

    private InkStory story;
    private VBoxContainer storyText;
    private VBoxContainer storyChoices;
    
    private ScrollContainer scroll;

    public override void _Ready()
    {
        fileName = GetNode<Label>("Container/Top/Label");
        resetButton = GetNode<Button>("Container/Top/ResetButton");
        loadButton = GetNode<Button>("Container/Top/LoadButton");
        fileDialog = GetNode<FileDialog>("FileDialog");
        storyText = GetNode<VBoxContainer>("Container/Bottom/Scroll/Margin/StoryText");
        storyChoices = GetNode<VBoxContainer>("Container/Bottom/StoryChoices");
        scroll = GetNode<ScrollContainer>("Container/Bottom/Scroll");
        story = GetNode<InkStory>("Story");

        resetButton.Connect("pressed", this, nameof(onResetButtonPressed));
        loadButton.Connect("pressed", this, nameof(onLoadButtonPressed));
        fileDialog.Connect("popup_hide", this, nameof(onFileDialogHide));
        story.Connect(nameof(InkStory.InkChoices), this, nameof(onStoryChoices));
        story.Connect(nameof(InkStory.InkContinued), this, nameof(onStoryContinued));
        story.Connect(nameof(InkStory.InkEnded), this, nameof(onStoryEnded));
    }

    private void resetStoryContent()
    {
        removeAllStoryContent();
        removeAllChoices();
    }

    private void removeAllStoryContent()
    {
        foreach (Node node in storyText.GetChildren()) {
            storyText.RemoveChild(node);
        }
    }

    private void removeAllChoices()
    {
        foreach (Node node in storyChoices.GetChildren()) {
            storyChoices.RemoveChild(node);
        }
    }

    private void onResetButtonPressed()
    {
        resetStoryContent();
        story.LoadStory();
        story.Continue();
    }

    private void onLoadButtonPressed()
    {
        fileDialog.PopupCentered();
    }

    private void onFileDialogHide()
    {
        resetStoryContent();
        fileName.Text = "";
        if (fileDialog.CurrentFile == null || fileDialog.CurrentFile.Length == 0) {
            resetButton.Disabled = true;
        } else {
            resetButton.Disabled = false;
            fileName.Text = fileDialog.CurrentFile;
            story.InkFile = ResourceLoader.Load(fileDialog.CurrentPath);
            story.LoadStory();
            story.Continue();
        }
    }

    private void onStoryContinued(String text, String[] tags)
    {
        text = text.Trim();
        if (text.Length > 0) {
            Label newLine = new Label();
            newLine.Autowrap = true;
            newLine.Text = text;
            addToStory(newLine);
        }

        story.Continue();
    }

    private void onStoryChoices(String[] choices)
    {
        int i = 0;
        foreach (String choice in choices)
        {
            if (i < storyChoices.GetChildCount()) { continue; }
            Button button = new Button();
            button.Text = choice;
            button.Connect("pressed", this, nameof(clickChoice), new Godot.Collections.Array() { i });
            storyChoices.AddChild(button);
            ++i;
        }
    }

    private void onStoryEnded()
    {
        CanvasItem endOfStory = new VBoxContainer();
        endOfStory.AddChild(new HSeparator());
        CanvasItem endOfStoryLine = new HBoxContainer();
        endOfStory.AddChild(endOfStoryLine);
        endOfStory.AddChild(new HSeparator());
        Control separator = new HSeparator();
        separator.SizeFlagsHorizontal = (int)(SizeFlags.Fill | SizeFlags.Expand);
        Label endOfStoryText = new Label();
        endOfStoryText.Text = "End of story";
        endOfStoryLine.AddChild(separator);
        endOfStoryLine.AddChild(endOfStoryText);
        endOfStoryLine.AddChild(separator.Duplicate());
        addToStory(endOfStory);
    }

    private void clickChoice(int idx)
    {
        story.ChooseChoiceIndex(idx);
        removeAllChoices();
        addToStory(new HSeparator());
        story.Continue();
    }

    private async void addToStory(CanvasItem item)
    {
        storyText.AddChild(item);
        await ToSignal(GetTree(), "idle_frame");
        await ToSignal(GetTree(), "idle_frame");
        scroll.ScrollVertical = (int)scroll.GetVScrollbar().MaxValue;
    }
}
#endif