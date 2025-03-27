class_name GDCustomBar extends GDHudElement

## A template for a visual indicator that tracks a value.

@export var progress_ratio : float = 0.0 ## How much progress this bar has made from 0.0 to 1.0.
@export var direction : int:## What direction the bar filling goes.
	get:
		return _direction
	set(val):
		_direction = val
		change_direction(val)

@export var left_color : Color: ## The bar's color on the left side.
	get:
		return _left_color
	set(val):
		_left_color = val
		
		if affect_left_color:
			change_left_color(val)

@export var right_color : Color: ## The bar's color on the right side.
	get:
		return _right_color
	set(val):
		_right_color = val
		
		if affect_right_color:
			change_right_color(val)

@export var affect_left_color : bool = true ## Whether the left side would be affected by the color.
@export var affect_right_color : bool = true ## Whether the right side would be affected by the color.

var _left_color : Color = Color.RED
var _right_color : Color = Color.GREEN
var _direction : int = BarDirection.LEFT_TO_RIGHT

func _ready() -> void:
	change_direction(direction)

func _process(_delta: float) -> void:
	update_bar()

func update_bar() -> void: ## Invoked every frame to update the progress bar.
	pass

func change_left_color(left_color : Color) -> void: ## Changes the left side's color to the one provided.
	pass 
	
func change_right_color(right_color : Color) -> void: ## Changes the right side's color to the one provided.
	pass

func change_direction(direction : int) -> void: ## Changes the direction of the bar.
	pass