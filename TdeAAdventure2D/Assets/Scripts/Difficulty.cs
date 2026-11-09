using UnityEngine;

// Dificultad segun la altura (en metros). 0 = facil, 1 = maximo.
// Llega al maximo a los 400 m. Cambia 400f para hacerlo mas rapido o mas lento.
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
