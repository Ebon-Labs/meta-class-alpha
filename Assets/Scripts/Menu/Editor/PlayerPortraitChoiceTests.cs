using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerPortraitChoiceTests
{
    [Test]
    public void Chosen_UsesPlayerUntilTheMalePortraitIsSelected()
    {
        var girl = new GameObject("girl");
        var boy = new GameObject("boy");
        try
        {
            Assert.AreEqual(girl, PlayerPortraitChoice.Chosen(false, girl, boy));
            Assert.AreEqual(boy, PlayerPortraitChoice.Chosen(true, girl, boy));
        }
        finally
        {
            Object.DestroyImmediate(girl);
            Object.DestroyImmediate(boy);
        }
    }

    [Test]
    public void PortraitMaterial_ColorsTheSelectionAndGraysTheOther()
    {
        var front = new Material(Shader.Find("Sprites/Default"));
        var gray = new Material(Shader.Find("Sprites/Default"));
        try
        {
            Assert.AreEqual(front, PlayerPortraitChoice.PortraitMaterial(true, front, gray));
            Assert.AreEqual(gray, PlayerPortraitChoice.PortraitMaterial(false, front, gray));
        }
        finally
        {
            Object.DestroyImmediate(front);
            Object.DestroyImmediate(gray);
        }
    }

    [Test]
    public void KeepSpare_RemembersThePrefabThatIsNotSpawned()
    {
        var spare = new GameObject("spare");
        var list = new List<GameObject>();
        try
        {
            PlayerPortraitChoice.KeepSpare(list, spare);
            PlayerPortraitChoice.KeepSpare(list, spare);
            PlayerPortraitChoice.KeepSpare(list, null);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(spare, list[0]);
        }
        finally
        {
            Object.DestroyImmediate(spare);
        }
    }

    [Test]
    public void Press_SelectsThePortraitUnderThePointer()
    {
        var viewObject = new GameObject("PortraitView");
        var girl = new GameObject(PlayerPortraitChoice.GirlName);
        var boy = new GameObject(PlayerPortraitChoice.BoyName);
        try
        {
            Camera view = viewObject.AddComponent<Camera>();
            view.orthographic = true;
            view.orthographicSize = 5f;
            view.pixelRect = new Rect(0f, 0f, 1000f, 1000f);
            view.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);

            var girlRect = girl.AddComponent<RectTransform>();
            girlRect.sizeDelta = new Vector2(1f, 1f);
            girl.transform.SetPositionAndRotation(new Vector3(-1.5f, 0f, 0f), Quaternion.identity);

            var boyRect = boy.AddComponent<RectTransform>();
            boyRect.sizeDelta = new Vector2(1f, 1f);
            boy.transform.SetPositionAndRotation(new Vector3(1.5f, 0f, 0f), Quaternion.identity);

            PlayerPortraitChoice.Select(false);
            Assert.IsTrue(PlayerPortraitChoice.Press(view.WorldToScreenPoint(boy.transform.position), view));
            Assert.IsTrue(PlayerPortraitChoice.UsesMale);

            Assert.IsTrue(PlayerPortraitChoice.Press(view.WorldToScreenPoint(girl.transform.position), view));
            Assert.IsFalse(PlayerPortraitChoice.UsesMale);
        }
        finally
        {
            PlayerPortraitChoice.Select(false);
            Object.DestroyImmediate(viewObject);
            Object.DestroyImmediate(girl);
            Object.DestroyImmediate(boy);
        }
    }
}
