using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Case01SceneSetup : EditorWindow
{
    [MenuItem("Vertigo Case/Setup Case 01 Scene Hierarchy")]
    public static void SetupScene()
    {
        // 1. Setup Main Camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        mainCam.orthographic = true;
        mainCam.orthographicSize = 5f;
        mainCam.farClipPlane = 1000f;
        mainCam.nearClipPlane = 0.3f;

        // 2. Setup Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = mainCam;
        canvas.planeDistance = 10f;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 3. Setup EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        // 4. Create Root "Case_01" under Canvas
        Transform existingCase01 = canvas.transform.Find("Case_01");
        GameObject case01Obj;
        if (existingCase01 != null)
        {
            case01Obj = existingCase01.gameObject;
        }
        else
        {
            case01Obj = new GameObject("Case_01", typeof(RectTransform));
            case01Obj.transform.SetParent(canvas.transform, false);
        }

        RectTransform case01Rect = case01Obj.GetComponent<RectTransform>();
        case01Rect.anchorMin = Vector2.zero;
        case01Rect.anchorMax = Vector2.one;
        case01Rect.offsetMin = Vector2.zero;
        case01Rect.offsetMax = Vector2.one;

        // List of layers from Back to Front
        string[] layerNames = new string[]
        {
            "ui_splash_bg",            // En arkada (Backmost)
            "ui_splash_bg_fx",
            "ui_splash_ship_bg1",
            "ui_splash_ship_bg2",
            "ui_splash_hologram_ship",
            "ui_splash_hologram_fx",
            "ui_splash_char_fx",
            "ui_splash_character"     // En önde (Frontmost)
        };

        foreach (string layerName in layerNames)
        {
            Transform existingLayer = case01Rect.Find(layerName);
            GameObject layerObj;
            if (existingLayer != null)
            {
                layerObj = existingLayer.gameObject;
            }
            else
            {
                layerObj = new GameObject(layerName, typeof(RectTransform), typeof(Image));
                layerObj.transform.SetParent(case01Rect, false);
            }

            Image img = layerObj.GetComponent<Image>();
            string assetPath = $"Assets/Case_01/{layerName}.png";
            Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (spriteAsset != null)
            {
                img.sprite = spriteAsset;
                img.SetNativeSize();
            }
            else
            {
                Debug.LogWarning($"[Case01SceneSetup] Sprite not found at: {assetPath}");
            }
        }

        // 5. Add a placeholder Particle System container for 2D VFX between BG and Character
        Transform existingVFX = case01Rect.Find("VFX_Particle_Container");
        if (existingVFX == null)
        {
            GameObject vfxObj = new GameObject("VFX_Particle_Container", typeof(RectTransform));
            vfxObj.transform.SetParent(case01Rect, false);
            // Place it right before ui_splash_character so particles render behind character but in front of BG
            Transform charTransform = case01Rect.Find("ui_splash_character");
            if (charTransform != null)
            {
                vfxObj.transform.SetSiblingIndex(charTransform.GetSiblingIndex());
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✅ [Case01SceneSetup] Case_01 UI & VFX Canvas hiyerarşisi başarıyla oluşturuldu!");
    }
}
