class_name GDNoteType

extends Node

## A template for a note type in GDScript. Must be inherited.

@export var should_miss : bool = false
@export var counts_toward_score : bool = true

var play_field : PlayField

var _initialized : bool = false

func initialize(): ## If it hasn't been initialized already, link itself to the play field
	if _initialized:
		return 
		
	var factory : NoteFactory = play_field.Factory
	
	factory.NoteSpawned.connect(spawn_note)
	play_field.InitializeNote.connect(initialize_note) 
	play_field.ModifyResult.connect(note_hit)
	
	_initialized = true
 
func initialize_note(_notes : Array[NoteData], _note_type : StringName) -> void: ## Used to set up note data initially for every note type.
	if (_note_type != name):
		return
		
	for note in _notes:
		note.ShouldMiss = should_miss
		note.CountsTowardScore = counts_toward_score

func spawn_note(_note : Note, _note_type : StringName) -> void: ## Triggers when the factory spawns a note of this type. Use this to set up your note.
	pass

func note_hit(_bar_line_name : StringName, _result : NoteResult) -> void: ## Triggers every time a note of this type is hit.
	pass
