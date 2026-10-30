using UnityEngine;

public class ScreenControlButton : MonoBehaviour
{
    public enum Action
    {
        Play,
        Pause,
        Stop
    }

    [SerializeField] Action action;

    public void OnPointerClick()
    {
        MainScreen screen = GetComponentInParent<MainScreen>();
        if (screen == null)
            return;

        switch (action)
        {
            case Action.Play:
                screen.Play();
                break;
            case Action.Pause:
                screen.Pause();
                break;
            case Action.Stop:
                screen.Stop();
                break;
        }
    }
}
