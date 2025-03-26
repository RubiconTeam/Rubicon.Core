class_name GDHudElement extends Control

## A GDScript variant HUD element for use in-game in [PlayField].

var play_field : PlayField ## A reference to the current [PlayField]. Automatically set by [PlayField].

func initialize() -> void: ## Triggers when added to [PlayField]
	pass

func options_updated() -> void: ## Triggers to make any adjustments for options that may have been changed.
	pass
