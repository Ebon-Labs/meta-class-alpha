using NUnit.Framework;
using UnityEngine;

public class InfoAreaTests
{
    [Test]
    public void PointInsideTheSphere_IsContained()
    {
        Assert.IsTrue(InfoArea.ContainsLocalPoint(new Vector3(0.5f, 0f, 0f), Vector3.zero, 1f));
    }

    [Test]
    public void PointOutsideTheSphere_IsExcluded()
    {
        Assert.IsFalse(InfoArea.ContainsLocalPoint(new Vector3(2f, 0f, 0f), Vector3.zero, 1f));
    }

    [Test]
    public void ZeroRadius_StillHasAMinimumVolume()
    {
        Assert.IsTrue(InfoArea.ContainsLocalPoint(Vector3.zero, Vector3.zero, 0f));
        Assert.IsFalse(InfoArea.ContainsLocalPoint(new Vector3(0.01f, 0f, 0f), Vector3.zero, 0f));
    }

    [Test]
    public void PlanetOf_FindsTheTaggedSiblingOrParent()
    {
        var wrapper = new GameObject("EarthWrapper");
        var planet = new GameObject("Earth");
        var info = new GameObject("InfoArea");
        var sun = new GameObject("Sun");
        var sunInfo = new GameObject("InfoArea");
        try
        {
            planet.tag = GazeFocusRules.PlanetTag;
            sun.tag = GazeFocusRules.PlanetTag;
            planet.transform.SetParent(wrapper.transform, false);
            info.transform.SetParent(wrapper.transform, false);
            sunInfo.transform.SetParent(sun.transform, false);

            Assert.AreSame(planet.transform, InfoArea.PlanetOf(info.transform));
            Assert.AreSame(sun.transform, InfoArea.PlanetOf(sunInfo.transform));
        }
        finally
        {
            Object.DestroyImmediate(wrapper);
            Object.DestroyImmediate(sun);
        }
    }

    [Test]
    public void InsideTheArea_TheRayIgnoresThatPlanet()
    {
        var wrapper = new GameObject("MarsWrapper");
        var planet = new GameObject("Mars");
        var surface = new GameObject("surface");
        var areaObject = new GameObject("InfoArea");
        var other = new GameObject("Venus");
        try
        {
            planet.tag = GazeFocusRules.PlanetTag;
            planet.transform.SetParent(wrapper.transform, false);
            surface.transform.SetParent(planet.transform, false);
            areaObject.transform.SetParent(wrapper.transform, false);
            areaObject.AddComponent<InfoArea>();

            Assert.IsFalse(InfoArea.AllowsRay(planet.transform, areaObject.transform.position));
            Assert.IsFalse(InfoArea.AllowsRay(surface.transform, areaObject.transform.position));
            Assert.IsTrue(InfoArea.AllowsRay(other.transform, areaObject.transform.position));
            Assert.IsTrue(InfoArea.AllowsRay(planet.transform, areaObject.transform.position + new Vector3(10f, 0f, 0f)));
        }
        finally
        {
            Object.DestroyImmediate(wrapper);
            Object.DestroyImmediate(other);
        }
    }
}
