class_name GDStatDisplay extends GDHudElement

## A template for a UI statistics (i.e. combo, judgment) element in GDScript. Must be inherited.

@export var data : BarLineElementData ## A reference to the element data.

var _initialized : bool = false

func initialize() -> void: ## If it hasn't been initialized already, add itself to the play field.
	if _initialized:
		return
		
	play_field.ScoreManager.StatisticsUpdated.connect(update_stats)
	_initialized = true

func update_stats(_combo : int, _hit : int, _distance : float) -> void: ## Triggers when the player either hits or misses a note. Must be inherited!
	pass

func get_hit_name(hit : int) -> StringName: ## A helper method to get the name based on the hit type.
	match hit:
		Judgment.PERFECT:
			return &"Perfect"
		Judgment.GREAT:
			return &"Great"
		Judgment.GOOD:
			return &"Good"
		Judgment.OKAY:
			return &"Okay"
		Judgment.BAD:
			return &"Bad"
		Judgment.MISS:
			return &"Miss"
		_:
			return ""
