extends Node

func _ready():
    set_process(true)
    $InkStory.LoadStory("ink/example.json")
    $InkStory.Continue();
    
func _process(delta):
    if Input.is_action_just_pressed("click_left"):
        if $DialogBox.call("Next"):
            if $InkStory.CanContinue:
                $InkStory.Continue()
            elif $InkStory.HasChoices:
                $InkStory.ChooseChoiceIndex(0)
            else:
                $DialogBox.call("ChangeText", "")

func _on_story_continued(text, tags):
    $DialogBox.call("ChangeText", text)