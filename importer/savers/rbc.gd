extends RefCounted

# RubiChart v2.0.0
static func save(chart : RubiChart, writer : FileAccess) -> void:
	var note_types : Array[StringName] = []
	var type_index_map : Dictionary[StringName, int] = {}

	# Precache note types 🙄
	for ind_chart : ChartData in chart.Charts:
		for section : SectionData in ind_chart.Sections:
			for row : RowData in section.Rows:
				for note : NoteData in row.Notes:
					var has_type : bool = note.Type.to_lower() != "normal"
					if not has_type or type_index_map.has(note.Type):
						continue
					
					note_types.push_back(note.Type)
					type_index_map.set(note.Type, note_types.size() - 1)

		for j in ind_chart.Strays.size():
			var cur_note : NoteData = ind_chart.Strays[j]
			var has_type : bool = cur_note.Type.to_lower() != "normal"
			if not has_type or type_index_map.has(cur_note.Type):
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
		var individual_chart : ChartData = chart.Charts[i]
		
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

		var section_count : int = individual_chart.Sections.size()
		writer.store_32(section_count)
		for s in section_count:
			var cur_section : SectionData = individual_chart.Sections[s]
			writer.store_32(cur_section.Measure)
			
			var row_count : int = cur_section.Rows.size()
			writer.store_32(row_count)
			for r in row_count:
				var cur_row : RowData = cur_section.Rows[r]
				cur_row.Section = cur_section
				writer.store_8(cur_row.Quant)
				writer.store_8(cur_row.Offset)
				writer.store_8(cur_row.LanePriority)
				
				var note_count : int = cur_row.Notes.size()
				writer.store_8(note_count)
				for n in note_count:
					var cur_note : NoteData = cur_row.Notes[n]
					var note_data : int = cur_note.Lane
					
					var is_hold : bool = cur_note.EndingRow != null
					var is_ending_row : bool = is_hold and cur_row == cur_note.EndingRow
					if is_ending_row:
						writer.store_8(note_data)
						continue
					
					if is_hold:
						note_data = (1 << 7) | note_data
					
					var has_type : bool = cur_note.Type.to_lower() != "normal"
					if has_type:
						note_data = (1 << 6) | note_data
					
					var has_params : bool = cur_note.Parameters != null and not cur_note.Parameters.is_empty()
					if has_params:
						note_data = (1 << 5) | note_data	

					writer.store_8(note_data)
					if has_type:
						writer.store_32(type_index_map[cur_note.Type])
					
					if has_params:
						write_note_parameters(writer, cur_note)

		# Strays
		var stray_count : int = individual_chart.Strays.size()
		writer.store_32(stray_count)
		for s in stray_count:
			var note : NoteData = individual_chart.Strays[s]
			var serialized_type : int = get_serialized_type(note)
			writer.store_8(serialized_type)
			
			writer.store_float(note.MeasureTime)
			writer.store_32(note.Lane)
			
			if serialized_type >= 4: # Is hold note
				writer.store_float(note.MeasureLength)
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
	if note.MeasureLength > 0.0:
		offset += 4

	return get_serialized_tap_note_type(note) + offset

static func get_serialized_tap_note_type(note : NoteData) -> int:
	if note.Type.to_lower() != "normal":
		if note.Parameters.size() > 0:
			return 3 # Typed tap note with params

		return 1 # Typed tap note

	if note.Parameters.size() > 0:
		return 2 # Tap note with params

	return 0 # Normal tap note
