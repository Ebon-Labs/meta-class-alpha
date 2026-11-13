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
    Vector3 boneUp;
    Vector3 boneForward;
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

    public static float LookPitch(Quaternion look)
    {
        float rise = (look * Vector3.forward).y;
        return -Mathf.Asin(Mathf.Clamp(rise, -1f, 1f)) * Mathf.Rad2Deg;
    }

    public static float LookRoll(Quaternion look)
    {
        Vector3 forward = look * Vector3.forward;
        if (forward.sqrMagnitude < 0.000001f)
            return 0f;
        if (Mathf.Abs(Vector3.Dot(forward.normalized, Vector3.up)) > 0.999f)
            return 0f;

        Quaternion upright = Quaternion.LookRotation(forward, Vector3.up);
        Vector3 rolledUp = (Quaternion.Inverse(upright) * look) * Vector3.up;
        return Vector3.SignedAngle(Vector3.up, rolledUp, Vector3.forward);
    }

    public static Vector3 UpAxis(Transform boneParent, Transform player)
    {
        return AxisInParent(boneParent, player, player != null ? player.up : Vector3.up, Vector3.up);
    }

    public static Vector3 EarAxis(Vector3 localUp, Vector3 localForward)
    {
        Vector3 ear = Vector3.Cross(localUp, localForward);
        if (ear.sqrMagnitude < 0.000001f)
            return Vector3.right;
        return ear.normalized;
    }

    public static Quaternion Nod(Quaternion rest, float pitch, Vector3 localUp, Vector3 localForward)
    {
        return rest * Quaternion.AngleAxis(pitch, EarAxis(localUp, localForward));
    }

    static Vector3 AxisInParent(Transform boneParent, Transform player, Vector3 worldAxis, Vector3 fallback)
    {
        if (boneParent == null || player == null)
            return fallback;

        Vector3 axis = boneParent.InverseTransformDirection(worldAxis);
        if (axis.sqrMagnitude < 0.000001f)
            return fallback;
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

    public static void StopIndependentMotion(GameObject player)
    {
        if (player == null)
            return;

        Animator[] animators = player.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
                continue;

            animator.runtimeAnimatorController = null;
            animator.enabled = false;
        }

        Animation[] clips = player.GetComponentsInChildren<Animation>(true);
        for (int i = 0; i < clips.Length; i++)
        {
            Animation clip = clips[i];
            if (clip == null)
                continue;

            clip.Stop();
            clip.playAutomatically = false;
            clip.clip = null;
            clip.enabled = false;
        }
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

    void Awake()
    {
        StopIndependentMotion(gameObject);
    }

    void Update()
    {
        StopIndependentMotion(gameObject);
    }

    void LateUpdate()
    {
        StopIndependentMotion(gameObject);
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
            boneUp = bone.InverseTransformDirection(transform.up);
            boneForward = bone.InverseTransformDirection(transform.forward);
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
        float pitch = CameraEditorController.ClampPitch(LookPitch(view.transform.localRotation));
        Quaternion nodded = Nod(boneRest, pitch, boneUp, boneForward);
        bone.localRotation = WithYaw(nodded, pose.Head, UpAxis(bone.parent, transform));
    }
}
