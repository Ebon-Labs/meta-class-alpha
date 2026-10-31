using UnityEngine;

public class MoveToEdge : MonoBehaviour
{
    public const float DefaultDuration = 7f;

    public static bool ShellVisible(bool traveling)
    {
        return traveling;
    }

    [SerializeField] Transform subject;
    [SerializeField] Transform destination;
    [SerializeField] float duration = DefaultDuration;
    [SerializeField] GameObject shownWhileMoving;

    Vector3 start;
    Vector3 fallbackForward;
    float elapsed;
    bool moving;

    public void OnPointerClick()
    {
        Begin();
    }

    public void Begin()
    {
        if (subject == null || destination == null)
            return;
        if (!BodyBounds.TryGet(destination, out _))
            return;

        start = subject.position;
        fallbackForward = subject.forward;
        elapsed = 0f;
        moving = true;
        SetShown(true);
    }

    void Update()
    {
        if (!moving)
            return;
        if (subject == null || destination == null || !BodyBounds.TryGet(destination, out Bounds bounds))
        {
            Finish();
            return;
        }

        elapsed += Time.deltaTime;
        subject.position = EdgeApproach.Position(start, bounds, fallbackForward, elapsed, duration);
        if (duration <= 0f || elapsed >= duration)
            Finish();
    }

    void OnDisable()
    {
        if (moving)
            Finish();
    }

    void Finish()
    {
        moving = false;
        SetShown(false);
    }

    void SetShown(bool active)
    {
        if (shownWhileMoving != null)
            shownWhileMoving.SetActive(ShellVisible(active));
    }
}
