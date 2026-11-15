using Mirror;
using UnityEngine;

public class PlayerNameplate : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnShown))]
    string shown = "";

    public string Shown => shown;

    PlayerLabel label;

    void Awake()
    {
        label = GetComponentInChildren<PlayerLabel>(true);
        if (label != null)
            label.Show("");
    }

    public override void OnStartClient()
    {
        Show(shown);
    }

    public void Publish(string name)
    {
        shown = PlayerLabel.ShownText(name);
        Show(shown);
    }

    void OnShown(string previous, string next)
    {
        Show(next);
    }

    void Show(string name)
    {
        if (label == null)
            label = GetComponentInChildren<PlayerLabel>(true);

        if (label != null)
            label.Show(name);
    }
}
