using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class TeleportRevealTests
{
    [Test]
    public void Apply_HidesTheCurrentPortalAndShowsTheNext()
    {
        GameObject current = new GameObject("PortalOne");
        GameObject next = new GameObject("PortalTwo");
        next.SetActive(false);
        try
        {
            TeleportReveal.Apply(current, next);

            Assert.IsFalse(current.activeSelf);
            Assert.IsTrue(next.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(current);
            Object.DestroyImmediate(next);
        }
    }

    [Test]
    public void Apply_HidesTheCurrentPortalWhenThereIsNoNext()
    {
        GameObject current = new GameObject("PortalThree");
        try
        {
            TeleportReveal.Apply(current, null);

            Assert.IsFalse(current.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(current);
        }
    }

    [Test]
    public void Complete_RevealsTheAssignedPortal()
    {
        GameObject current = new GameObject("PortalTwo");
        GameObject next = new GameObject("PortalThree");
        next.SetActive(false);
        TeleportReveal reveal = current.AddComponent<TeleportReveal>();
        reveal.Next = next;
        try
        {
            TeleportReveal.Complete(current);

            Assert.IsFalse(current.activeSelf);
            Assert.IsTrue(next.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(current);
            Object.DestroyImmediate(next);
        }
    }

    [Test]
    public void Complete_LeavesAnUnassignedPortalVisible()
    {
        GameObject current = new GameObject("Chair");
        try
        {
            TeleportReveal.Complete(current);

            Assert.IsTrue(current.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(current);
        }
    }
}
