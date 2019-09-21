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
    private RichTextLabel storyText;
    private VBoxContainer storyChoices;

    public override void _Ready()
    {
        fileSelect = GetNode<OptionButton>("VBoxContainer/GridContainer/OptionButton");
        fileDialog = GetNode<FileDialog>("FileDialog");
        fileSelect.Connect("item_selected", this, nameof(onFileSelectItemSelected));
        fileDialog.Connect("file_selected", this, nameof(onFileDialogFileSelected));
        fileDialog.Connect("popup_hide", this, nameof(onFileDialogHide));

        storyNode = GetNode("Story");
        storyNode.Connect(nameof(Story.InkContinued), this, nameof(onStoryContinued));
        storyNode.Connect(nameof(Story.InkChoices), this, nameof(onStoryChoices));
        storyText = GetNode<RichTextLabel>("VBoxContainer/StoryText");
        storyChoices = GetNode<VBoxContainer>("VBoxContainer/StoryChoices");
        storyNode.SetScript(ResourceLoader.Load("res://addons/paulloz.ink/InkStory.cs") as Script);
    }

    private void resetFileSelectItems()
    {
        while (fileSelect.GetItemCount() > 2)
            fileSelect.RemoveItem(fileSelect.GetItemCount() - 1);
    }

    private void resetStoryContent()
    {
        storyText.Text = "";
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
            continueStoryMaximally();
        }
    }

    private void continueStoryMaximally()
    {
        while ((bool)storyNode.Get("CanContinue"))
        {
            try
            {
                storyNode.Call("Continue");
            }
            catch (Ink.Runtime.StoryException e)
            {
                onStoryContinued(e.ToString(), new String[] { });
            }
        }
    }

    private void onStoryContinued(String text, String[] tags)
    {
        storyText.Text = (storyText.Text + text).TrimStart(new char[] { ' ', '\n' });
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
        resetStoryContent();
        storyNode.Callv("ChooseChoiceIndex", new Godot.Collections.Array() { idx });
        continueStoryMaximally();
    }
}
// #endif