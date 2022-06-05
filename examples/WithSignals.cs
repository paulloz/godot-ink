using Godot;

public class WithSignals : Control
{
    private InkPlayer player;
    private StoryContainer container;

    private Timer timer;

    public override void _Ready()
    {
        // Retrieve or create some Nodes we know we'll need quite often
        player = GetNode<InkPlayer>("InkPlayer");
        container = GetNode<StoryContainer>("Container");
        timer = new Timer()
        {
            Autostart = false,
            WaitTime = 0.3f,
            OneShot = true,
        };
        timer.Connect("timeout", player, "Continue");
        AddChild(timer);

        // Start the story
        timer.Start();
    }

    public void OnStoryInkEnded()
    {
        container.Add(container.CreateSeparation(), 0.4f);
        container.Add(container.CreateSeparation(), 0.5f);
        container.Add(container.CreateSeparation(), 0.6f);
    }

    public void OnStoryInkContinued(string text, string[] _)
    {
        text = text.Trim();
        if (text.Length > 0)
            container.Add(container.CreateText(text));

        // Go again
        timer.Start();
    }

    public void OnStoryInkChoices(string[] choices)
    {
        container.Add(container.CreateSeparation(), 0.2f);
        // Add a button for each choice
        for (int i = 0; i < choices.Length; ++i)
            container.Add(container.CreateChoice(choices[i], i), 0.4f);
    }

    protected void OnChoiceClick(int choiceIndex)
    {
        container.CleanChoices();
        // Choose the clicked choice and continue onward
        player.ChooseChoiceIndexAndContinue(choiceIndex);
    }
}
