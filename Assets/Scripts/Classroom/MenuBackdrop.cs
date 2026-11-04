using UnityEngine;

[DisallowMultipleComponent]
public class MenuBackdrop : MonoBehaviour
{
    const float ImageAspect = 16f / 9f;

    [SerializeField] Camera view;

    void LateUpdate()
    {
        if (view == null)
            view = Camera.main;

        if (view == null || !view.orthographic)
            return;

        float height = view.orthographicSize * 2f;
        float width = height * view.aspect;
        if (width / height > ImageAspect)
            transform.localScale = new Vector3(width, width / ImageAspect, 1f);
        else
            transform.localScale = new Vector3(height * ImageAspect, height, 1f);
    }
}
