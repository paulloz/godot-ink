# godot-ink

**This project is heavily under construction as C# support in Godot is a WIP.**

[Ink](https://github.com/inkle/ink) integration for [Godot Engine](https://github.com/godotengine/godot).  
As C# custom types aren't available yet, this package includes a **InkStory** packed scene.

### Currently supported features:
* Running an Ink story and branching with choice indexes
* Getting/Setting Ink variables (InkLists aren't supported yet)
* Observing Ink variables (Inklists aren't supported yet)

### TODO:
* Getting/Setting/Observing InkLists
* External function bindings
* Saving and loading Ink state
* Read/Visit counts
* Knot/Stitch jumping
* Tags
* On the fly Ink to JSON compilation 

## Usage

You'll need to put `ink-engine-runtime.dll` at the root of your Godot project.

## License

**godot-ink** is released under MIT license (see the [LICENSE](/LICENSE) file for more information).

Example files in the `ink/` come from the [Writing with ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md) tutorial.

