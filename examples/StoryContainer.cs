using Godot;
using System;

using GodotArray = Godot.Collections.Array;

public class StoryContainer : Control
{
    [Signal]
    public delegate void ChoiceClick(int choiceIndex);

    private ScrollContainer scroll;
    private BoxContainer container;

    public override void _Ready()
    {
        scroll = GetNode<ScrollContainer>("VMargin/Scroll");
        container = GetNode<BoxContainer>("VMargin/Scroll/HMargin/StoryContainer");
    }

    public async void Add(CanvasItem newNode, float delay = 0)
    {
        // Fade-in effect
        Tween tween = new Tween();
        tween.InterpolateProperty(newNode, "modulate", Colors.Transparent, Colors.White, 0.5f,
                                  Tween.TransitionType.Linear, Tween.EaseType.InOut, delay);
        tween.Connect("tween_all_completed", tween, "queue_free");
        newNode.AddChild(tween);
        newNode.Modulate = Colors.Transparent;
        container.AddChild(newNode); // We actually add the thing here
        tween.Start();

        // Wait for the scrollbar to recalculate and scroll to the bottom
        await ToSignal(GetTree(), "idle_frame");
        scroll.ScrollVertical = (int)scroll.GetVScrollbar().MaxValue;
    }

    public void CleanChoices()
    {
        // Remove all nodes in choiceButtons group
        foreach (Node choice in GetTree().GetNodesInGroup("choiceButtons"))
            choice.QueueFree();
    }

    public Label CreateText(String text)
    {
        Label label = new Label()
        {
            Autowrap = true,
            Text = text,
        };
        return label;
    }

    public Button CreateChoice(String text, int choiceIndex)
    {
        Button button = new Button()
        {
            Text = text,
            SizeFlagsHorizontal = (int)SizeFlags.ShrinkCenter,
        };
        button.AddToGroup("choiceButtons");
        button.Connect("pressed", this, "OnButtonPressed", new GodotArray { choiceIndex });
        return button;
    }

    public HSeparator CreateSeparation()
    {
        return new HSeparator();
    }

    private void OnButtonPressed(int choiceIndex)
    {
        EmitSignal(nameof(ChoiceClick), choiceIndex);
    }
}
