using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot.Collections;
using Rubicon.Core.Settings.Attributes;
using Array = Godot.Collections.Array;

namespace Rubicon.Core.Settings;

[GlobalClass, StaticAutoloadSingleton("Rubicon.Core.Settings", "UserSettings")]
public partial class UserSettingsInstance : Node
{
	private UserSettingsData _data;

	public override void _Ready()
	{
		if (Load() != Error.Ok)
		{
			GD.Print("Failed to load settings. Resetting to defaults.");
			Reset();
			Save();
		}

		UpdateSettings();
		UpdateKeybinds();
	}

	public void UpdateSettings()
	{
		Window mainWindow = GetTree().GetRoot();
		mainWindow.Mode = Video.Fullscreen;
		DisplayServer.WindowSetVsyncMode(Video.VSync);
		Engine.MaxFps = Video.MaxFps;
		mainWindow.Scaling3DMode = Video.Settings3D.Scaling3DMode;
		mainWindow.FsrSharpness = Video.Settings3D.FsrSharpness;
	}

	public void UpdateKeybinds()
	{
		foreach (var bind in Bindings.Map)
		{
			string curAction = bind.Key;
			Array<InputEvent> events = bind.Value;

			InputMap.ActionEraseEvents(curAction);

			for (int i = 0; i < events.Count; i++)
			{
				InputMap.ActionAddEvent(curAction, events[i]);
			}
		}
	}

	public Error Load(string path = null)
	{
		path ??= ProjectSettings.GetSetting("rubicon/general/settings_save_path").AsString();
		if (!FileAccess.FileExists(path))
		{
			GD.PrintErr("Settings file not found.");
			return Error.FileNotFound;
		}

		ConfigFile config = new();
		Error loadError = config.Load(path);
		if (loadError != Error.Ok)
		{
			GD.PrintErr($"Failed to load settings file: {loadError}");
			return loadError;
		}

		_data = new UserSettingsData();
		_data.Load(config);
		
		return Error.Ok;
	}
	
	public Error Save(string path = null)
	{
		path ??= ProjectSettings.GetSetting("rubicon/general/settings_save_path").AsString();
		ConfigFile configFile = _data.CreateConfigFileInstance();
		return configFile.Save(path);
	}

	public void Reset()
	{
		_data = new UserSettingsData();
	}

	public Variant GetSetting(string key)
	{
		return _data.GetSetting(key);
	}

	public void SetSetting(string key, Variant value)
	{
		_data.SetSetting(key, value);
	}
}
