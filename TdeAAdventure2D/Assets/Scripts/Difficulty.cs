using UnityEngine;

public static class Difficulty
{
    public const float MetersPerLevel = 50f;

    public static float At(float height)
    {
        return Mathf.Clamp01(height / 400f);
    }

    public static int Level(float height)
    {
        return 1 + Mathf.FloorToInt(Mathf.Max(0f, height) / MetersPerLevel);
    }
}
