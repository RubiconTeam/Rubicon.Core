## Helper functions for Conductor and time-based stuff.
##
## ConductorUtility's functions ported into GDScript.
class_name ConductorUtility

## Converts a measure to milliseconds based on the number of beats in a measure.
static func measures_to_ms(measure:float, bpm:float, time_signature_numerator:float = 4) -> float:
	return measure * (60000 / (bpm / time_signature_numerator))

## Converts a beat to milliseconds based on the current BPM.
static func beats_to_ms(beat:float, bpm:float) -> float:
	return beat * (60000 / bpm)

## Converts a step to milliseconds based on the current BPM.
static func steps_to_ms(step:float, bpm:float, time_signature_denominator = 4) -> float:
	return step * (60000 / bpm / time_signature_denominator)

## Converts milliseconds to measures based on a list of BPM changes.
static func ms_to_measures(ms_time:float, bpm_list:Array[BpmInfo]) -> float:
	var bpm:BpmInfo = bpm_list[-1]
	for i:int in bpm_list.size():
		if bpm_list[i].MsTime > ms_time:
			bpm = bpm_list[i - 1]
			break
	
	var measure_value:float = measures_to_ms(1, bpm.Bpm, bpm.TimeSignatureNumerator)
	var offset:float = ms_time - bpm.MsTime
	return bpm.Time + (offset / measure_value)

## Converts measures to beats.
static func measures_to_beats(measure:float, time_signature_numerator:float = 4) -> float:
	return measure * time_signature_numerator

## Converts measures to steps.
static func measures_to_steps(measure:float, time_signature_numerator:float = 4, time_signature_denominator:float = 4) -> float:
	return beats_to_steps(measures_to_beats(measure, time_signature_numerator), time_signature_denominator)

## Converts beats to steps.
static func beats_to_steps(beats:float, time_signature_denominator:float = 4) -> float:
	return beats * time_signature_denominator

## Converts beats to measures.
static func beats_to_measures(beats:float, time_signature_numerator:float = 4) -> float:
	return beats / time_signature_numerator

## Converts steps to measures.
static func steps_to_measures(steps:float, time_signature_numerator:float = 4, time_signature_denominator:float = 4) -> float:
	return steps / (time_signature_numerator * time_signature_denominator)
