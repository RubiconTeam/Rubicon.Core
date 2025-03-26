class_name GDHealthBar extends GDCustomBar

## A template for a health bar in GDScript. Must be inherited.

func initialize() -> void:
	if play_field == null:
		return
		
	direction = BarDirection.LEFT_TO_RIGHT if play_field.TargetIndex == 0 else BarDirection.RIGHT_TO_LEFT
	
func _process(delta: float) -> void:
	if play_field != null:
		progress_ratio = float(play_field.Health) / float(play_field.MaxHealth)
	
	super._process(delta)
