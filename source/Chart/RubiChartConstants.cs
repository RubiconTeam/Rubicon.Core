using Rubicon.Core.Data;

namespace Rubicon.Core.Chart;

public static class RubiChartConstants
{
    /// <summary>
    /// The current chart version.
    /// </summary>
    public static readonly VersionInfo ChartVersion = new(2, 0, 0, 0);

    /// <summary>
    /// The max lane count a chart can have.
    /// </summary>
    public const int MaxLaneCount = 32;

    /// <summary>
    /// Types of quants available for RubiChart.
    /// </summary>
    public static readonly byte[] Quants = [4, 8, 12, 16, 24, 32, 48, 64, 192];
}