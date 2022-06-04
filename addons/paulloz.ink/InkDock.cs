#if TOOLS
using Godot;

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

        resetButton.Connect("pressed", this, nameof(OnResetButtonPressed));
        loadButton.Connect("pressed", this, nameof(OnLoadButtonPressed));
        fileDialog.Connect("popup_hide", this, nameof(OnFileDialogHide));
        story.Connect(nameof(InkStory.InkChoices), this, nameof(OnStoryChoices));
        story.Connect(nameof(InkStory.InkContinued), this, nameof(OnStoryContinued));
        story.Connect(nameof(InkStory.InkEnded), this, nameof(OnStoryEnded));
    }

    private void ResetStoryContent()
    {
        RemoveAllStoryContent();
        RemoveAllChoices();
    }

    private void RemoveAllStoryContent()
    {
        foreach (Node n in storyText.GetChildren())
            storyText.RemoveChild(n);
    }

    private void RemoveAllChoices()
    {
        foreach (Node n in storyChoices.GetChildren())
            storyChoices.RemoveChild(n);
    }

    private void OnResetButtonPressed()
    {
        ResetStoryContent();
        story.LoadStory();
        story.Continue();
    }

    private void OnLoadButtonPressed()
    {
        fileDialog.PopupCentered();
    }

    private void OnFileDialogHide()
    {
        ResetStoryContent();
        fileName.Text = "";
        if (fileDialog.CurrentFile == null || fileDialog.CurrentFile.Length == 0)
            resetButton.Disabled = true;
        else
        {
            resetButton.Disabled = false;
            fileName.Text = fileDialog.CurrentFile;
            story.InkFile = ResourceLoader.Load(fileDialog.CurrentPath);
            story.LoadStory();
            story.Continue();
        }
    }

    private void OnStoryContinued(string text, string[] _)
    {
        text = text.Trim();
        if (text.Length > 0)
        {
            Label newLine = new Label()
            {
                Autowrap = true,
                Text = text,
            };
            AddToStory(newLine);
        }

        story.Continue();
    }

    private void OnStoryChoices(string[] choices)
    {
        int i = 0;
        foreach (string choice in choices)
        {
            if (i < storyChoices.GetChildCount()) continue;

            Button button = new Button()
            {
                Text = choice,
            };
            button.Connect("pressed", this, nameof(ClickChoice), new Godot.Collections.Array() { i });
            storyChoices.AddChild(button);
            ++i;
        }
    }

    private void OnStoryEnded()
    {
        CanvasItem endOfStory = new VBoxContainer();
        endOfStory.AddChild(new HSeparator());
        CanvasItem endOfStoryLine = new HBoxContainer();
        endOfStory.AddChild(endOfStoryLine);
        endOfStory.AddChild(new HSeparator());
        Control separator = new HSeparator()
        {
            SizeFlagsHorizontal = (int)(SizeFlags.Fill | SizeFlags.Expand),
        };
        Label endOfStoryText = new Label()
        {
            Text = "End of story"
        };
        endOfStoryLine.AddChild(separator);
        endOfStoryLine.AddChild(endOfStoryText);
        endOfStoryLine.AddChild(separator.Duplicate());
        AddToStory(endOfStory);
    }

    private void ClickChoice(int idx)
    {
        story.ChooseChoiceIndex(idx);
        RemoveAllChoices();
        AddToStory(new HSeparator());
        story.Continue();
    }

    private async void AddToStory(CanvasItem item)
    {
        storyText.AddChild(item);
        await ToSignal(GetTree(), "idle_frame");
        await ToSignal(GetTree(), "idle_frame");
        scroll.ScrollVertical = (int)scroll.GetVScrollbar().MaxValue;
    }
}
#endif
