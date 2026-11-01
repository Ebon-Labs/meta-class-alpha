using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class CameraPointerControllerTests
{
    [Test]
    public void HeldGaze_ReachesTheDefaultClickTime()
    {
        float counter = 0f;
        counter = GazeDwellTimer.Advance(counter, 1f);
        Assert.IsFalse(GazeDwellTimer.IsComplete(counter, 3f));

        counter = GazeDwellTimer.Advance(counter, 2f);
        Assert.IsTrue(GazeDwellTimer.IsComplete(counter, 3f));
        Assert.AreEqual(1f, GazeDwellTimer.SliderFraction(counter, 3f), 0.0001f);
    }

    [Test]
    public void EnvironmentAndOccupiedEdge_DoNotStartFocus()
    {
        Assert.IsTrue(GazeFocusRules.BlocksNewFocus(isEnvironment: true, pointerDisabled: false, teleportableAtEdge: false));
        Assert.IsTrue(GazeFocusRules.BlocksNewFocus(isEnvironment: false, pointerDisabled: false, teleportableAtEdge: true));
    }

    [Test]
    public void Teleport_PlacesTheObserverJustOutsideTheHitVolume()
    {
        Bounds bounds = new Bounds(new Vector3(0f, 1f, 0f), new Vector3(2f, 2f, 2f));
        Vector3 edge = TeleportPlacement.EdgePosition(bounds, new Vector3(0f, 1f, 8f), Vector3.forward);

        Assert.AreEqual(0f, edge.x, 0.0001f);
        Assert.AreEqual(1f, edge.y, 0.0001f);
        Assert.AreEqual(1f + TeleportPlacement.EdgeMargin, edge.z, 0.0001f);
    }

    [Test]
    public void GoHomeOrb_IncludesTheOrbAndItsChildren()
    {
        GameObject orb = new GameObject("GoHomeOrb");
        GameObject child = new GameObject("label");
        GameObject other = new GameObject("Sun");
        try
        {
            child.transform.SetParent(orb.transform, false);

            Assert.IsTrue(GoHomeOrb.IsTarget(orb.transform));
            Assert.IsTrue(GoHomeOrb.IsTarget(child.transform));
            Assert.IsFalse(GoHomeOrb.IsTarget(other.transform));
            Assert.IsFalse(GoHomeOrb.IsTarget(null));
        }
        finally
        {
            Object.DestroyImmediate(orb);
            Object.DestroyImmediate(other);
        }
    }
}
