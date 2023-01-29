using Godot;

namespace GodotInk.Examples.TeenyTiny;

public partial class Portal : Area2D
{
    [Export]
    private Vector2I destination;

    public override void _Ready()
    {
        BodyEntered += WhenBodyEntered;
    }

    private void WhenBodyEntered(Node2D body)
    {
        if (body is not Player) return;

        body.GlobalPosition = destination;
    }
}
