using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RoomListTests
{
    [Test]
    public void Label_UsesTheRoomHostAndPort()
    {
        Assert.AreEqual("192.168.0.8:7777", RoomList.Label(new Uri("tcp://192.168.0.8:7777")));
        Assert.AreEqual("localhost", RoomList.Label(new Uri("tcp://localhost")));
        Assert.AreEqual("", RoomList.Label(null));
    }

    [Test]
    public void ItemPosition_StacksEachRoomDownward()
    {
        Assert.AreEqual(new Vector2(0f, 0f), RoomList.ItemPosition(0, 5f));
        Assert.AreEqual(new Vector2(0f, -5f), RoomList.ItemPosition(1, 5f));
        Assert.AreEqual(new Vector2(0f, -10f), RoomList.ItemPosition(2, 5f));
    }

    [Test]
    public void Nearest_PicksTheRoomUnderTheTouch()
    {
        var min = new List<Vector2> { new Vector2(0f, 0f), new Vector2(0f, 80f) };
        var max = new List<Vector2> { new Vector2(40f, 30f), new Vector2(40f, 110f) };
        var centers = new List<Vector2> { new Vector2(20f, 15f), new Vector2(20f, 95f) };

        Assert.AreEqual(1, RoomList.Nearest(new Vector2(18f, 100f), min, max, centers, 0f));
        Assert.AreEqual(-1, RoomList.Nearest(new Vector2(200f, 200f), min, max, centers, 0f));
    }
}
