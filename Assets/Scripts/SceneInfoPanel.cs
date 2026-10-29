using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SceneInfoPanel : MonoBehaviour
{
    [SerializeField] GameObject sceneInfo;
    [SerializeField] TMP_Text label;

    readonly List<Object> sources = new List<Object>();
    readonly Dictionary<Object, string> texts = new Dictionary<Object, string>();

    void Awake()
    {
        if (sceneInfo != null && sources.Count == 0)
            sceneInfo.SetActive(false);
    }

    public void Show(Object source, string text)
    {
        if (source == null || sceneInfo == null)
            return;

        if (string.IsNullOrWhiteSpace(text))
        {
            Hide(source);
            return;
        }

        texts[source] = text;
        sources.Remove(source);
        sources.Add(source);
        Apply();
    }

    public void Hide(Object source)
    {
        if (source == null)
            return;

        sources.Remove(source);
        texts.Remove(source);
        Apply();
    }

    void Apply()
    {
        if (sceneInfo == null)
            return;

        if (sources.Count == 0)
        {
            sceneInfo.SetActive(false);
            return;
        }

        Object current = sources[sources.Count - 1];
        if (label != null && texts.TryGetValue(current, out string text))
            label.text = text;

        sceneInfo.SetActive(true);
    }
}
