using System.Linq;

namespace Rubicon.Core.Chart;

/// <summary>
/// Keeps track of a scroll velocity change.
/// </summary>
[GlobalClass] public partial class SvChange : Resource
{
    /// <summary>
    /// The time to execute this scroll velocity change, in milliseconds.
    /// </summary>
    public float MsTime = 0f;

    /// <summary>
    /// The starting position of this scroll velocity change. Is set by <see cref="RubiChart.ConvertData">RubiChart.ConvertData</see>.
    /// </summary>
    public float Position = 0f;
    
    /// <summary>
    /// The measure to execute this scroll velocity change.
    /// </summary>
    [Export] public float Time = 0;

    /// <summary>
    /// The scroll velocity value
    /// </summary>
    [Export] public float Multiplier = 1f;

    public void ConvertData(TimeChange[] bpmInfo, SvChange previousChange = null)
    {
        TimeChange bpm = bpmInfo.Last();
        for (int i = 0; i < bpmInfo.Length; i++)
        {
            if (bpmInfo[i].Time > Time)
            {
                bpm = bpmInfo[i - 1];
                break;
            }
        }

        MsTime = ConductorUtility.MeasureToMs(Time - bpm.Time, bpm.Bpm, bpm.TimeSignatureNumerator) + bpm.MsTime;
        if (previousChange != null)
        {
            float previousEndingPoint = (float)(previousChange.Position + (MsTime - previousChange.MsTime) * previousChange.Multiplier);
            Position = previousEndingPoint;
            // Position = (float)((previousChange.Position + ) + ((MsTime - previousChange.MsTime) * Multiplier));
        }
    }
}