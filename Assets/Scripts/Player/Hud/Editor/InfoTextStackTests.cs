using NUnit.Framework;

public class InfoTextStackTests
{
    sealed class Source
    {
    }

    [Test]
    public void Show_LastEntryWins()
    {
        InfoTextStack<Source> stack = new InfoTextStack<Source>();
        Source first = new Source();
        Source second = new Source();

        stack.Show(first, "Mercurio");
        stack.Show(second, "Venus");

        Assert.IsTrue(stack.TryGetCurrent(out Source current));
        Assert.AreSame(second, current);
        Assert.IsTrue(stack.TryGetText(current, out string text));
        Assert.AreEqual("Venus", text);
    }

    [Test]
    public void BlankText_RemovesTheSource()
    {
        InfoTextStack<Source> stack = new InfoTextStack<Source>();
        Source source = new Source();
        stack.Show(source, "Terra");
        stack.Show(source, "   ");

        Assert.IsTrue(stack.IsEmpty);
    }

    [Test]
    public void Hide_DropsOnlyThatSource()
    {
        InfoTextStack<Source> stack = new InfoTextStack<Source>();
        Source kept = new Source();
        Source removed = new Source();
        stack.Show(kept, "Marte");
        stack.Show(removed, "Jupiter");
        stack.Hide(removed);

        Assert.AreEqual(1, stack.Count);
        Assert.IsTrue(stack.TryGetCurrent(out Source current));
        Assert.AreSame(kept, current);
    }

    [Test]
    public void NullSource_IsIgnored()
    {
        InfoTextStack<Source> stack = new InfoTextStack<Source>();
        stack.Show(null, "Sol");
        stack.Hide(null);

        Assert.IsTrue(stack.IsEmpty);
    }
}
