extends Node

func _ready():
    set_process(true)
    InkStory.connect("ink-continued", self, "_on_story_continued")
    InkStory.connect("ink-choices", self, "_on_choices")
    InkStory.LoadStory("ink/example.json")
    InkStory.Continue();
    
func _process(delta):
    if Input.is_action_just_pressed("click_left"):
        if get_node("DialogBox").call("Next"):
            if InkStory.CanContinue:
                InkStory.Continue()
            else:
                get_node("DialogBox").call("ChangeText", "")

func _on_choices(choices):
    pass

func _on_story_continued(text):
    get_node("DialogBox").call("ChangeText", text)