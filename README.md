# godot-ink

**This project is heavily under construction as C# support in Godot is a WIP.**

[Ink](https://github.com/inkle/ink) integration for [Godot Engine](https://github.com/godotengine/godot).  
As C# custom types aren't available yet, this package includes an **InkStory** packed scene.

### Currently supported features:
* Running an Ink story and branching with choice indexes
* Getting/Setting Ink variables (InkLists aren't supported yet)
* Observing Ink variables (Inklists aren't supported yet)
* External function bindings

### TODO:
* Getting/Setting/Observing InkLists
* Saving and loading Ink state
* Read/Visit counts
* Knot/Stitch jumping
* Tags
* On the fly Ink to JSON compilation 

## How to use

You'll need to put `ink-engine-runtime.dll` at the root of your Godot project.

Everything revolves around the `InkStory` packed scene. For the sake of explanations, let's assume your scene contains an `InkStory` node called `story`.

You'll need to point its `InkFilePath` exported variable to the location of your JSON Ink file, whether from the inspector or from a script.

### Running the story and making choices

Getting content from the story is done by calling the `.Continue()` method.
```C#
// From C#
InkStory story = GetNode("story") as InkStory;
while (story.CanContinue()) {
    GD.Print(story.Continue());
    // Alternatively, text can be accessed from story.CurrentText
}
```
```GDScript
# From GDScript
var story = get_node("story")
while story.CanContinue():
    print(story.Continue())
    # Alternatively, text can be accessed from story.get("CurrentText")
```

Choices are made with the `.ChooseChoiceIndex(int)` method.
```C#
// From C#
if (story.HasChoices()) {
    for (short i = 0; i < story.CurrentChoices.Count; ++i) {
        GD.Print(story.CurrentChoices[i]);
    }
    ...
    story.ChooseChoiceIndex(index);
}
```
```GDScript
# From GDScript
if story.HasChoices():
    for choice in story.CurrentChoices:
        print(choice)
    ...
    story.ChooseChoiceIndex(index)
```

### Using signals

If you don't want to bother accessing `CurrentText` and `CurrentChoices`, signals are emitted when the story continues forward and when a new choice appears.

```C#
// From C#
...
{
    ...
    story.Connect(InkStory.Signals.Continued, this, "OnStoryContinued");
    story.Connect(InkStory.Signals.Choices, this, "OnChoices");
}

public void OnStoryContinued(String text)
{
    GD.Print(text);
}

public void OnChoices(String[] choices)
{
    foreach (String choice in choices) {
        GD.Print(choice);
    }
}
```
```GDScript
# From GDScript
    ...
    story.connect("ink-continued", self, "_on_story_continued")
    story.connect("ink-choices", self, "_on_choices")

func _on_story_continued(currentText):
    print(currentText)

func _on_choices(currentChoices):
    for choice in choices:
        print(choice)
```

### Using Ink variables

Ink variables (except InkLists for now) can be get and set.

```C#
// From C#
story.GetVariable("foo");
story.SetVariable("foo", "bar");
```
```GDScript
# From GDScript
story.GetVariable("foo")
story.SetVariable("foo", "bar")
```

They can also be observed with signals.
```C#
// From C#
...
{
    ...
    story.Connect(story.ObserveVariable("foo"), this, "FooObserver");
}

public void FooObserver(String varName, String varValue)
{
    GD.Print(String.Format("{0} == {1}", varName, varValue));
}
```
```GDScript
# From GDScript
    ...
    story.connect(story.ObserveVariable("foo"), self, "_foo_observer")

func _foo_observer(varName, varValue):
    print(varName, " = ", varValue)
```


## License

**godot-ink** is released under MIT license (see the [LICENSE](/LICENSE) file for more information).

Example files in the `ink/` come from the [Writing with ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md) tutorial.

