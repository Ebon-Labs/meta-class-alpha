using UnityEngine;

public static class EditorMouseLook
{
    public static void Accumulate(ref float yaw, ref float pitch, float mouseX, float mouseY, float sensitivity)
    {
        yaw += mouseX * sensitivity;
        pitch -= mouseY * sensitivity;
    }

    public static Vector3 Euler(float yaw, float pitch)
    {
        return new Vector3(pitch, yaw, 0f);
    }
}
