using UnityEngine;

public static class TutorialHost
{
    public const string ScenePath = "Assets/Scenes/Tutorial.unity";

    public static bool Press(Vector2 screen, Camera view)
    {
        if (!SolarSystemThumbHost.Hits(GameObject.Find(CarouselMenu.TutorialName), screen, view))
            return false;

        SolarSystemThumbHost.StartHostScene(ScenePath);
        return true;
    }
}
