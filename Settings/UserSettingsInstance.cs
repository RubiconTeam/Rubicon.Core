using Godot.Collections;
using Rubicon.Core.Data;

namespace Rubicon.Core.Settings;

[GlobalClass, StaticAutoloadSingleton("Rubicon.Core.Settings", "UserSettings")]
public partial class UserSettingsInstance : Node
{
	private UserSettingsData _data;
	
	// i need to store this somewhere
	public static readonly string[] InputActionExclusionList =
	[
		"ui_accept", 
		"ui_select", 
		"ui_cancel", 
		"ui_focus_next",
		"ui_focus_prev",
		"ui_left",
		"ui_right",
		"ui_up",
		"ui_down",
		"ui_page_up",
		"ui_page_down",
		"ui_home",
		"ui_end",
		"ui_cut",
		"ui_copy",
		"ui_paste",
		"ui_undo",
		"ui_redo",
		"ui_text_completion_query",
		"ui_text_completion_accept",
		"ui_text_completion_replace",
		"ui_text_newline",
		"ui_text_newline_blank",
		"ui_text_newline_above",
		"ui_text_indent",
		"ui_text_dedent",
		"ui_text_backspace",
		"ui_text_backspace_word",
		"ui_text_backspace_word.macos",
		"ui_text_backspace_all_to_left.macos",
		"ui_text_delete",
		"ui_text_delete_word",
		"ui_text_delete_word.macos",
		"ui_text_delete_all_to_right.macos",
		"ui_text_caret_left",
		"ui_text_caret_word_left",
		"ui_text_caret_word_left.macos",
		"ui_text_caret_right",
		"ui_text_caret_word_right",
		"ui_text_caret_word_right.macos",
		"ui_text_caret_up",
		"ui_text_caret_down",
		"ui_text_caret_line_start",
		"ui_text_caret_line_start.macos",
		"ui_text_caret_line_end",
		"ui_text_caret_line_end.macos",
		"ui_text_caret_page_up",
		"ui_text_caret_page_down",
		"ui_text_caret_document_start",
		"ui_text_caret_document_start.macos",
		"ui_text_caret_document_end",
		"ui_text_caret_document_end.macos",
		"ui_text_caret_add_below",
		"ui_text_caret_add_below.macos",
		"ui_text_caret_add_above",
		"ui_text_caret_add_above.macos",
		"ui_text_scroll_up",
		"ui_text_scroll_up.macos",
		"ui_text_scroll_down",
		"ui_text_scroll_down.macos",
		"ui_text_select_all",
		"ui_text_select_word_under_caret",
		"ui_text_select_word_under_caret.macos",
		"ui_text_add_selection_for_next_occurrence",
		"ui_text_skip_selection_for_next_occurrence",
		"ui_text_clear_carets_and_selection",
		"ui_text_toggle_insert_mode",
		"ui_menu",
		"ui_text_submit",
		"ui_unicode_start",
		"ui_graph_duplicate",
		"ui_graph_delete",
		"ui_filedialog_up_one_level",
		"ui_filedialog_refresh",
		"ui_filedialog_show_hidden",
		"ui_swap_input_direction"
	];    

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
		foreach (var (curAction, events) in Bindings.Map)
		{
			InputMap.ActionEraseEvents(curAction);

			foreach (var t in events)
				InputMap.ActionAddEvent(curAction, t);
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
		_data = new UserSettingsData
		{
			Bindings =
			{
				Map = RubiconEngine.DefaultInputMap.Duplicate()
			}
		};
	}
	
	/// <inheritdoc cref="UserSettingsData.GetSetting"/>
	public Variant GetSetting(string key) => _data.GetSetting(key);

	/// <inheritdoc cref="UserSettingsData.SetSetting"/>
	public void SetSetting(string key, Variant value)
	{
		_data.SetSetting(key, value);
	}

	/// <inheritdoc cref="UserSettingsData.GetSections"/>
	public string[] GetSections() => _data.GetSections();

	/// <inheritdoc cref="UserSettingsData.GetSectionKeys"/>
	public string[] GetSectionKeys(string section) => _data.GetSectionKeys(section);

	/// <inheritdoc cref="UserSettingsData.GetAttributesForSetting"/>
	public AttributeData[] GetAttributesForSetting(string key) => _data.GetAttributesForSetting(key);
}
