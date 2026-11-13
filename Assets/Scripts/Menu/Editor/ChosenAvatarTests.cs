using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ChosenAvatarTests
{
    [Test]
    public void PrefabFor_UsesThePortraitEachPlayerChose()
    {
        var girl = new GameObject(ChosenAvatar.GirlPrefabName);
        var boy = new GameObject(ChosenAvatar.BoyPrefabName);
        var list = new List<GameObject> { girl, boy };
        try
        {
            Assert.AreEqual(girl, ChosenAvatar.PrefabFor(false, girl, boy, list, boy));
            Assert.AreEqual(boy, ChosenAvatar.PrefabFor(true, girl, boy, list, girl));
            Assert.AreEqual(boy, ChosenAvatar.PrefabFor(true, null, null, list, girl));
            Assert.AreEqual(girl, ChosenAvatar.PrefabFor(false, null, null, list, boy));
        }
        finally
        {
            Object.DestroyImmediate(girl);
            Object.DestroyImmediate(boy);
        }
    }

    [Test]
    public void MaleFor_KeepsTheGuestChoiceAndTheHostChoice()
    {
        Assert.IsTrue(ChosenAvatar.MaleFor(isLocal: false, hasChoice: true, chosenMale: true, localMale: false));
        Assert.IsFalse(ChosenAvatar.MaleFor(isLocal: false, hasChoice: true, chosenMale: false, localMale: true));
        Assert.IsTrue(ChosenAvatar.MaleFor(isLocal: true, hasChoice: false, chosenMale: false, localMale: true));
        Assert.IsFalse(ChosenAvatar.MaleFor(isLocal: true, hasChoice: false, chosenMale: true, localMale: false));
    }

    [Test]
    public void WaitsForChoice_OnlyForARemotePlayer()
    {
        Assert.IsTrue(ChosenAvatar.WaitsForChoice(isLocal: false, hasChoice: false));
        Assert.IsFalse(ChosenAvatar.WaitsForChoice(isLocal: false, hasChoice: true));
        Assert.IsFalse(ChosenAvatar.WaitsForChoice(isLocal: true, hasChoice: false));
    }
}
