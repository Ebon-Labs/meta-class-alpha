using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class MenuReturnTests
{
    [Test]
    public void Advance_AddsOnlyPositiveTime()
    {
        Assert.AreEqual(1.5f, MenuReturn.Advance(1f, 0.5f), 0.0001f);
        Assert.AreEqual(1f, MenuReturn.Advance(1f, 0f), 0.0001f);
    }

    [Test]
    public void IsDue_WaitsUntilThreeSeconds()
    {
        Assert.IsFalse(MenuReturn.IsDue(2.9f, MenuReturn.DelaySeconds));
        Assert.IsTrue(MenuReturn.IsDue(MenuReturn.DelaySeconds, MenuReturn.DelaySeconds));
    }

    [Test]
    public void Choose_StopsTheActiveSession()
    {
        Assert.AreEqual(MenuReturn.LeaveKind.Host, MenuReturn.Choose(true, true, true));
        Assert.AreEqual(MenuReturn.LeaveKind.Client, MenuReturn.Choose(true, false, true));
        Assert.AreEqual(MenuReturn.LeaveKind.Server, MenuReturn.Choose(true, true, false));
        Assert.AreEqual(MenuReturn.LeaveKind.LoadMenu, MenuReturn.Choose(false, false, false));
    }

    [Test]
    public void Begin_StartsOnlyThePortalThatReturnsToTheMenu()
    {
        GameObject portal = new GameObject("PortalThree");
        GameObject other = new GameObject("PortalOne");
        portal.AddComponent<MenuReturn>();
        try
        {
            MenuReturn.Begin(other);
            MenuReturn.Begin(portal);

            Assert.IsTrue(portal.GetComponent<MenuReturn>().IsRunning);
            Assert.IsNull(other.GetComponent<MenuReturn>());
        }
        finally
        {
            Object.DestroyImmediate(portal);
            Object.DestroyImmediate(other);
        }
    }
}
