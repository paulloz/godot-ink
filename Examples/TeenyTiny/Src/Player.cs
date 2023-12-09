using Godot;

namespace GodotInk.Examples.TeenyTiny;

public partial class Player : CharacterBody2D
{
    public static bool InputsEnabled { get; set; } = true;

    [Export]
    private float speed = 100.0f;

    private Node2D pivot = null!;
    private CollisionShape2D collision = null!;
    private RayCast2D rayCast = null!;
    private Sprite2D sprite = null!;
    private AnimationPlayer animationPlayer = null!;
    private Camera2D camera = null!;

    public override void _Ready()
    {
        pivot = this.GetComponent<Node2D>("Pivot");
        collision = this.GetComponent<CollisionShape2D>("Collision");
        rayCast = this.GetComponent<RayCast2D>("Pivot/Ray");
        sprite = this.GetComponent<Sprite2D>("MainSprite");
        animationPlayer = this.GetComponent<AnimationPlayer>("AnimationPlayer");
        camera = this.GetComponent<Camera2D>("Camera");
    }

    public override void _PhysicsProcess(double _)
    {
        // Compute intended movement.
        Vector2 direction = InputsEnabled ? Input.GetVector(
            Constants.Actions.MoveWest, Constants.Actions.MoveEast,
            Constants.Actions.MoveNorth, Constants.Actions.MoveSouth
        ) : Vector2.Zero;

        // Graphics and animation.
        if (!Mathf.IsZeroApprox(direction.X))
            sprite.Scale = pivot.Scale with { X = direction.Sign().X };
        animationPlayer.Play(!Mathf.IsZeroApprox(direction.LengthSquared()) ? "Walk" : "Idle");

        // Update the rayCast node. Only update on 4 dirs.
        pivot.Rotation = direction switch
        {
            { X: 0.0f, Y: -1.0f } => Vector2.Down.AngleTo(Vector2.Up),
            { X: 1.0f, Y: 0.0f } => Vector2.Down.AngleTo(Vector2.Right),
            { X: 0.0f, Y: 1.0f } => 0.0f,
            { X: -1.0f, Y: 0.0f } => Vector2.Down.AngleTo(Vector2.Left),
            _ => pivot.Rotation,
        };

        // Actually move.
        Velocity = direction * speed;
        MoveAndSlide();
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(Constants.Actions.Interact))
        {
            (rayCast.GetCollider() switch
            {
                Interactable interactable => interactable,
                TileMap map => map.GetInteractableAt(map.GlobalToMap(rayCast.ToGlobal(rayCast.TargetPosition))),
                _ => null,
            })?.Interact();
        }
    }

    public void ChangeCameraLimits(int top, int right, int bottom, int left)
    {
        camera.LimitTop = top;
        camera.LimitRight = right;
        camera.LimitBottom = bottom;
        camera.LimitLeft = left;
    }
}
