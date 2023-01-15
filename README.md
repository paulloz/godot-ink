# godot-ink

An [ink](https://github.com/inkle/ink) integration for [Godot Engine](https://github.com/godotengine/godot).  

## Notice

This branch contains the new version of **godot-ink** for Godot 4. It is under heavy development.  

If you still want to try the current implementation, here's the installation process.

* Clone the [ink repository](https://github.com/inkle/ink) somewhere on your computer.
* Copy `addons/GodotInk/` into your `addons/` folder.
* Reference the addon's `.props` file in your `.csproj`.
```xml
  <Import Project="addons/GodotInk/GodotInk.props" />
```
* Create a `.csproj.user` file alongside your `.csproj` file. This file should not be tracked in VCS, must have the same name as your `.csproj` file and contain the following.
```xml
<Project>
  <PropertyGroup>
    <InkProjectFolder>/path/to/the/ink/repository</InkProjectFolder>
  </PropertyGroup>
</Project>
```
* Build once.
* Enable the addon.

## License

*godot-ink* is released under MIT license (see the [LICENSE](/LICENSE) file for more information).
