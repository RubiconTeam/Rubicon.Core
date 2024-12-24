using System.Linq;
using Rubicon.Core.Chart;

namespace Rubicon.Core;

/// <summary>
/// Helper functions for Conductor and time-based stuff.
/// </summary>
public static class ConductorUtility
{
    /// <summary>
    /// Converts a measure to milliseconds based on the number of beats in a measure.
    /// </summary>
    /// <param name="measure">The measure</param>
    /// <param name="bpm">The BPM</param>
    /// <param name="timeSignatureNumerator">The number of beats in a measure.</param>
    /// <returns>The measure, in milliseconds.</returns>
    public static float MeasureToMs(float measure, float bpm, float timeSignatureNumerator = 4f)
    {
        return measure * (60000f / (bpm / timeSignatureNumerator));
    }

    /// <summary>
    /// Converts milliseconds to measures based on a list of BPM changes.
    /// </summary>
    /// <param name="msTime">The time in milliseconds</param>
    /// <param name="bpmList">The bpm list (<see cref="RubiChart.ConvertData"/> needs to be invoked beforehand!)</param>
    /// <returns>The milliseconds, in measures</returns>
    public static float MsToMeasures(float msTime, BpmInfo[] bpmList)
    {
        BpmInfo bpm = bpmList.Last();
        for (int i = 0; i < bpmList.Length; i++)
        {
            if (bpmList[i].MsTime > msTime)
            {
                bpm = bpmList[i - 1];
                break;
            }
        }

        float measureValue = MeasureToMs(1, bpm.Bpm, bpm.TimeSignatureNumerator);
        float offset = msTime - bpm.MsTime;
        return bpm.Time + (offset / measureValue);
    }

    /// <summary>
    /// Converts measures to beats.
    /// </summary>
    /// <param name="measure">The measure.</param>
    /// <param name="timeSignatureNumerator">The number of beats in a measure.</param>
    /// <returns>The measure, in beats.</returns>
    public static float MeasureToBeats(float measure, float timeSignatureNumerator = 4f)
    {
        return measure * timeSignatureNumerator;
    }

    /// <summary>
    /// Converts measures to steps.
    /// </summary>
    /// <param name="measure">The measure</param>
    /// <param name="timeSignatureNumerator">The number of beats in a measure.</param>
    /// <param name="timeSignatureDenominator">The type of note which equals one beat.</param>
    /// <returns>The measure, in steps.</returns>
    public static float MeasureToSteps(float measure, float timeSignatureNumerator = 4f, float timeSignatureDenominator = 4f)
    {
        return measure * timeSignatureNumerator * timeSignatureDenominator;
    }

    /// <summary>
    /// Converts beats to steps.
    /// </summary>
    /// <param name="beats">The beats</param>
    /// <param name="timeSignatureDenominator">The type of note which equals one beat.</param>
    /// <returns>The beats, in steps.</returns>
    public static float BeatsToSteps(float beats, float timeSignatureDenominator = 4f)
    {
        return beats * timeSignatureDenominator;
    }

    /// <summary>
    /// Converts steps to measures.
    /// </summary>
    /// <param name="steps">The steps.</param>
    /// <param name="timeSignatureNumerator">The number of beats in a measure.</param>
    /// <param name="timeSignatureDenominator">The type of note which equals one beat.</param>
    /// <returns>The steps, in measures.</returns>
    public static float StepsToMeasures(float steps, float timeSignatureNumerator = 4f, float timeSignatureDenominator = 4f)
    {
        return steps / (timeSignatureNumerator * timeSignatureDenominator);
    }
}