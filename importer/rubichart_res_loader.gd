@tool

class_name RbcResourceFormatLoader

extends ResourceFormatLoader

const RbcV2_0_0_0 = preload("res://addons/Rubicon.Core/importer/loaders/rbc_2.0.gd")

func _get_recognized_extensions() -> PackedStringArray:
	return PackedStringArray(["rbc"])

func _get_resource_type(path: String) -> String:
	if path.get_extension().to_lower() == "rbc":
		return "Resource"

	return ""

func _handles_type(type: StringName) -> bool:
	return ClassDB.is_parent_class(type, "Resource")

func _load(path: String, original_path: String, use_sub_threads: bool, cache_mode: int):
	if not ResourceLoader.exists(path):
		return ERR_FILE_NOT_FOUND

	var reader : FileAccess = FileAccess.open(path, FileAccess.READ)
	var error : Error = reader.get_error()
	if error != OK:
		return error

	var chart : RubiChart = null
	var extension : String = path.get_extension().to_lower()
	if extension != "rbc":
		return ERR_INVALID_DATA

	var rbc_check : PackedByteArray = reader.get_buffer(4)
	if rbc_check.decode_u32(0) == 16842752: # Parse RubiChart v1.1 charts
		print("RBC file at " + reader.get_path() + " is version 1.1! Please convert it to at least RubiChart v2.0.0 or higher.")
		reader.close()
		return ERR_INVALID_DATA
	else:
		if not (rbc_check.get_string_from_utf8() == "RBCN"):
			return ERR_INVALID_DATA
		
		var version : int = reader.get_32()
		match version:
			16843008:
				print("RBC file at " + reader.get_path() + " is version 1.1.1! Please convert it to at least RubiChart v2.0.0 or higher.")
				reader.close()
				return ERR_INVALID_DATA
			33554432:
				chart = RbcV2_0_0_0.convert(reader)	
			_:
				chart = RbcV2_0_0_0.convert(reader)

	reader.close()
	return chart
