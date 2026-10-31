using Mirror;
using UnityEngine;

public class CameraEditorController : NetworkBehaviour
{
    public float sensitivity = 5f;

    float mouseX;
    float mouseY;

    void Start()
    {
        if (!Application.isEditor)
            return;

        print("We are running this from inside of the editor!");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        EditorMouseLook.Accumulate(ref mouseX, ref mouseY, Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), sensitivity);
        transform.eulerAngles = EditorMouseLook.Euler(mouseX, mouseY);
    }
}
