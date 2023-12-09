using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotInk.Examples.TeenyTiny;

public partial class DialogueBox : NinePatchRect
{
    [Export]
    private int charactersPerSecond = 12;

    private AnimationPlayer animationPlayer = null!;
    private RichTextLabel richTextLabel = null!;

    private Container choicesContainer = null!;

    private Tween textTween = null!;
    private InkStory? story;

    private int currentChoiceSelection;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        richTextLabel = GetNode<RichTextLabel>("MarginContainer/RichTextLabel");

        choicesContainer = GetNode<Container>("MarginContainer/ChoicesContainer");

        textTween = CreateTween();
        textTween.Kill();

        SetProcessInput(false);
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey) return;

        // Eat all the events!
        GetViewport().SetInputAsHandled();

        if (CountCurrentChoices() > 0)
        {
            var newChoiceSelection = ComputeNewChoiceSelection(inputEvent);
            if (newChoiceSelection != currentChoiceSelection)
            {
                var label = choicesContainer.GetChild<Label>(currentChoiceSelection);
                label.Text = $" {label.Text[1..]}";
                currentChoiceSelection = newChoiceSelection;
                label = choicesContainer.GetChild<Label>(currentChoiceSelection);
                label.Text = $">{label.Text[1..]}";
            }
        }

        if (inputEvent.IsActionPressed(Constants.Actions.Select))
        {
            // Safety.
            if (story is null)
            {
                Close();
            }
            // The text animation is playing, skip it.
            else if (textTween.IsRunning())
            {
                EndTweenDialogueText();
            }
            // More dialogue, loop back to a new tween for the next line.
            // Make sure to auto-skip blank lines, though.
            else if (story.CanContinue && story.Continue() is string text && !string.IsNullOrWhiteSpace(text))
            {
                TweenDialogueText(text);
            }
            // We have choices to handle.
            else if (story.CurrentChoices.Count > 0)
            {
                if (CountCurrentChoices() <= 0)
                {
                    StartChoicesSequence(story.CurrentChoices);
                }
                else
                {
                    EndChoicesSequence();
                    _Input(inputEvent);
                }
            }
            // Nothing else to do but close this window.
            else
            {
                Close();
            }
        }
    }

    private int ComputeNewChoiceSelection(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(Constants.Actions.Up))
        {
            return Mathf.PosMod(currentChoiceSelection - 1, CountCurrentChoices());
        }
        else if (inputEvent.IsActionPressed(Constants.Actions.Down))
        {
            return Mathf.PosMod(currentChoiceSelection + 1, CountCurrentChoices());
        }

        return currentChoiceSelection;
    }

    private void TweenDialogueText(string text)
    {
        // Set full text.
        richTextLabel.Text = text;

        // And animate.
        textTween = CreateTween();

        textTween.TweenMethod(
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

        var canContinue = story is not null && (story.CanContinue || story.CurrentChoices.Count > 0);
        animationPlayer.Play(canContinue ? "CanContinue" : "CantContinue");
    }

    private void StartChoicesSequence(IReadOnlyList<InkChoice> choices)
    {
        // Unset text.
        richTextLabel.Text = "";

        // Init the selection.
        currentChoiceSelection = 0;

        // Add all choices.
        foreach (var choice in choices)
        {
            choicesContainer.AddChild(new Label() { Text = $"{(choice.Index == 0 ? ">" : "")} {choice.Text}" });
        }
    }

    private void EndChoicesSequence()
    {
        story?.ChooseChoiceIndex(currentChoiceSelection);

        FreeChoices();
    }

    public void Open(InkStory story)
    {
        this.story = story;

        // Reset everything, just in case.
        richTextLabel.Text = "";
        FreeChoices();

        // Everything seems right, open the window.
        animationPlayer.Play("Open");
        // And once it's done, send in the first line of dialogue.
        animationPlayer.Connect(
            AnimationPlayer.SignalName.AnimationFinished,
            Callable.From((StringName _) => TweenDialogueText(story.Continue())),
            (uint)ConnectFlags.OneShot
        );

        SetProcessInput(true);
        // Stinky.
        Player.InputsEnabled = false;
    }

    public void Close()
    {
        story = null;
        animationPlayer.Play("Close");

        SetProcessInput(false);
        // Stinky.
        Player.InputsEnabled = true;
    }

    private void FreeChoices()
    {
        foreach (var child in choicesContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    private int CountCurrentChoices()
    {
        return choicesContainer.GetChildren().Count(child => !child.IsQueuedForDeletion());
    }
}
