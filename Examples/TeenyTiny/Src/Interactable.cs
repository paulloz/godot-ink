using Godot;

namespace GodotInk.Examples.TeenyTiny;

public partial class Interactable : Node2D
{
    public void Interact()
    {
        if (GetMeta("Dialogue") is Variant dialogueId && !string.IsNullOrEmpty((string)dialogueId))
            Locator.Fetch<IDialogueService>().PlayDialogue((string)dialogueId);
    }
}
