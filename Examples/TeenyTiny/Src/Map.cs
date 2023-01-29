using Godot;

namespace GodotInk.Examples.TeenyTiny;

public partial class Map : TileMap, IDialogueService
{
    [Export]
    private InkStory story = null!;

    [Export]
    private DialogueBox dialogueBox = null!;

    private Interactable chest = null!;
    private Interactable? chestKey;

    public override void _EnterTree()
    {
        Locator.Provide<IDialogueService>(this);
    }

    public override void _ExitTree()
    {
        Locator.Unprovide<IDialogueService>(this);
    }

    public void PlayDialogue(string id)
    {
        // Go to the right knot in the story.
        story.ResetCallstack();
        story.ChoosePathString(id);

        // Weird that we have no content but oh well...
        if (!story.CanContinue) return;

        // Forward to the UI.
        dialogueBox.Open(story);
    }

    public override void _Ready()
    {
        chest = this.GetComponent<Interactable>("Chest");
        chestKey = this.GetComponent<Interactable>("ChestKey");

        // Observe some variables.
        story.ObserveVariable("Inventory", Callable.From<string, InkList>(ObserveInventory));

        // And bind some functions.
        story.BindExternalFunction("Animate", Callable.From<string>(Animate));
    }

    private void ObserveInventory(string _, InkList list)
    {
        if (chestKey is not null && list.Contains("ChestKey"))
        {
            EraseCell(1, LocalToMap(chestKey.Position));
            chestKey.QueueFree();
            chestKey = null;
        }
    }

    private void Animate(string id)
    {
        switch (id)
        {
            case "Chest":
                Vector2I cellCoords = LocalToMap(chest.Position);
                SetCell(1, cellCoords, GetCellSourceId(1, cellCoords),
                        GetCellAtlasCoords(1, cellCoords) + new Vector2I(2, 0));
                break;
        }
    }
}
