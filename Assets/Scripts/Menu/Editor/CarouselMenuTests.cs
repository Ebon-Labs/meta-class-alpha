using NUnit.Framework;
using UnityEngine;

public class CarouselMenuTests
{
    [Test]
    public void Clockwise_TurnsNinetyDegreesOnLocalUp()
    {
        Vector3 facing = CarouselMenu.Clockwise(Quaternion.identity) * Vector3.forward;

        Assert.AreEqual(90f, CarouselMenu.StepDegrees);
        Assert.AreEqual(1f, facing.x, 0.001f);
        Assert.AreEqual(0f, facing.y, 0.001f);
        Assert.AreEqual(0f, facing.z, 0.001f);
    }

    [Test]
    public void CounterClockwise_TurnsTheOtherWay()
    {
        Vector3 facing = CarouselMenu.CounterClockwise(Quaternion.identity) * Vector3.forward;

        Assert.AreEqual(-1f, facing.x, 0.001f);
        Assert.AreEqual(0f, facing.y, 0.001f);
        Assert.AreEqual(0f, facing.z, 0.001f);
    }

    [Test]
    public void Turn_ReachesTheStepInOneAndAHalfSeconds()
    {
        Quaternion from = Quaternion.identity;
        Quaternion to = CarouselMenu.Clockwise(from);

        Assert.AreEqual(1.5f, CarouselMenu.TurnDuration);
        Assert.AreEqual(0f, Quaternion.Angle(CarouselMenu.Turn(from, to, 0f, CarouselMenu.TurnDuration), from), 0.001f);

        Vector3 facing = CarouselMenu.Turn(from, to, CarouselMenu.TurnDuration, CarouselMenu.TurnDuration) * Vector3.forward;
        Assert.AreEqual(1f, facing.x, 0.001f);
        Assert.AreEqual(0f, facing.z, 0.001f);
    }

    [Test]
    public void SignedStep_CreateGoesClockwiseAndCreateBackGoesBack()
    {
        Assert.AreEqual(CarouselMenu.StepDegrees, CarouselMenu.SignedStep(CarouselMenu.CreateName));
        Assert.AreEqual(CarouselMenu.StepDegrees, CarouselMenu.SignedStep(ImageMenuThumb.ObjectName));
        Assert.AreEqual(-CarouselMenu.StepDegrees, CarouselMenu.SignedStep(CarouselMenu.CreateBackName));
        Assert.AreEqual(-CarouselMenu.StepDegrees, CarouselMenu.SignedStep(CarouselMenu.EnterName));
        Assert.AreEqual(CarouselMenu.SignedStep(CarouselMenu.CreateName), CarouselMenu.SignedStep(CarouselMenu.EnterBackName));
        Assert.AreEqual(0f, CarouselMenu.SignedStep("Title"));
        Assert.AreEqual(0f, CarouselMenu.SignedStep(CreateRoomButton.ObjectName));
        Assert.AreEqual(0f, CarouselMenu.SignedStep(CarouselMenu.TutorialName));
    }

    [Test]
    public void TutorialButton_WinsTheTouchAndDoesNotTurn()
    {
        var viewObject = new GameObject("CarouselView");
        var enter = new GameObject(CarouselMenu.EnterName);
        var tutorial = new GameObject(CarouselMenu.TutorialName);
        try
        {
            Camera view = viewObject.AddComponent<Camera>();
            view.orthographic = true;
            view.orthographicSize = 5f;
            view.pixelRect = new Rect(0f, 0f, 1000f, 1000f);
            view.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);

            var enterRect = enter.AddComponent<RectTransform>();
            enterRect.sizeDelta = new Vector2(0.4f, 0.2f);
            enter.transform.SetPositionAndRotation(new Vector3(0f, 0.15f, 0f), Quaternion.identity);

            var tutorialRect = tutorial.AddComponent<RectTransform>();
            tutorialRect.sizeDelta = new Vector2(0.4f, 0.2f);
            tutorial.transform.SetPositionAndRotation(new Vector3(0f, -0.15f, 0f), Quaternion.identity);

            Vector2 onTutorial = view.WorldToScreenPoint(tutorial.transform.position);
            Assert.AreEqual(0f, CarouselMenu.SignedStep(onTutorial, view, enter, tutorial));

            Vector2 onEnter = view.WorldToScreenPoint(enter.transform.position);
            Assert.AreEqual(-CarouselMenu.StepDegrees, CarouselMenu.SignedStep(onEnter, view, enter, tutorial));
        }
        finally
        {
            Object.DestroyImmediate(tutorial);
            Object.DestroyImmediate(enter);
            Object.DestroyImmediate(viewObject);
        }
    }

    [Test]
    public void BackButtons_KeepTheirDirectionFromAnyYaw()
    {
        Quaternion tilted = Quaternion.Euler(0f, 40f, 0f);

        Quaternion enterBack = tilted * Quaternion.AngleAxis(CarouselMenu.SignedStep(CarouselMenu.EnterBackName), Vector3.up);
        Quaternion createBack = tilted * Quaternion.AngleAxis(CarouselMenu.SignedStep(CarouselMenu.CreateBackName), Vector3.up);

        Quaternion create = tilted * Quaternion.AngleAxis(CarouselMenu.SignedStep(CarouselMenu.CreateName), Vector3.up);
        Assert.AreEqual(0f, Quaternion.Angle(enterBack, create), 0.001f);
        Assert.AreEqual(0f, Quaternion.Angle(createBack, CarouselMenu.Clockwise(tilted)), 0.001f);
    }

    [Test]
    public void SignedStep_IgnoresALabelTurnedAwayFromTheCamera()
    {
        var viewObject = new GameObject("CarouselView");
        var label = new GameObject(CarouselMenu.EnterBackName);
        try
        {
            Camera view = viewObject.AddComponent<Camera>();
            view.orthographic = true;
            view.orthographicSize = 5f;
            view.pixelRect = new Rect(0f, 0f, 200f, 100f);
            view.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);

            var rect = label.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40f, 10f);
            label.transform.SetPositionAndRotation(new Vector3(0f, 0f, 5f), Quaternion.Euler(0f, 90f, 0f));

            Vector2 screen = view.WorldToScreenPoint(label.transform.position);
            Assert.AreEqual(0f, CarouselMenu.SignedStep(screen, view, label));

            label.transform.rotation = Quaternion.identity;
            screen = view.WorldToScreenPoint(label.transform.position);
            Assert.AreEqual(CarouselMenu.SignedStep(CarouselMenu.CreateName), CarouselMenu.SignedStep(screen, view, label));
        }
        finally
        {
            Object.DestroyImmediate(label);
            Object.DestroyImmediate(viewObject);
        }
    }

    [Test]
    public void ScreenTouch_HitsTheLabelUnderTheFinger()
    {
        Vector3 a = new Vector3(10f, 20f, 5f);
        Vector3 b = new Vector3(40f, 20f, 5f);
        Vector3 c = new Vector3(10f, 50f, 5f);
        Vector3 d = new Vector3(40f, 50f, 5f);

        Assert.IsTrue(CarouselMenu.ContainsScreenPoint(new Vector2(25f, 35f), a, b, c, d, out _));
        Assert.IsFalse(CarouselMenu.ContainsScreenPoint(new Vector2(5f, 35f), a, b, c, d, out _));
    }

    [Test]
    public void ScreenTouch_IgnoresALabelBehindTheCamera()
    {
        Vector3 a = new Vector3(10f, 20f, -1f);
        Vector3 b = new Vector3(40f, 20f, -1f);
        Vector3 c = new Vector3(10f, 50f, -1f);
        Vector3 d = new Vector3(40f, 50f, -1f);

        Assert.IsFalse(CarouselMenu.ContainsScreenPoint(new Vector2(25f, 35f), a, b, c, d, out _));
    }
}
