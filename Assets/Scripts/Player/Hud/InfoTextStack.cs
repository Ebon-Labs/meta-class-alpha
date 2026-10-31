using System.Collections.Generic;

public sealed class InfoTextStack<TKey> where TKey : class
{
    readonly List<TKey> sources = new List<TKey>();
    readonly Dictionary<TKey, string> texts = new Dictionary<TKey, string>();

    public int Count => sources.Count;

    public bool IsEmpty => sources.Count == 0;

    public void Show(TKey source, string text)
    {
        if (source == null)
            return;

        if (string.IsNullOrWhiteSpace(text))
        {
            Hide(source);
            return;
        }

        texts[source] = text;
        sources.Remove(source);
        sources.Add(source);
    }

    public void Hide(TKey source)
    {
        if (source == null)
            return;

        sources.Remove(source);
        texts.Remove(source);
    }

    public bool TryGetCurrent(out TKey source)
    {
        source = null;
        if (sources.Count == 0)
            return false;

        source = sources[sources.Count - 1];
        return true;
    }

    public bool TryGetText(TKey source, out string text)
    {
        return texts.TryGetValue(source, out text);
    }
}
