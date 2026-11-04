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
        Assert.AreEqual(-CarouselMenu.StepDegrees, CarouselMenu.SignedStep(CarouselMenu.CreateBackName));
        Assert.AreEqual(-CarouselMenu.StepDegrees, CarouselMenu.SignedStep(CarouselMenu.EnterName));
        Assert.AreEqual(CarouselMenu.StepDegrees, CarouselMenu.SignedStep(CarouselMenu.EnterBackName));
        Assert.AreEqual(0f, CarouselMenu.SignedStep("Title"));
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
