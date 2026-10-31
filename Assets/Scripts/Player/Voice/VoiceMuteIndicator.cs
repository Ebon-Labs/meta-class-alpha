using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class VoiceMuteIndicator : MonoBehaviour
{
    [SerializeField] GameObject iconObject;
    [SerializeField] Image icon;
    [SerializeField] Sprite mutedSprite;

    bool appliedMuted;

    public static bool ShouldShowIcon(bool hasClassroom, bool guestsMuted)
    {
        return hasClassroom && guestsMuted;
    }

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
        bool muted = ShouldShowIcon(ClassController.Instance != null, ClassController.Instance != null && ClassController.Instance.MuteGuests);
        if (!force && muted == appliedMuted)
            return;

        appliedMuted = muted;

        if (icon != null)
        {
            icon.sprite = muted ? mutedSprite : null;
            icon.enabled = muted;
        }

        if (iconObject != null && iconObject.activeSelf != muted)
            iconObject.SetActive(muted);
    }
}
