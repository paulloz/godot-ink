using Godot;

namespace GodotInk.Examples.TeenyTiny;

public partial class CameraZone : Area2D
{
    [Export]
    private Vector2I min;

    [Export]
    private Vector2I max;

    public override void _Ready()
    {
        BodyEntered += WhenBodyEntered;
    }

    private void WhenBodyEntered(Node2D body)
    {
        (body as Player)?.ChangeCameraLimits(min.Y, max.X, max.Y, min.X);
    }
}
