# For loading RubiChart v1.1
static func convert(reader : FileAccess) -> RubiChart:
	# Assuming this reader is at position 1 currently
	
	var chart : RubiChart = RubiChart.new()
	chart.Difficulty = reader.get_32()
	chart.ScrollSpeed = reader.get_float()
	
	var charter_length : int = int(reader.get_32())	
	chart.Charter = reader.get_buffer(charter_length).get_string_from_utf8()
	
	var note_types_length : int = int(reader.get_32())
	var note_types : PackedStringArray = []
	for i in note_types_length:
		var type_length : int = int(reader.get_32())
		note_types.push_back(reader.get_buffer(type_length).get_string_from_utf8())
	
	var amtOfCharts : int = int(reader.get_32())
	var charts : Array[IndividualChart] = []
	for i in amtOfCharts:
		var individual_chart : IndividualChart = IndividualChart.new()
		
		var name_length : int = int(reader.get_32())
		individual_chart.Name = reader.get_buffer(name_length).get_string_from_utf8()
		individual_chart.Lanes = int(reader.get_32())
		
		var target_switch_count : int = int(reader.get_32())
		var target_switches : Array[TargetSwitch] = []
		for j in target_switch_count:
			var target_switch : TargetSwitch = TargetSwitch.new()
			target_switch.Time = reader.get_float()
			
			var ts_name_length : int = int(reader.get_32())
			target_switch.Name = reader.get_buffer(ts_name_length).get_string_from_utf8()
			target_switches.push_back(target_switch)
		
		individual_chart.Switches = target_switches
		
		var sv_change_count : int = int(reader.get_32())
		var sv_changes : Array[SvChange] = []
		for j in sv_change_count:
			var sv_change : SvChange = SvChange.new()
			sv_change.Time = reader.get_float()
			sv_change.Multiplier = reader.get_float()
			sv_changes.push_back(sv_change)
		
		individual_chart.SvChanges = sv_changes
		
		var note_count : int = int(reader.get_32())
		var notes : Array[NoteData] = []
		for j in note_count:
			var note : NoteData = NoteData.new()
			notes.push_back(note)
			
			var serialized_type : int = reader.get_8()
			note.Time = reader.get_float()
			note.Lane = int(reader.get_32())

			if serialized_type >= 4: # Is hold note
				note.Length = reader.get_float()
				serialized_type -= 4
			
			match serialized_type:
				1: # Typed note
					note.Type = note_types[int(reader.get_32())]
				2: # Note with params
					read_note_parameters(reader, note)
				3: # Typed note with params
					note.Type = note_types[int(reader.get_32())]
					read_note_parameters(reader, note)
		
		individual_chart.Notes = notes
		charts.push_back(individual_chart)
	
	chart.Charts = charts
	
	return chart

static func read_note_parameters(reader : FileAccess, note : NoteData) -> void:
	var param_count : int = int(reader.get_32())
	for k in param_count:
		var param_name_length : int = int(reader.get_32())
		var param_name : StringName = reader.get_buffer(param_name_length).get_string_from_utf8()
		var param_value_length : int = int(reader.get_32())
		var param_value : Variant = bytes_to_var(reader.get_buffer(param_value_length))
		note.Parameters.set(param_name, param_value)
