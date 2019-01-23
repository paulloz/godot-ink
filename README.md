# godot-ink

**This project is heavily under construction as C# support in Godot is a WIP.**

[Ink](https://github.com/inkle/ink) integration for [Godot Engine](https://github.com/godotengine/godot).  
As C# supprt in Godot is still in its early stages, you might need to manually add the addon files to your `.csproj`. 

### Currently supported features:
* Running an Ink story and branching with choice indexes
* Saving and loading Ink state
* Tags
* Knot/Stitch jumping
* Getting/Setting Ink variables (InkLists aren't supported yet)
* Observing Ink variables (Inklists aren't supported yet)
* External function bindings
* Read/Visit counts

### TODO:
* Getting/Setting/Observing InkLists
* On the fly Ink to JSON compilation 

## How to use

You'll need to put `ink-engine-runtime.dll` at the root of your Godot project.

Everything revolves around the `Story` node.

If nothing is specified, the **C#** usage is the same as the **GDScript** one.

### Loading the story

To load your story, you can:

* Point the `InkFilePath` exported variable to the location of your JSON Ink file and check the `AutoLoadStory` checkbox in the inspector.
* Point the `InkFilePath` exported variable to the location of your JSON Ink file (in the inspector or via a script) and call `story.LoadStory()`.
* Call `story.LoadStory(path)` with path pointing to the location of your JSON Ink file.

### Running the story and making choices

Getting content from the story is done by calling the `.Continue()` method.
```GDScript
var story = get_node("Story")
while story.CanContinue:
    print(story.Continue())
    # Alternatively, text can be accessed from story.CurrentText
```

Choices are made with the `.ChooseChoiceIndex(int)` method.
```GDScript
if story.HasChoices:
    for choice in story.CurrentChoices:
        print(choice)
    ...
    story.ChooseChoiceIndex(index)
```

### Using signals

If you don't want to bother accessing `CurrentText` and `CurrentChoices`, signals are emitted when the story continues forward and when a new choice appears.

```GDScript
    ...
    story.connect("InkContinued", self, "_on_story_continued")
    story.connect("InkChoices", self, "_on_choices")

func _on_story_continued(currentText, currentTags):
    print(currentText)

func _on_choices(currentChoices):
    for choice in choices:
        print(choice)
```

In **C#**, you can use the `nameof()` on the `[Signal]` delegates.

```C#
story.Connect(nameof(InkStory.InkContinued), this, "OnStoryContinued");
story.Connect(nameof(InkStory.InkChoices), this, "OnChoices");
```

The above signals are also available through the node inspector.

### Save / Load

You get and set the json state by calling `.GetState()` and `.SetState(String)`.

```GDScript
story.SetState(story.GetState())
```

Alternatively you can save and load directly from disk (either by passing a path or a file as argument) with `.LoadStateFromDisk` and `.SaveStateOnDisk`.

```GDScript
story.SaveStateOnDisk("save.json")
story.LoadStateFromDisk("save.json")
```

```GDScript
var file = File.new()
file.open("save.json", File.WRITE)
story.SaveStateOnDisk(file)
file.close


file.open("save.json", File.READ)
story.LoadStateFromDisk(file)
file.close
```

### Tags

Tags, global tags and knot tags are accessible respectively through `.CurrentTags`, `.GlobalTags` and `.TagsForContentAtPath(String)`.

```GDScript
print(story.CurrentTags)
print(story.GlobalTags)
print(story.TagsForContentAtPath("mycoolknot"))
```

As shown above, current tags are also passed along the current text in the `InkContinued` event.

### Jumping to a Knot/Stitch

You can [jump to a particular knot or stitch](https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md#jumping-to-a-particular-scene) with `.ChoosePathString(String)`. This method will return `false` if the jump failed.

```GDScript
if story.ChoosePathString("mycoolknot.myradstitch"):
    story.Continue()
```

### Using Ink variables

Ink variables (except InkLists for now) can be get and set.

```GDScript
story.GetVariable("foo")
story.SetVariable("foo", "bar")
```

They can also be observed with signals.

```GDScript
    ...
    story.connect(story.ObserveVariable("foo"), self, "_foo_observer")

func _foo_observer(varName, varValue):
    print(varName, " = ", varValue)
```

#### Read/Visit count

You can know how many times a knot/stitch has been visited with `.VisitCountPathString(String)`.

```GDScript
print(story.VisitCountPathString("mycoolknot.myradstitch"))
```

## License

**godot-ink** is released under MIT license (see the [LICENSE](/LICENSE) file for more information).

Example files in the `ink/` come from the [Writing with ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md) tutorial.

