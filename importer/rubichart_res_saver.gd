@tool

class_name RbcResourceFormatSaver

extends ResourceFormatSaver

const Rbc = preload("res://addons/Rubicon.Core/importer/savers/rbc.gd")

func _get_recognized_extensions(resource : Resource) -> PackedStringArray:
	return PackedStringArray(["rbc"])

func _recognize(resource : Resource) -> bool:
	return resource is RubiChart

func _save(resource : Resource, path : String, flags : int) -> Error:
	if resource is not RubiChart:
		return FAILED
	
	var writer : FileAccess = FileAccess.open(path, FileAccess.WRITE)
	var error : Error = writer.get_error()
	if error != OK:
		return error
	
	var chart : RubiChart = resource as RubiChart
	var extension : String = path.get_extension().to_lower()
	match extension:
		"rbc":
			Rbc.save(chart, writer)

	writer.close()
	return OK
