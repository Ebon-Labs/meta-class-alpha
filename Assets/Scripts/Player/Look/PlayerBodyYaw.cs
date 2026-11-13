using Mirror;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerBodyYaw : NetworkBehaviour
{
    Camera view;
    float carriedYaw;

    public static void Split(Quaternion look, out Quaternion bodyYaw, out Quaternion headPitch)
    {
        bodyYaw = Quaternion.Euler(0f, look.eulerAngles.y, 0f);
        headPitch = Quaternion.Inverse(bodyYaw) * look;
    }

    void LateUpdate()
    {
        Apply();
    }

    void Apply()
    {
        if (!isLocalPlayer)
            return;

        if (view == null)
            view = GetComponentInChildren<Camera>();
        if (view == null)
            return;

        Transform head = view.transform;
        Quaternion look = head == transform ? transform.rotation : head.localRotation;
        Split(look, out Quaternion bodyYaw, out Quaternion headPitch);
        if (!Application.isEditor)
            carriedYaw += FloorCarry.ConsumeYawDegrees();
        transform.rotation = bodyYaw * Quaternion.Euler(0f, carriedYaw, 0f);
        if (head != transform)
            head.localRotation = headPitch;
    }
}
