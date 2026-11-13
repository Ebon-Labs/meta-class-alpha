using UnityEngine;

[DisallowMultipleComponent]
public class TeleportReveal : MonoBehaviour
{
    public GameObject Next;

    public static void Complete(GameObject current)
    {
        if (current == null)
            return;

        TeleportReveal reveal = current.GetComponent<TeleportReveal>();
        if (reveal == null)
            return;

        Apply(current, reveal.Next);
    }

    public static void Apply(GameObject current, GameObject next)
    {
        if (current != null)
            current.SetActive(false);
        if (next != null)
            next.SetActive(true);
    }
}
