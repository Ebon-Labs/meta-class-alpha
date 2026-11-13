using UnityEngine;

[DisallowMultipleComponent]
public class RotateClassButton : MonoBehaviour
{
    public const string ObjectName = "RotateClassButton";

    public static bool IsTarget(Transform hit)
    {
        for (Transform current = hit; current != null; current = current.parent)
        {
            if (current.name == ObjectName || current.GetComponent<RotateClassButton>() != null)
                return true;
        }

        return false;
    }

    public static void Turn()
    {
        if (RoomSync.RequestTurn())
            return;

        ClassController classController = ClassController.ResolveExisting(FindFirstObjectByType<ClassController>());
        if (classController == null)
            return;

        classController.TurnClockwise();
    }

    public void OnPointerClick()
    {
        Turn();
    }
}
