using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomUtils
{
    // In this context, range.x is the low value of the range and range.y is the high value.
    public static float GenerateRandom(Vector2 range)
    {
        return UnityEngine.Random.Range(range.x, range.y);
    }

    public static float GenerateRandom(float inputRange, float percentVariation)
    {
        float min = inputRange * (1 - percentVariation);
        float max = inputRange * (1 + percentVariation);

        return UnityEngine.Random.Range(min, max);
    }

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
