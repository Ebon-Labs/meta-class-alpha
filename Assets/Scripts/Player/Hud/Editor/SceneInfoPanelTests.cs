using NUnit.Framework;

public class SceneInfoPanelTests
{
    sealed class Source
    {
    }

    [Test]
    public void ShowingTheSameSourceAgain_MovesItToTheFront()
    {
        InfoTextStack<Source> stack = new InfoTextStack<Source>();
        Source room = new Source();
        Source planet = new Source();
        stack.Show(room, "Sala");
        stack.Show(planet, "Terra");
        stack.Show(room, "Sala atualizada");

        Assert.IsTrue(stack.TryGetCurrent(out Source current));
        Assert.AreSame(room, current);
        Assert.IsTrue(stack.TryGetText(current, out string text));
        Assert.AreEqual("Sala atualizada", text);
        Assert.AreEqual(2, stack.Count);
    }

    [Test]
    public void HidingTheCurrentSource_RevealsThePreviousOne()
    {
        InfoTextStack<Source> stack = new InfoTextStack<Source>();
        Source first = new Source();
        Source second = new Source();
        stack.Show(first, "primeiro");
        stack.Show(second, "segundo");
        stack.Hide(second);

        Assert.IsTrue(stack.TryGetCurrent(out Source current));
        Assert.AreSame(first, current);
        Assert.IsFalse(stack.IsEmpty);
    }
}
