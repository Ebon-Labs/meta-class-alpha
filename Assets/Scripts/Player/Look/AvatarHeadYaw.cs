using Mirror;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class AvatarHeadYaw : NetworkBehaviour
{
    public const float LimitDegrees = 45f;
    public const string BoneName = "head";
    public const string BodyName = "Avatar";

    Transform bone;
    Transform body;
    Quaternion boneRest;
    Quaternion bodyRest;
    float bodyYaw;
    float lookYaw;
    bool captured;

    public readonly struct YawPose
    {
        public readonly float Body;
        public readonly float Head;

        public YawPose(float body, float head)
        {
            Body = body;
            Head = head;
        }
    }

    public static float ClampYaw(float yaw, float limit)
    {
        return Mathf.Clamp(Mathf.DeltaAngle(0f, yaw), -limit, limit);
    }

    public static YawPose Follow(float bodyYaw, float lookYaw, float limit)
    {
        float delta = Mathf.DeltaAngle(bodyYaw, lookYaw);
        float head = Mathf.Clamp(delta, -limit, limit);
        float body = bodyYaw + (delta - head);
        return new YawPose(Mathf.DeltaAngle(0f, body), head);
    }

    public static float LookYaw(Quaternion look)
    {
        Vector3 forward = look * Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.000001f)
            return 0f;
        return Vector3.SignedAngle(Vector3.forward, forward, Vector3.up);
    }

    public static Vector3 UpAxis(Transform boneParent, Transform player)
    {
        if (boneParent == null || player == null)
            return Vector3.up;

        Vector3 axis = boneParent.InverseTransformDirection(player.up);
        if (axis.sqrMagnitude < 0.000001f)
            return Vector3.up;
        return axis.normalized;
    }

    public static Quaternion WithYaw(Quaternion rest, float yaw, Vector3 axis)
    {
        if (axis.sqrMagnitude < 0.000001f)
            axis = Vector3.up;
        return Quaternion.AngleAxis(yaw, axis.normalized) * rest;
    }

    public static Transform FindBone(Transform root, string boneName)
    {
        if (root == null || string.IsNullOrEmpty(boneName))
            return null;

        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].GetComponent<Camera>() != null)
                continue;
            if (all[i].name == boneName)
                return all[i];
        }

        return null;
    }

    public static Transform FindBody(Transform player, Transform bone)
    {
        if (player == null || bone == null)
            return null;

        Transform body = bone;
        while (body.parent != null && body.parent != player)
            body = body.parent;

        if (body == null || body.parent != player || body.GetComponent<Camera>() != null)
            return null;
        return body;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (!captured)
        {
            bone = FindBone(transform, BoneName);
            if (bone == null)
                return;
            body = FindBody(transform, bone);
            if (body == null)
                return;
            boneRest = bone.localRotation;
            bodyRest = body.localRotation;
            captured = true;
        }

        Camera view = GetComponentInChildren<Camera>();
        if (view == null)
            return;

        Vector3 flat = view.transform.localRotation * Vector3.forward;
        flat.y = 0f;
        if (flat.sqrMagnitude >= 0.000001f)
            lookYaw = LookYaw(view.transform.localRotation);

        YawPose pose = Follow(bodyYaw, lookYaw, LimitDegrees);
        bodyYaw = pose.Body;
        body.localRotation = WithYaw(bodyRest, bodyYaw, Vector3.up);
        Vector3 axis = UpAxis(bone.parent, transform);
        bone.localRotation = WithYaw(boneRest, pose.Head, axis);
    }
}
