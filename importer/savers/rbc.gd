extends RefCounted

# RubiChart v1.1.1
static func save(chart : RubiChart, writer : FileAccess) -> void:
	# Cache note types
	var note_types : Array[StringName] = []
	var type_index_map : Dictionary[StringName, int] = {}
	for i in chart.Charts.size():
		var individual_chart : IndividualChart = chart.Charts[i]
		for j in individual_chart.Notes.size():
			var cur_note : NoteData = individual_chart.Notes[j]
			if note_types.has(cur_note.Type) or cur_note.Type == &"normal":
				continue
			
			note_types.push_back(cur_note.Type)
			type_index_map.set(cur_note.Type, note_types.size() - 1)
	
	writer.store_buffer("RBCN".to_utf8_buffer())
	writer.store_32(chart.Version)
	writer.store_32(chart.Difficulty)
	writer.store_float(chart.ScrollSpeed)

	var charter_bytes : PackedByteArray = chart.Charter.to_utf8_buffer()
	writer.store_32(charter_bytes.size())
	writer.store_buffer(charter_bytes)
	
	writer.store_32(note_types.size())
	for i in note_types.size():
		var note_type_bytes : PackedByteArray = note_types[i].to_utf8_buffer()
		writer.store_32(note_type_bytes.size())
		writer.store_buffer(note_type_bytes)
	
	writer.store_32(chart.Charts.size())
	for i in chart.Charts.size():
		var individual_chart : IndividualChart = chart.Charts[i]
		
		var name_bytes : PackedByteArray = individual_chart.Name.to_utf8_buffer()
		writer.store_32(name_bytes.size())
		writer.store_buffer(name_bytes)
		
		writer.store_32(individual_chart.Lanes)
		
		writer.store_32(individual_chart.Switches.size())
		for j in individual_chart.Switches.size():
			var target_switch : TargetSwitch = individual_chart.Switches[j]
			writer.store_float(target_switch.Time)
			
			var target_name_bytes : PackedByteArray = target_switch.Name.to_utf8_buffer()
			writer.store_32(target_name_bytes.size())
			writer.store_buffer(target_name_bytes)
		
		writer.store_32(individual_chart.SvChanges.size())
		for j in individual_chart.SvChanges.size():
			var sv_change : SvChange = individual_chart.SvChanges[j]
			writer.store_float(sv_change.Time)
			writer.store_float(sv_change.Multiplier)

		writer.store_32(individual_chart.Notes.size())
		for n in individual_chart.Notes.size():
			var note : NoteData = individual_chart.Notes[n]
			var serialized_type : int = get_serialized_type(note)
			writer.store_8(serialized_type)

			writer.store_float(note.Time)
			writer.store_32(note.Lane)

			if serialized_type >= 4: # Is hold note
				writer.store_float(note.Length)
				serialized_type -= 4

			match serialized_type:
				1: # Typed note
					writer.store_32(type_index_map[note.Type])	
				2: # Note with params
					write_note_parameters(writer, note)
				3: # Typed note with params
					writer.store_32(type_index_map[note.Type])
					write_note_parameters(writer, note)

static func write_note_parameters(writer : FileAccess, note : NoteData) -> void:
	writer.store_32(note.Parameters.size())
	
	var keys : Array[StringName] = note.Parameters.keys()
	for k in keys.size(): 
		var param_name_bytes : PackedByteArray = keys[k].to_utf8_buffer()
		writer.store_32(param_name_bytes.size())
		writer.store_buffer(param_name_bytes)
		
		var param_value_bytes : PackedByteArray = var_to_bytes(note.Parameters[keys[k]])
		writer.store_32(param_value_bytes.size())
		writer.store_buffer(param_value_bytes)
		
static func get_serialized_type(note : NoteData) -> int:
	var offset : int = 0
	if note.Length > 0.0:
		offset += 4
		
	return get_serialized_tap_note_type(note) + offset
	
static func get_serialized_tap_note_type(note : NoteData) -> int:
	if note.Type != &"normal":
		if note.Parameters.size() > 0:
			return 3 # Typed tap note with params

		return 1 # Typed tap note

	if note.Parameters.size() > 0:
		return 2 # Tap note with params

	return 0 # Normal tap note
