using Godot;

namespace GodotInk.Examples.TeenyTiny;

public partial class DialogueBox : NinePatchRect
{
    [Export]
    private int charactersPerSecond = 12;

    private AnimationPlayer animationPlayer = null!;
    private RichTextLabel richTextLabel = null!;

    private Tween textTween = null!;
    private InkStory? story;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        richTextLabel = GetNode<RichTextLabel>("MarginContainer/RichTextLabel");

        textTween = CreateTween();
        textTween.Kill();

        SetProcessInput(false);
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey) return;

        // Eat all the events!
        GetViewport().SetInputAsHandled();

        if (inputEvent.IsActionPressed("ui_select"))
        {
            // Safety.
            if (story is null)
                Close();
            // The text animation is playing, skip it.
            else if (textTween.IsRunning())
                EndTweenDialogueText();
            // More dialogue, loop back to a new tween for the next line.
            // Make sure to auto-skip blank lines, though.
            else if (story.CanContinue && story.Continue() is string text && !string.IsNullOrWhiteSpace(text))
                TweenDialogueText(text);
            // Nothing else to do but close this window.
            else
                Close();
        }
    }

    private void TweenDialogueText(string text)
    {
        // Set full text.
        richTextLabel.Text = text;

        // And animate.
        textTween = CreateTween();

        _ = textTween.TweenMethod(
            Callable.From<float>((float ratio) => richTextLabel.VisibleRatio = ratio),
            0.0f, 1.0f,
            text.Length / charactersPerSecond
        );
        textTween.Finished += EndTweenDialogueText;
        textTween.Play();

        animationPlayer.Play("TweeningText");
    }

    private void EndTweenDialogueText()
    {
        if (textTween.IsRunning())
            textTween.Kill();
        richTextLabel.VisibleRatio = 1.0f;
        animationPlayer.Play(story?.CanContinue ?? false ? "CanContinue" : "CantContinue");
    }

    public void Open(InkStory story)
    {
        this.story = story;

        // Everything seems right, open the window.
        animationPlayer.Play("Open");
        // And once it's done, send in the first line of dialogue.
        _ = animationPlayer.Connect(
            AnimationPlayer.SignalName.AnimationFinished,
            Callable.From((StringName _) => TweenDialogueText(story.Continue())),
            (uint)ConnectFlags.OneShot
        );

        SetProcessInput(true);
    }

    public void Close()
    {
        story = null;
        animationPlayer.Play("Close");

        SetProcessInput(false);
    }
}
