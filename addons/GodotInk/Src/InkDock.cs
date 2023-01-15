#if TOOLS

#nullable enable
using System.Linq;

namespace Godot.Ink
{
    [Tool]
    public partial class InkDock : Control
    {
        private Button loadButton = null!;
        private Button startButton = null!;
        private Button stopButton = null!;
        private Button clearButton = null!;
        private FileDialog fileDialog = null!;

        private Label storyNameLabel = null!;
        private VBoxContainer storyText = null!;
        private VBoxContainer storyChoices = null!;
        private ScrollContainer scroll = null!;

        private InkStory? story;
        private bool storyStarted;

        public override void _Ready()
        {
            base._Ready();

            // Initialize top.
            loadButton = GetNode<Button>("Container/Left/Top/LoadButton");
            fileDialog = GetNode<FileDialog>("FileDialog");
            storyNameLabel = GetNode<Label>("Container/Left/Top/Label");
            startButton = GetNode<Button>("Container/Left/Top/StartButton");
            stopButton = GetNode<Button>("Container/Left/Top/StopButton");
            clearButton = GetNode<Button>("Container/Left/Top/ClearButton");

            // Connect UI events.
            loadButton.Pressed += () => fileDialog.PopupCentered();
            fileDialog.FileSelected += LoadStory;
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
            bool hasStory = story is InkStory;

            storyNameLabel.Text = hasStory ? story!.ResourcePath : string.Empty;

            startButton.Visible = hasStory && !storyStarted;
            stopButton.Visible = hasStory && storyStarted;
            clearButton.Visible = hasStory;
            clearButton.Disabled = storyText.GetChildCount() <= 0;

            storyChoices.GetParent<Control>().Visible = hasStory;
        }

        private void LoadStory(string path)
        {
            story = GD.Load<InkStory>(path);

            UpdateTop();
        }

        private void StartStory()
        {
            if (story is not InkStory) return;

            storyStarted = true;
            ContinueStory();

            UpdateTop();
        }

        private void StopStory()
        {
            storyStarted = false;
            story?.ResetState();

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
            if (story is not InkStory || !story.CanContinue) return;

            story.Continue();

            string currentText = story.CurrentText;
            if (currentText.Length > 0)
            {
                Label newLine = new Label()
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

                    Button button = new Button()
                    {
                        Text = choice.Text,
                    };
                    button.Pressed += () => ClickChoice(choice.Index);

                    storyChoices.AddChild(button);

                    ++i;
                }
            }

            ContinueStory();
        }

        private void ClickChoice(int idx)
        {
            if (story is not InkStory) return;

            story.ChooseChoiceIndex(idx);
            RemoveAllChoices();
            AddToStory(new HSeparator());

            ContinueStory();
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
    }
}
#nullable restore

#endif