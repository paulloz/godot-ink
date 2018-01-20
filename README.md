# godot-ink

**This project is heavily under construction as C# support in Godot is a WIP.**

[Ink](https://github.com/inkle/ink) integration for [Godot Engine](https://github.com/godotengine/godot).  
As C# custom types aren't available yet, this package includes an **InkStory** packed scene.

### Currently supported features:
* Running an Ink story and branching with choice indexes
* Knot/Stitch jumping
* Getting/Setting Ink variables (InkLists aren't supported yet)
* Observing Ink variables (Inklists aren't supported yet)
* External function bindings
* Read/Visit counts

### TODO:
* Getting/Setting/Observing InkLists
* Saving and loading Ink state
* Tags
* On the fly Ink to JSON compilation 

## How to use

You'll need to put `ink-engine-runtime.dll` at the root of your Godot project.

Everything revolves around the `InkStory` packed scene. For the sake of explanations, let's assume your scene contains an `InkStory` node called `story`.
You can also use it in as an AutoLoad.

You'll need to point its `InkFilePath` exported variable to the location of your JSON Ink file, whether from the inspector or from a script.  

If nothing is specified, the **C#** usage is the same as the **GDScript** one.

### Running the story and making choices

Getting content from the story is done by calling the `.Continue()` method.
```GDScript
var story = get_node("story")
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
    story.connect("ink-continued", self, "_on_story_continued")
    story.connect("ink-choices", self, "_on_choices")

func _on_story_continued(currentText):
    print(currentText)

func _on_choices(currentChoices):
    for choice in choices:
        print(choice)
```

In **C#**, you can use the `Signals` enum instead of signal names.

```C#
story.Connect(InkStory.Signals.Continued, this, "OnStoryContinued");
story.Connect(InkStory.Signals.Choices, this, "OnChoices");
```

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

