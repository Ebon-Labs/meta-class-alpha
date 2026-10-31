using UnityEngine;

public static class BillboardScale
{
    public static float Uniform(float fontSize, float distance, float fieldOfViewDegrees, float screenHeight, float parentScale)
    {
        float halfFovRad = fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad;
        float worldHeight = (fontSize / screenHeight) * 2f * distance * Mathf.Tan(halfFovRad);
        return worldHeight / (fontSize * parentScale);
    }
}
