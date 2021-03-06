﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides utility functions to generate random floating point values within a specified range.
/// </summary>
public class RandomUtils
{
    /// <summary>
    /// Generates a random floating point value within the range specified.
    /// In this context, range.x is the low value of the range and range.y is the high value.
    /// </summary>
    /// <param name="range">
    /// Defines the range within which the randomly generated floating point value must lie.
    /// </param>
    /// <returns>
    /// The randomly generated floating point value.
    /// </returns>
    public static float GenerateRandom(Vector2 range)
    {
        float return_value = UnityEngine.Random.Range(range.x, range.y);
        return return_value;
    }

    /// <summary>
    /// Generate a random value above and below the value specified in inputRange.
    /// </summary>
    /// <param name="inputRange">
    /// The value from which the newly generated value must vary.
    /// </param>
    /// <param name="percentVariation">
    /// The percent of variation above and below the specified value of inputRange.
    /// </param>
    /// <returns>
    /// The randomly generated floating point value.
    /// </returns>
    public static float GenerateRandom(float inputRange, float percentVariation)
    {
        float min = inputRange * (1 - percentVariation);
        float max = inputRange * (1 + percentVariation);

        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    /// Randomly generate the low/high values of a range, using a specified range and
    /// percent of variation from the specified range.
    /// </summary>
    /// <param name="inputRange">
    /// The range from which the new range will be generated by specified percent of variation.
    /// </param>
    /// <param name="percentVariation">
    /// The allowed percentage of variation from the specified low and high values
    /// in the randomly generated low and high values of the new range.
    /// </param>
    /// <returns>
    /// Low and high values of the randomly generated numeric range.
    /// </returns>
    public static Vector2 GenerateRandomRange(Vector2 inputRange, float percentVariation)
    {
        float minLow, maxLow;
        float minHigh, maxHigh;
        float inputRangeWidth = inputRange.y - inputRange.x;
        Vector2 returnRandomRange = new Vector2();

        minLow = inputRange.x - percentVariation * inputRangeWidth;
        maxLow = inputRange.x + percentVariation * inputRangeWidth;
        minHigh = inputRange.y - percentVariation * inputRangeWidth;
        maxHigh = inputRange.y + percentVariation * inputRangeWidth;

        minLow = Mathf.Max(0, minLow);
        minHigh = Mathf.Max(0, minHigh);

        returnRandomRange.x = UnityEngine.Random.Range(minLow, maxLow);
        returnRandomRange.y = UnityEngine.Random.Range(minHigh, maxHigh);

        return (returnRandomRange);
    }
}
