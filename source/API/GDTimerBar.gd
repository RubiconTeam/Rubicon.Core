class_name GDTimerBar extends GDCustomBar

## A template for a bar that tracks the time left in-game.

@export var length : float = 0.0 ## The length of the instrumental in seconds. Is modifiable just in case you want to screw around with it.

func initialize() -> void:
	if play_field == null:
		return
		
	var music : AudioStreamPlayer = play_field.Music	
	length = music.stream.get_length()

func _process(delta: float) -> void:
	if play_field != null:
		progress_ratio = Conductor.GetRawTime() / length
	
	super._process(delta)
