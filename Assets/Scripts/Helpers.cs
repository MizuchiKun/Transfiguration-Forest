using System;
using TMPro.EditorUtilities;
using UnityEngine;

public static class Helpers
{
    #region Fields

    #endregion

    /// <summary>
    /// Maps the given value from the input range to the output range and optionally clamps it inside the output range.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <param name="inputMin">The min value of the input range.</param>
    /// <param name="inputMax">The max value of the input range.</param>
    /// <param name="outputMin">The min value of the output range.</param>
    /// <param name="outputMax">The max value of the output range.</param>
    /// <param name="doClamp">Whether to clamp the result to the output range. <br/>Defaults to true.</param>
    /// <returns><c>value</c> mapped to the output range.</returns>
    public static float Map(float value, float inputMin, float inputMax,
                            float outputMin, float outputMax, bool doClamp = true)
    {
        float inputInterval = inputMax - inputMin;
        float outputInterval = outputMax - outputMin;
        float absInputDifference = value - inputMin;
        float relInputDifference = absInputDifference / inputInterval;
        float mappedToOutput = outputMin + relInputDifference * outputInterval;
        if (doClamp)
            return Mathf.Clamp(mappedToOutput, outputMin, outputMax);
        else
            return mappedToOutput;
    }
}
