#if TOOLS
using Godot;
using System.Linq;

[Tool]
public class InkDock : Control
{
    private InkStory story;
    private bool storyStarted;

    private Button loadButton;
    private FileDialog fileDialog;

    private Label storyNameLabel;

    private Button startButton;
    private Button stopButton;
    private Button clearButton;

    private VBoxContainer storyText;
    private VBoxContainer storyChoices;

    private ScrollContainer scroll;

    public override void _Ready()
    {
        // Initialize top.
        loadButton = GetNode<Button>("Container/Left/Top/LoadButton");
        fileDialog = GetNode<FileDialog>("FileDialog");
        storyNameLabel = GetNode<Label>("Container/Left/Top/Label");
        startButton = GetNode<Button>("Container/Left/Top/StartButton");
        stopButton = GetNode<Button>("Container/Left/Top/StopButton");
        clearButton = GetNode<Button>("Container/Left/Top/ClearButton");

        loadButton.Connect("pressed", fileDialog, "popup_centered");
        fileDialog.Connect("popup_hide", this, nameof(LoadStoryResource));
        startButton.Connect("pressed", this, nameof(StartStory));
        stopButton.Connect("pressed", this, nameof(StopStory));
        clearButton.Connect("pressed", this, nameof(ClearStory), new Godot.Collections.Array { false });

        // Initialize bottom.
        storyText = GetNode<VBoxContainer>("Container/Left/Scroll/Margin/StoryText");
        storyChoices = GetNode<VBoxContainer>("Container/Right/StoryChoices");
        scroll = GetNode<ScrollContainer>("Container/Left/Scroll");

        // Set icons.
        loadButton.Icon = GetIcon("Load", "EditorIcons");
        startButton.Icon = GetIcon("Play", "EditorIcons");
        stopButton.Icon = GetIcon("Stop", "EditorIcons");
        clearButton.Icon = GetIcon("Clear", "EditorIcons");

        UpdateTop();
    }

    private void UpdateTop()
    {
        bool hasStory = story != null;

        storyNameLabel.Text = hasStory ? story.InkFile.ResourcePath : string.Empty;

        startButton.Visible = hasStory && !storyStarted;
        stopButton.Visible = hasStory && storyStarted;
        clearButton.Visible = hasStory;
        clearButton.Disabled = storyText.GetChildCount() <= 0;

        storyChoices.GetParent<Control>().Visible = hasStory;
    }

    private void LoadStoryResource()
    {
        StopStory();
        story = null;

        if (!string.IsNullOrEmpty(fileDialog.CurrentFile))
        {
            story = new InkStory()
            {
                AutoLoadStory = false,
                InkFile = ResourceLoader.Load(fileDialog.CurrentPath),
            };

            story.Connect(nameof(InkStory.InkContinued), this, nameof(OnStoryContinued));
            story.Connect(nameof(InkStory.InkChoices), this, nameof(OnStoryChoices));
            story.Connect(nameof(InkStory.InkEnded), this, nameof(OnStoryEnded));

            AddChild(story);
        }

        UpdateTop();
    }

    private void StartStory()
    {
        if (story == null) return;

        story.LoadStory();
        storyStarted = true;
        story.Continue();

        UpdateTop();
    }

    private void StopStory()
    {
        storyStarted = false;
        story?.LoadStory();

        ClearStory(true);
    }

    private void ClearStory(bool clearChoices)
    {
        RemoveAllStoryContent();
        if (clearChoices)
            RemoveAllChoices();

        UpdateTop();
    }

    private void OnStoryContinued(string text, string[] tags)
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

            if (tags.Length > 0)
            {
                newLine = new Label()
                {
                    Autowrap = true,
                    Align = Label.AlignEnum.Center,
                    Text = $"# {string.Join(", ", tags)}",
                };
                newLine.AddColorOverride("font_color", GetColor("font_color_disabled", "Button"));
                AddToStory(newLine);
            }
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

    private void RemoveAllStoryContent()
    {
        foreach (Node n in storyText.GetChildren())
            storyText.RemoveChild(n);
    }

    private void RemoveAllChoices()
    {
        foreach (Node n in storyChoices.GetChildren().OfType<Button>())
            storyChoices.RemoveChild(n);
    }
}
#endif
