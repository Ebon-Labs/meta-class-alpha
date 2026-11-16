using NUnit.Framework;
using UnityEngine;

public class EbonLabsCreditTests
{
    [Test]
    public void Url_OpensTheEbonLabsSite()
    {
        Assert.AreEqual("https://ebonlabs.pages.dev/", EbonLabsCredit.Url);
        Assert.AreEqual("EbonLabsCredit", EbonLabsCredit.ObjectName);
    }

    [Test]
    public void Press_WithoutTheMark_DoesNotTakeTheTouch()
    {
        Assert.IsFalse(EbonLabsCredit.Press(Vector2.zero, null));
    }

    [Test]
    public void Mark_AcceptsATouchOnTheIconAndRejectsOneBesideIt()
    {
        var viewObject = new GameObject("CreditView");
        var mark = new GameObject(EbonLabsCredit.ObjectName);
        try
        {
            Camera view = viewObject.AddComponent<Camera>();
            view.orthographic = true;
            view.orthographicSize = 5f;
            view.pixelRect = new Rect(0f, 0f, 1000f, 1000f);
            view.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);

            var rect = mark.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.68f, 0.44f);
            mark.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            Vector2 onMark = view.WorldToScreenPoint(mark.transform.position);
            Assert.IsTrue(ImageMenuThumb.Hits(mark, onMark, view));
            Assert.IsFalse(ImageMenuThumb.Hits(mark, onMark + new Vector2(400f, 0f), view));
        }
        finally
        {
            Object.DestroyImmediate(mark);
            Object.DestroyImmediate(viewObject);
        }
    }
}
