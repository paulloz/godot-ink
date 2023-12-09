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
        if (body is not Player player) return;

        Teleport(player);
    }

    private async void Teleport(Player player)
    {
        var transition = GetNode<ColorRect>("/root/TeenyTiny/UILayer/UIRoot/Transition");

        transition.Visible = true;

        player.GlobalPosition = destination;

        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);

        transition.Visible = false;
    }
}
