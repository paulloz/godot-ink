using Godot;

public class WithLoop : Control
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
        AddChild(timer);
    }

    public override void _Process(float delta)
    {
        // If the time is running, we want to wait
        if (timer.TimeLeft > 0) return;

        // Check if we have anything to consume
        if (player.CanContinue)
        {
            string text = player.Continue().Trim();
            if (text.Length > 0)
                container.Add(container.CreateText(text));

            // Maybe we have choices now that we moved on?
            if (player.HasChoices)
            {
                container.Add(container.CreateSeparation(), 0.2f);
                // Add a button for each choice
                for (int i = 0; i < player.CurrentChoices.Length; ++i)
                    container.Add(container.CreateChoice(player.CurrentChoices[i], i), 0.4f);
            }
            timer.Start();
        }
        else if (!player.HasChoices)
        {
            container.Add(container.CreateSeparation(), 0.4f);
            container.Add(container.CreateSeparation(), 0.5f);
            container.Add(container.CreateSeparation(), 0.6f);
            SetProcess(false);
        }
    }

    protected void OnChoiceClick(int choiceIndex)
    {
        container.CleanChoices();
        // Choose the clicked choice
        player.ChooseChoiceIndex(choiceIndex);
    }
}
