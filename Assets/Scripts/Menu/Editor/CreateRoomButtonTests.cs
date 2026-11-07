using NUnit.Framework;
using UnityEngine;

public class CreateRoomButtonTests
{
    [Test]
    public void Ready_WaitsForFiveCharacters()
    {
        Assert.IsFalse(CreateRoomButton.Ready(null));
        Assert.IsFalse(CreateRoomButton.Ready(""));
        Assert.IsFalse(CreateRoomButton.Ready("abcd"));
        Assert.IsTrue(CreateRoomButton.Ready("abcde"));
        Assert.IsTrue(CreateRoomButton.Ready("sala norte"));
    }

    [Test]
    public void Alpha_StaysDimUntilTheAliasIsLongEnough()
    {
        Assert.AreEqual(CreateRoomButton.DisabledAlpha, CreateRoomButton.Alpha("abcd"));
        Assert.AreEqual(1f, CreateRoomButton.Alpha("abcde"));
        Assert.AreEqual(CreateRoomButton.DisabledAlpha, CreateRoomButton.Alpha("abcde".Substring(0, 4)));
        Assert.IsTrue(CreateRoomButton.IsCaption("Entrar\n\n"));
        Assert.IsFalse(CreateRoomButton.IsCaption("Voltar"));

        Color tint = CreateRoomButton.WithAlpha(new Color(0.2f, 0.4f, 0.6f, 0.1f), "abcde");
        Assert.AreEqual(0.2f, tint.r, 0.001f);
        Assert.AreEqual(0.4f, tint.g, 0.001f);
        Assert.AreEqual(0.6f, tint.b, 0.001f);
        Assert.AreEqual(1f, tint.a, 0.001f);
    }
}
