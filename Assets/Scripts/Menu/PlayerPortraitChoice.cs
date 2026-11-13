using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerPortraitChoice : MonoBehaviour
{
    public const string GirlName = "Player1";
    public const string BoyName = "Player2";

    public GameObject girlPrefab;
    public GameObject boyPrefab;
    public Material grayscale;

    static GameObject girl;
    static GameObject boy;
    static Material gray;

    public static bool UsesMale { get; private set; }

    public static GameObject Chosen(bool male, GameObject girlPrefab, GameObject boyPrefab)
    {
        return male ? boyPrefab : girlPrefab;
    }

    public static Material PortraitMaterial(bool colored, Material grayscale)
    {
        return colored ? null : grayscale;
    }

    public static void KeepSpare(List<GameObject> prefabs, GameObject spare)
    {
        if (prefabs == null || spare == null || prefabs.Contains(spare))
            return;

        prefabs.Add(spare);
    }

    public static void Select(bool male)
    {
        UsesMale = male;
        Show();
    }

    public static void Show()
    {
        Paint(GameObject.Find(GirlName), !UsesMale);
        Paint(GameObject.Find(BoyName), UsesMale);
    }

    public static void Paint(GameObject portrait, bool colored)
    {
        if (portrait == null)
            return;

        Image image = portrait.GetComponent<Image>();
        if (image == null)
            return;

        image.material = PortraitMaterial(colored, Gray());
        image.color = Color.white;
    }

    public static bool Press(Vector2 screen, Camera view)
    {
        if (SolarSystemThumbHost.Hits(GameObject.Find(BoyName), screen, view))
        {
            Select(true);
            return true;
        }

        if (SolarSystemThumbHost.Hits(GameObject.Find(GirlName), screen, view))
        {
            Select(false);
            return true;
        }

        return false;
    }

    public static void Apply(NetworkManager manager)
    {
        if (manager == null)
            return;

        GameObject currentGirl = girl != null ? girl : manager.playerPrefab;
        GameObject chosen = Chosen(UsesMale, currentGirl, boy);
        GameObject spare = Chosen(!UsesMale, currentGirl, boy);
        if (chosen != null)
            manager.playerPrefab = chosen;

        KeepSpare(manager.spawnPrefabs, spare);
    }

    void Awake()
    {
        if (girlPrefab != null)
            girl = girlPrefab;

        if (boyPrefab != null)
            boy = boyPrefab;

        if (grayscale != null)
            gray = grayscale;
    }

    void Start()
    {
        Select(false);
    }

    static Material Gray()
    {
        if (gray != null)
            return gray;

        Shader shader = Shader.Find("Custom/MenuGrayscale");
        if (shader == null)
            return null;

        gray = new Material(shader);
        return gray;
    }
}
