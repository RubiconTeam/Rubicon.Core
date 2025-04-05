class_name RubiChartConstants

## The current chart version.
static var chart_version : VersionInfo:
	get:
		var version : VersionInfo = VersionInfo.new()
		version.SetVersion(2, 0, 0, 0, "")
		return version

## The max lane count a chart can have.
static var max_lane_count : int = 32

## Types of quants available for RubiChart.
static var quants : PackedByteArray = [4, 8, 12, 16, 24, 32, 48, 64, 192];