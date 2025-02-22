class_name GDStatDisplay

extends Control

## A template for a UI statistics (i.e. combo, judgment) element in GDScript. Must be inherited.

@export var ui_style : UiStyle: ## A reference to the UI Style.
	get:
		return play_field.UiStyle

var play_field : PlayField
var _initialized : bool = false

func initialize() -> void: ## If it hasn't been initialized already, add itself to the play field.
	if _initialized:
		return
		
	play_field.StatisticsUpdated.connect(update_stats)
	_initialized = true

func update_stats(_combo : int, _hit : int, _distance : float) -> void: ## Triggers when the player either hits or misses a note. Must be inherited!
	pass

func get_hit_material(hit : int) -> Material: ## A helper method to get the material based on the hit type.
	match hit:
		0:
			return ui_style.PerfectMaterial
		1:
			return ui_style.GreatMaterial
		2:
			return ui_style.GoodMaterial
		3:
			return ui_style.OkayMaterial
		4:
			return ui_style.BadMaterial
		5:
			return ui_style.MissMaterial

	return null

func get_hit_name(hit : int) -> StringName: ## A helper method to get the name based on the hit type.
	match hit:
		0:
			return &"Perfect"
		1:
			return &"Great"
		2:
			return &"Good"
		3:
			return &"Okay"
		4:
			return &"Bad"
		5:
			return &"Miss"
		_:
			return ""
