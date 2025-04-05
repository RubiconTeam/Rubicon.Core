# For loading RubiChart 2.0
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
	var charts : Array[ChartData] = []
	for i in amtOfCharts:
		var individual_chart : ChartData = ChartData.new()
		
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
		
		var hold_note_cache : Dictionary[int, NoteData]= {}
		var section_count : int = int(reader.get_32())
		print(section_count)
		
		var sections : Array[SectionData] = []
		while section_count > 0:
			section_count -= 1
	
			var cur_section : SectionData = SectionData.new()
			cur_section.Measure = int(reader.get_32())
			sections.push_back(cur_section)

			var row_count : int = int(reader.get_32())
			var rows : Array[RowData] = []
			while row_count > 0:
				row_count -= 1
	
				var cur_row : RowData = RowData.new()
				cur_row.Section = cur_section
				cur_row.Quant = reader.get_8()
				cur_row.Offset = reader.get_8()
				cur_row.LanePriority = reader.get_8()

				rows.push_back(cur_row)

				var note_count : int = reader.get_8()
				var notes : Array[NoteData] = []
				while note_count > 0:
					note_count -= 1
	
					var note_data : int = reader.get_8()
					var lane : int = note_data & 0b00011111
					var measure_time : float = cur_section.Measure + float(cur_row.Offset) / cur_row.Quant
					if hold_note_cache.has(lane) and hold_note_cache[lane] != null:
						var hold_note : NoteData = hold_note_cache[lane]
						hold_note.EndingRow = cur_row
						hold_note_cache[lane] = null
						notes.push_back(hold_note)
						continue
					
					var is_hold : bool = ((note_data & 0b10000000) >> 7) == 1
					var has_type : bool = ((note_data & 0b01000000) >> 6) == 1
					var has_params : bool = ((note_data & 0b00100000) >> 5) == 1

					var cur_note : NoteData = NoteData.new()
					cur_note.Lane = lane
					
					if is_hold:
						hold_note_cache[lane] = cur_note
			
					if has_type:
						cur_note.Type = note_types[int(reader.get_32())]

					if has_params:
						read_note_parameters(reader, cur_note)

					cur_note.StartingRow = cur_row
					notes.push_back(cur_note)

				cur_row.Notes = notes

			cur_section.Rows = rows

		individual_chart.Sections = sections

		# Strays
		var note_count : int = int(reader.get_32())
		var notes : Array[NoteData] = []
		for j in note_count:
			var note : NoteData = NoteData.new()
			notes.push_back(note)
		
			var serialized_type : int = reader.get_8()
			note.MeasureTime = reader.get_float()
			note.Lane = int(reader.get_32())
		
			if serialized_type >= 4: # Is hold note
				note.MeasureLength = reader.get_float()
				serialized_type -= 4
		
			match serialized_type:
				1: # Typed note
					note.Type = note_types[int(reader.get_32())]
				2: # Note with params
					read_note_parameters(reader, note)
				3: # Typed note with params
					note.Type = note_types[int(reader.get_32())]
					read_note_parameters(reader, note)
			
		individual_chart.Strays = notes
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
