/*
 * Copyright 2024 Rubicon Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

global using Godot;
global using System;
using System.Collections.Generic;
using System.IO;
using Godot.Collections;
using GodotSharp.Utilities;
using Rubicon.Core;
using Rubicon.Core.Data;

namespace Rubicon;

/// <summary>
/// A Node that contains basic engine info. Essentially the Main class.
/// More useful in GDScript than it is in C#.
/// </summary>
[GlobalClass, Autoload("RubiconEngine")]
public partial class RubiconEngineInstance : Node
{
	/// <summary>
	/// The current version of Rubicon being used.
	/// </summary>
	public static readonly VersionInfo Version = new(0, 2, 0, 0, "-alpha");
	
	/// <summary>
	/// The minimum aspect ratio the viewport can scale down to.
	/// </summary>
	[Export] public float MinimumAspectRatio = ProjectSettings.GetSetting("rubicon/general/minimum_aspect_ratio").AsSingle();
	
	/// <summary>
	/// The minimum aspect ratio the viewport can scale up to.
	/// </summary>
	[Export] public float MaximumAspectRatio = ProjectSettings.GetSetting("rubicon/general/maximum_aspect_ratio").AsSingle();

	[Export] public Godot.Collections.Dictionary<StringName, string> NoteTypePaths = new();
	
	[Export] public Godot.Collections.Dictionary<string, Array<InputEvent>> DefaultInputMap = new();

	private Window _mainWindow;

	private float _minimumAspectRatio;
	private float _maximumAspectRatio;

	private Vector2I _viewportSize;
	private Vector2I _previousWindowSize;
	
	public override void _Ready()
	{
		_mainWindow = GetWindow();
		_viewportSize = new Vector2I(ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32(), ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32());

		Array<StringName> actionNames = InputMap.GetActions();
		foreach (string actionName in actionNames) 
			DefaultInputMap[actionName] = InputMap.ActionGetEvents(actionName);
		
		List<string> noteTypePaths = [];
		noteTypePaths.AddRange(PathUtility.GetAbsoluteFilePathsAt("res://Resources/Game/Notetypes/", true));
		for (int i = 0; i < noteTypePaths.Count; i++)
		{
			string path = noteTypePaths[i];
			string noteType = Path.GetFileNameWithoutExtension(path.GetFile());
			string ext = path.GetExtension().ToLower();
			if (NoteTypePaths.ContainsKey(noteType) || noteType.ToLower() == "normal" || (ext != "tscn" && ext != "scn" && ext != "cs" && ext != "gd"))
				continue;
			
			NoteTypePaths.Add(noteType, path);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Vector2I windowSize = _mainWindow.Size;
		if (_previousWindowSize == windowSize)
			return;

		float aspectRatio = Mathf.Clamp(windowSize.Aspect(), MinimumAspectRatio, MaximumAspectRatio);
		_mainWindow.ContentScaleSize = new Vector2I(Mathf.FloorToInt(_viewportSize.Y * aspectRatio), _viewportSize.Y);
	}

	/// <inheritdoc cref="Version"/>
	public VersionInfo GetVersion() => Version;
}
