#if TOOLS
#nullable enable

using Godot;
using System;
using System.Linq;

namespace GodotInk;

[Tool]
public partial class InkDock : VBoxContainer
{
    private Button loadButton = null!;
    private Button startButton = null!;
    private Button stopButton = null!;
    private Button clearButton = null!;

    private Label storyNameLabel = null!;
    private VBoxContainer storyText = null!;
    private VBoxContainer storyChoices = null!;
    private ScrollContainer scroll = null!;

    private EditorFileDialog fileDialog = null!;

    private string storyPath = "";
    private InkStory? story;
    private bool storyStarted;

    public override void _Ready()
    {
        base._Ready();

        CustomMinimumSize = new Vector2(0.0f, 228.0f);

        // Initialize file dialog.
        fileDialog = new()
        {
            FileMode = EditorFileDialog.FileModeEnum.OpenFile,
            Access = EditorFileDialog.AccessEnum.Resources,
        };
        fileDialog.AddFilter("*.ink", "Ink Stories");
        fileDialog.FileSelected += LoadStory;

        AddChild(fileDialog);

        // Initialize top.
        loadButton = GetNode<Button>("Container/Left/Top/LoadButton");
        storyNameLabel = GetNode<Label>("Container/Left/Top/Label");
        startButton = GetNode<Button>("Container/Left/Top/StartButton");
        stopButton = GetNode<Button>("Container/Left/Top/StopButton");
        clearButton = GetNode<Button>("Container/Left/Top/ClearButton");

        // Connect UI events.
        loadButton.Pressed += () => fileDialog.PopupCenteredClamped(new Vector2I(1050, 700), 0.8f);
        startButton.Pressed += StartStory;
        stopButton.Pressed += StopStory;
        clearButton.Pressed += () => ClearStory(false);

        // Initialize bottom.
        storyText = GetNode<VBoxContainer>("Container/Left/Scroll/Margin/StoryText");
        storyChoices = GetNode<VBoxContainer>("Container/Right/StoryChoices");
        scroll = GetNode<ScrollContainer>("Container/Left/Scroll");

        // Set icons.
        loadButton.Icon = GetThemeIcon("Load", "EditorIcons");
        startButton.Icon = GetThemeIcon("Play", "EditorIcons");
        stopButton.Icon = GetThemeIcon("Stop", "EditorIcons");
        clearButton.Icon = GetThemeIcon("Clear", "EditorIcons");

        // Update UI.
        UpdateTop();
    }

    private void UpdateTop()
    {
        bool hasStory = story != null;

        storyNameLabel.Text = hasStory ? storyPath : string.Empty;

        startButton.Visible = hasStory && !storyStarted;
        stopButton.Visible = hasStory && storyStarted;
        clearButton.Visible = hasStory;
        clearButton.Disabled = storyText.GetChildCount() <= 0;

        storyChoices.GetParent<Control>().Visible = hasStory;
    }

    private void LoadStory(string path)
    {
        try
        {
            storyPath = path;
            story = ResourceLoader.Load<InkStory>(path, null, ResourceLoader.CacheMode.Ignore);

            story.Continued += ContinueStory;

            UpdateTop();
        }
        catch (InvalidCastException)
        {
            GD.PrintErr($"{path} is not a valid ink story. "
                       + "Please make sure it was imported with `is_main_file` set to `true`.");
        }
    }

    private void StartStory()
    {
        if (story == null) return;

        storyStarted = true;
        story.ContinueMaximally();

        UpdateTop();
    }

    private void StopStory()
    {
        StopStory(false);
    }

    private void StopStory(bool setStoryToNull)
    {
        storyStarted = false;

        try
        {
            story?.ResetState();
        }
        catch (ObjectDisposedException)
        {
            story = null;
        }

        if (setStoryToNull)
            story = null;

        ClearStory(true);
    }

    private void ClearStory(bool clearChoices)
    {
        RemoveAllStoryContent();
        if (clearChoices)
            RemoveAllChoices();

        UpdateTop();
    }

    private void ContinueStory()
    {
        if (story == null) return;

        string currentText = story.CurrentText.Trim();

        if (currentText.Length > 0)
        {
            Label newLine = new()
            {
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Text = currentText,
            };
            AddToStory(newLine);

            if (story.CurrentTags.Count > 0)
            {
                newLine = new Label()
                {
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = $"# {string.Join(", ", story.CurrentTags)}",
                };
                newLine.AddThemeColorOverride("font_color", GetThemeColor("font_color_disabled", "Button"));
                AddToStory(newLine);
            }
        }

        if (story.CurrentChoices.Count > 0)
        {
            int i = 0;
            foreach (InkChoice choice in story.CurrentChoices)
            {
                if (i < storyChoices.GetChildCount()) continue;

                Button button = new()
                {
                    Text = choice.Text,
                };

                button.Pressed += () => ClickChoice(choice.Index);

                storyChoices.AddChild(button);

                ++i;
            }
        }
    }

    private void ClickChoice(int idx)
    {
        if (story == null) return;

        story.ChooseChoiceIndex(idx);
        RemoveAllChoices();
        AddToStory(new HSeparator());

        if (story.CanContinue)
            story.ContinueMaximally();
    }

    private async void AddToStory(CanvasItem item)
    {
        storyText.AddChild(item);
        await ToSignal(GetTree(), "process_frame");
        await ToSignal(GetTree(), "process_frame");
        scroll.ScrollVertical = (int)scroll.GetVScrollBar().MaxValue;
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

    public void WhenInkResourceReimported()
    {
        StopStory(true);
    }
}

#endif
