// #if TOOLS
using Godot;
using System;

[Tool]
public class InkDock : Control
{
    private OptionButton fileSelect;
    private FileDialog fileDialog;

    private String currentFilePath;

    private Node storyNode;
    private VBoxContainer storyText;
    private VBoxContainer storyChoices;
    
    private ScrollBar scrollbar;

    public override void _Ready()
    {
        fileSelect = GetNode<OptionButton>("Container/Top/OptionButton");
        fileDialog = GetNode<FileDialog>("FileDialog");
        fileSelect.Connect("item_selected", this, nameof(onFileSelectItemSelected));
        fileDialog.Connect("file_selected", this, nameof(onFileDialogFileSelected));
        fileDialog.Connect("popup_hide", this, nameof(onFileDialogHide));

        storyNode = GetNode("Story");
        storyNode.SetScript(ResourceLoader.Load("res://addons/paulloz.ink/InkStory.cs") as Script);
        storyNode.Connect(nameof(InkStory.InkChoices), this, nameof(onStoryChoices));
        storyText = GetNode<VBoxContainer>("Container/Bottom/Scroll/Margin/StoryText");
        storyChoices = GetNode<VBoxContainer>("Container/Bottom/StoryChoices");

        scrollbar = this.GetNode<ScrollContainer>("Container/Bottom/Scroll").GetVScrollbar();
    }

    private void resetFileSelectItems()
    {
        while (fileSelect.GetItemCount() > 2)
            fileSelect.RemoveItem(fileSelect.GetItemCount() - 1);
    }

    private void resetStoryContent()
    {
        this.removeAllStoryContent();
        this.removeAllChoices();
    }

    private void removeAllStoryContent()
    {
        foreach (Node n in storyText.GetChildren())
            storyText.RemoveChild(n);
    }

    private void removeAllChoices()
    {
        foreach (Node n in storyChoices.GetChildren())
            storyChoices.RemoveChild(n);
    }

    private void onFileSelectItemSelected(int id)
    {
        if (id == 0)
        {
            resetFileSelectItems();
            resetStoryContent();
            currentFilePath = "";
        }
        else if (id == 1)
        {
            fileSelect.Select(0);
            fileDialog.PopupCentered();
        }
    }

    private void onFileDialogFileSelected(String path)
    {
        if (path.EndsWith(".json"))
        {
            resetFileSelectItems();
            fileSelect.AddItem(path.Substring(path.FindLast("/") + 1));
            currentFilePath = path;
        }
    }

    private void onFileDialogHide()
    {
        if (currentFilePath == null || currentFilePath.Length == 0)
            fileSelect.Select(0);
        else
        {
            fileSelect.Select(2);
            storyNode.Set("InkFile", ResourceLoader.Load(currentFilePath));
            storyNode.Call("LoadStory");
            resetStoryContent();
            continueStoryMaximally();
        }
    }

    private async void continueStoryMaximally()
    {
        while ((bool)storyNode.Get("CanContinue"))
        {
            try
            {
                storyNode.Call("Continue");
                onStoryContinued(storyNode.Get("CurrentText") as String, new String[] { });
            }
            catch (Ink.Runtime.StoryException e)
            {
                onStoryContinued(e.ToString(), new String[] { });
            }
        }
        await ToSignal(GetTree(), "idle_frame");
        this.scrollbar.Value = this.scrollbar.MaxValue;
    }

    private void onStoryContinued(String text, String[] tags)
    {
        Label newLine = new Label();
        newLine.Autowrap = true;
        newLine.Text = text.Trim(new char[] { ' ', '\n' });
        this.storyText.AddChild(newLine);
    }

    private void onStoryChoices(String[] choices)
    {
        int i = 0;
        foreach (String choice in choices)
        {
            Button button = new Button();
            button.Text = choice;
            button.Connect("pressed", this, nameof(clickChoice), new Godot.Collections.Array() { i });
            storyChoices.AddChild(button);
            ++i;
        }
    }

    private void clickChoice(int idx)
    {
        storyNode.Callv("ChooseChoiceIndex", new Godot.Collections.Array() { idx });
        this.removeAllChoices();
        this.storyText.AddChild(new HSeparator());
        continueStoryMaximally();
    }
}
// #endif