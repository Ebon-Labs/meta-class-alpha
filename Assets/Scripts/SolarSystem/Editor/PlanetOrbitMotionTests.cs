using NUnit.Framework;

public class PlanetOrbitMotionTests
{
    [Test]
    public void InteractionMarker_IsPoseLocked_OtherChildrenAreNot()
    {
        Assert.IsTrue(PlanetOrbitMotion.LocksChildPose("InteractionMarkerOrb"));
        Assert.IsTrue(PlanetOrbitMotion.LocksChildPose("InteractionMarkerOrb (1)"));
        Assert.IsFalse(PlanetOrbitMotion.LocksChildPose("PlanetLabel"));
        Assert.IsFalse(PlanetOrbitMotion.LocksChildPose(null));
    }

    [Test]
    public void OrbitAndSpin_NeverRunTogether()
    {
        Assert.IsFalse(PlanetOrbitMotion.AllowsSpin(orbitEnabled: true, spinEnabled: true));
        Assert.IsTrue(PlanetOrbitMotion.AllowsSpin(orbitEnabled: false, spinEnabled: true));
        Assert.IsFalse(PlanetOrbitMotion.AllowsSpin(orbitEnabled: false, spinEnabled: false));
    }
}
