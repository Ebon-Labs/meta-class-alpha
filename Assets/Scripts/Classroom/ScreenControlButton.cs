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

    public static void Execute(Action action, IScreenPlayback screen)
    {
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

    public void OnPointerClick()
    {
        Execute(action, GetComponentInParent<MainScreen>());
    }
}

public interface IScreenPlayback
{
    void Play();
    void Pause();
    void Stop();
}
