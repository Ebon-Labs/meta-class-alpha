using NUnit.Framework;
using TMPro;
using UnityEngine;

public class PlayerNameTests
{
    [Test]
    public void Ready_WaitsForTwoCharacters()
    {
        Assert.IsFalse(PlayerName.Ready(null));
        Assert.IsFalse(PlayerName.Ready(""));
        Assert.IsFalse(PlayerName.Ready("a"));
        Assert.IsTrue(PlayerName.Ready("ab"));
        Assert.AreEqual(PlayerName.DisabledAlpha, PlayerName.Alpha("a"));
        Assert.AreEqual(1f, PlayerName.Alpha("ab"));
    }

    [Test]
    public void MainButtons_StayClosedUntilTheNameIsLongEnough()
    {
        Assert.IsTrue(PlayerName.Gates(CarouselMenu.CreateName));
        Assert.IsTrue(PlayerName.Gates(CarouselMenu.EnterName));
        Assert.IsTrue(PlayerName.Gates(CarouselMenu.TutorialName));
        Assert.IsFalse(PlayerName.Gates(CarouselMenu.CreateBackName));
        Assert.IsFalse(PlayerName.Gates(ImageMenuThumb.ObjectName));

        Assert.IsFalse(PlayerName.Allows(CarouselMenu.CreateName, "a"));
        Assert.IsFalse(PlayerName.Allows(CarouselMenu.EnterName, ""));
        Assert.IsFalse(PlayerName.Allows(CarouselMenu.TutorialName, null));
        Assert.IsTrue(PlayerName.Allows(CarouselMenu.CreateName, "ab"));
        Assert.IsTrue(PlayerName.Allows(CarouselMenu.EnterName, "ab"));
        Assert.IsTrue(PlayerName.Allows(CarouselMenu.TutorialName, "ab"));
        Assert.IsTrue(PlayerName.Allows(CarouselMenu.EnterBackName, ""));
    }

    [Test]
    public void Remember_KeepsTheNameAfterTheFieldIsGone()
    {
        var input = new GameObject(PlayerName.InputName);
        try
        {
            input.AddComponent<TMP_InputField>().text = "Ana";
            PlayerName.Remember();
            Object.DestroyImmediate(input);
            input = null;

            Assert.AreEqual("Ana", PlayerName.Remembered());
        }
        finally
        {
            if (input != null)
                Object.DestroyImmediate(input);
        }
    }
}
