using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot.Collections;
using Rubicon.Core.Data;
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
		}

		Save();
		UpdateSettings();
		UpdateBinds();
	}

	public void UpdateSettings()
	{
		Window mainWindow = GetTree().GetRoot();
		mainWindow.Mode = Video.Fullscreen;
		DisplayServer.WindowSetVsyncMode(Video.VSync);
		Engine.MaxFps = Video.MaxFps;
		mainWindow.Scaling3DMode = Video.Settings3D.Scaling3DMode;
		mainWindow.FsrSharpness = Video.Settings3D.FsrSharpness;
		
		AudioServer.SetBusVolumeLinear(0, (float)Audio.MasterVolume);
		AudioServer.SetBusVolumeLinear(1, (float)Audio.MusicVolume);
		AudioServer.SetBusVolumeLinear(2, (float)Audio.SfxVolume);
	}

	public void UpdateBinds()
	{
		foreach (var bind in Bindings.Map)
		{
			string curAction = bind.Key;
			Array<InputEvent> events = bind.Value;

			InputMap.ActionEraseEvents(curAction);

			for (int i = 0; i < events.Count; i++)
				InputMap.ActionAddEvent(curAction, events[i]);
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

		Reset();
		_data.Load(config);
		
		return Error.Ok;
	}
	
	/// <summary>
	/// Saves the current UserSettings to the provided path.
	/// </summary>
	/// <param name="path">The path to save it to.</param>
	/// <returns>An error, if any.</returns>
	public Error Save(string path = null)
	{
		path ??= ProjectSettings.GetSetting("rubicon/general/settings_save_path").AsString();
		ConfigFile configFile = _data.CreateConfigFileInstance();
		return configFile.Save(path);
	}

	/// <summary>
	/// Resets all settings.
	/// </summary>
	public void Reset()
	{
		_data = new UserSettingsData();
		_data.Bindings.Map = RubiconEngine.DefaultInputMap.Duplicate();
	}

	/// <inheritdoc cref="UserSettingsData.GetSetting"/>
	public Variant GetSetting(string key)
	{
		return _data.GetSetting(key);
	}

	/// <inheritdoc cref="UserSettingsData.SetSetting"/>
	public void SetSetting(string key, Variant value)
	{
		_data.SetSetting(key, value);
	}

	/// <inheritdoc cref="UserSettingsData.GetSections"/>
	public string[] GetSections()
	{
		return _data.GetSections();
	}

	/// <inheritdoc cref="UserSettingsData.GetSectionKeys"/>
	public string[] GetSectionKeys(string section)
	{
		return _data.GetSectionKeys(section);
	}

	/// <inheritdoc cref="UserSettingsData.GetAttributesForSetting"/>
	public AttributeData[] GetAttributesForSetting(string key)
	{
		return _data.GetAttributesForSetting(key);
	}
}
