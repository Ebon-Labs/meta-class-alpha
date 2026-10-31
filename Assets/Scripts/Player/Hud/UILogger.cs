using TMPro;
using UnityEngine;

public class UILogger : MonoBehaviour
{
    public TextMeshProUGUI LogUI;

    string incomingMessage = "";

    void Update()
    {
        if (LogUI == null)
            return;

        LogUI.text = DisplayText(incomingMessage);
    }

    public static string DisplayText(string message)
    {
        return message ?? "";
    }

    public void SetMessage(string message)
    {
        incomingMessage = message;
    }
}
