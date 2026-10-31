using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class VoiceMuteIndicator : MonoBehaviour
{
    [SerializeField] GameObject iconObject;
    [SerializeField] Image icon;
    [SerializeField] Sprite mutedSprite;

    bool _appliedMuted;

    void Awake()
    {
        if (icon == null && iconObject != null)
            icon = iconObject.GetComponent<Image>();

        Apply(force: true);
    }

    void LateUpdate()
    {
        Apply(force: false);
    }

    void Apply(bool force)
    {
        bool muted = ClassController.Instance != null && ClassController.Instance.MuteGuests;
        if (!force && muted == _appliedMuted)
            return;

        _appliedMuted = muted;

        if (icon != null)
        {
            icon.sprite = muted ? mutedSprite : null;
            icon.enabled = muted;
        }

        if (iconObject != null && iconObject.activeSelf != muted)
            iconObject.SetActive(muted);
    }
}
