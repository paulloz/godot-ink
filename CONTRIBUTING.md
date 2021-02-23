# How to contribute

# Testing and filling issues

If you experience any unexpected behaviour while using godot-ink, please submit a [GitHub issue](https://github.com/paulloz/godot-ink/issues/new/choose) using the appropriate template.  
Before your submission, take a minute to search if your problem has already been reported.  

# Submitting changes

Please send a [GitHub Pull Request](https://github.com/paulloz/godot-ink/pull/new/master) with a clear list of what you've done.  
Please follow the coding conventions (below) and make sure your commits are split in a sensible way.  
Always write a clear log message for your commits. One-line messages are fine for small changes, but bigger changes should look like this:
```
$ git commit -m "A brief summary of the commit
>
> A description of what changed and its impact."
```
If your Pull Request is related to another [GitHub issue](https://github.com/paulloz/godot-ink/issues), please reference its #id in your commit log, but refrain from using the [closing syntax](https://docs.github.com/en/github/managing-your-work-on-github/linking-a-pull-request-to-an-issue#linking-a-pull-request-to-an-issue-using-a-keyword).

# Coding conventions

* You should indent using four spaces (soft tabs)
* GDScript code should name everything using snake_case
* C# code should name public members using PascalCase and everything else using camelCase
* Curly braces are mandatory and should follow K&R style
* Operators should be surrounder by spaces
* Comments are nice and healthy
* In general, keep in mind your code will be read by others and should be tidy and nice to look at
