using TMPro;
using UnityEngine;

public class SceneInfoPanel : MonoBehaviour
{
    [SerializeField] GameObject sceneInfo;
    [SerializeField] TMP_Text label;

    readonly InfoTextStack<Object> entries = new InfoTextStack<Object>();

    void Awake()
    {
        if (sceneInfo != null && entries.IsEmpty)
            sceneInfo.SetActive(false);
    }

    public void Show(Object source, string text)
    {
        if (source == null || sceneInfo == null)
            return;

        entries.Show(source, text);
        Apply();
    }

    public void Hide(Object source)
    {
        if (source == null)
            return;

        entries.Hide(source);
        Apply();
    }

    void Apply()
    {
        if (sceneInfo == null)
            return;

        if (!entries.TryGetCurrent(out Object current))
        {
            sceneInfo.SetActive(false);
            return;
        }

        if (label != null && entries.TryGetText(current, out string text))
            label.text = text;

        sceneInfo.SetActive(true);
    }
}
