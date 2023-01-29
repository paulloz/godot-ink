using Godot;

namespace GodotInk.Examples.TeenyTiny;

public interface IDialogueService : IService
{
    public void PlayDialogue(string id);
}

public class EmptyDialogueService : IDialogueService
{
    public void PlayDialogue(string id)
    {
        GD.Print($"Opening dialogue: {id}.");
    }
}
