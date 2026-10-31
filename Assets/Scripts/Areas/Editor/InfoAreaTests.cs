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
}
