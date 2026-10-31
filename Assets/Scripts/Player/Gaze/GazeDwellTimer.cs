public static class GazeDwellTimer
{
    public static float Advance(float counter, float deltaTime)
    {
        return counter + deltaTime;
    }

    public static bool IsComplete(float counter, float clickTime)
    {
        return counter >= clickTime;
    }

    public static float SliderFraction(float counter, float clickTime)
    {
        if (clickTime <= 0f)
            return 0f;

        return counter / clickTime;
    }

    public static bool ShowLoader(bool pointerEntered, bool hasBasicController)
    {
        return pointerEntered && hasBasicController;
    }
}
