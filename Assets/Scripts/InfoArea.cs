using Mirror;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InfoArea : MonoBehaviour
{
    const string LoremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

    [TextArea(3, 8)]
    [SerializeField] string body = LoremIpsum;

    SphereCollider volume;
    bool localPlayerInside;

    void Awake()
    {
        volume = GetComponent<SphereCollider>();
        volume.isTrigger = true;
    }

    void OnDisable()
    {
        if (!localPlayerInside)
            return;

        localPlayerInside = false;
        FindLocalPanel()?.Hide(this);
    }

    void Update()
    {
        SceneInfoPanel panel = FindLocalPanel();
        bool inside = panel != null && Contains(panel.transform.position);

        if (inside == localPlayerInside)
            return;

        localPlayerInside = inside;

        if (panel == null)
            return;

        if (inside)
            panel.Show(this, body);
        else
            panel.Hide(this);
    }

    bool Contains(Vector3 worldPoint)
    {
        if (volume == null)
            return false;

        Vector3 local = transform.InverseTransformPoint(worldPoint);
        float radius = Mathf.Max(0.001f, volume.radius);
        return (local - volume.center).sqrMagnitude <= radius * radius;
    }

    static SceneInfoPanel FindLocalPanel()
    {
        if (!NetworkClient.active || NetworkClient.localPlayer == null)
            return null;

        return NetworkClient.localPlayer.GetComponent<SceneInfoPanel>();
    }
}
