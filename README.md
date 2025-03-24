# Rubicon.Core

General purpose rhythm game framework add-on originally created for [Rubicon Engine](https://github.com/RubiconTeam/Rubicon). Also allows for use of RubiCharts.

Although this add-on will obviously have a bunch of remnants related to Rubicon Engine, this could be used as a base for ones looking to implement a rhythm game into their own game.

Though that being said, if you do master Rubicon.Core, you'd have no problem using Rubicon Engine.

## Setting up outside of Rubicon Engine
First off, you need a few things to get going.
- [Godot Engine 4.4-stable](https://godotengine.org/download/archive/4.4-stable/)  (No guarantee for older or newer versions.)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PukiTools.GodotSharp](https://github.com/Binpuki/PukiTools.GodotSharp)

### Instructions (With Git) (Recommended)
This method ensures that you can stay up-to-date with whatever branch you need Rubicon.Core on, no matter if it's the latest or another certain version.
1. Make sure you have Godot open along with the .NET SDK installed, and have your project with .NET capabilities open.
2. Navigate to your git project folder in your terminal of choice.
3. Add the PukiTools.GodotSharp repo as a submodule to your Godot Project (Example: `git submodule add https://github.com/RubiconTeam/Rubicon.Core.git`) and enable it as a plugin in Project Settings.
4. Add this repo as a submodule to the Godot Project as well (Example: `git submodule add https://github.com/Binpuki/PukiTools.GodotSharp addons/PukiTools.GodotSharp/`) and enableit as a plugin in Project Settings.
5. Click the Build button (the hammer icon on the top right), restart the editor for good measure, and go off!

Do keep in mind that each time you clone your project's repo, you will have to run `git submodule init recursive` the first time around, then `git submodule update --remote` to update the submodules.

### Instructions (No Git)
This method is the most easiest, but slightly tougher to maintain.
1. Make sure you have Godot open along with the .NET SDK installed, and have your project with .NET capabilities open.
2. Add PukiTools.GodotSharp as an add-on onto path `res://addons/PukiTools.GodotSharp` and enable the plugin in Project Settings.
3. Add the contents of this repo as an add-on at path `res://addons/Rubicon.Core/` and enable the plugin.
4. Click the Build button (the hammer icon on the top right), restart the editor for good measure, and go off!
