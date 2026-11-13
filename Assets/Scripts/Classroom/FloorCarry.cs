using UnityEngine;

public static class FloorCarry
{
    public static float PendingYawDegrees { get; private set; }

    public static Vector3 Position(
        Vector3 playerPosition,
        Vector3 fromPosition,
        Quaternion fromRotation,
        Vector3 toPosition,
        Quaternion toRotation)
    {
        Vector3 offset = Quaternion.Inverse(fromRotation) * (playerPosition - fromPosition);
        return toPosition + toRotation * offset;
    }

    public static Quaternion Rotation(Quaternion playerRotation, Quaternion fromRotation, Quaternion toRotation)
    {
        return toRotation * Quaternion.Inverse(fromRotation) * playerRotation;
    }

    public static float YawDegrees(Quaternion fromRotation, Quaternion toRotation)
    {
        float yaw = (toRotation * Quaternion.Inverse(fromRotation)).eulerAngles.y;
        if (yaw > 180f)
            yaw -= 360f;
        return yaw;
    }

    public static void AddYaw(float degrees)
    {
        PendingYawDegrees += degrees;
    }

    public static float ConsumeYawDegrees()
    {
        float yaw = PendingYawDegrees;
        PendingYawDegrees = 0f;
        return yaw;
    }
}
