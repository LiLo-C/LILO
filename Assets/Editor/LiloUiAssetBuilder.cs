using System.IO;
using Lilo.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Memasang aset UI baru (Assets/Sprites/UIAssets) ke prefab dan ke scene MainMenu.
/// Dijalankan sekali dari menu LILO/UI. Setelah itu semua tata letak diatur lewat Inspector.
///
/// Ukuran mengikuti Reference Resolution 2796 × 1290. Sprite yang ada diekspor kecil,
/// jadi ditampilkan sekitar 3× ukuran aslinya (lihat UiScale).
/// </summary>
public static class LiloUiAssetBuilder
{
    private const string SpriteRoot = "Assets/Sprites/UIAssets/";
    private const string PrefabRoot = "Assets/Prefabs/UI/";
    private const string SfxTogglePath = PrefabRoot + "SfxToggle.prefab";
    private const string GameplayHudPath = PrefabRoot + "GameplayHud.prefab";
    private const string FontPath = "Assets/Fonts/American Typewriter Bold SDF.asset";
    private const float UiScale = 3f;

    private static readonly Color Ink = Hex("#39444A");
    private static readonly Color Paper = Hex("#DFE3DA");
    private static readonly Color Dim = new Color(0.04f, 0.063f, 0.094f, 0.72f); // #0A1018

    [MenuItem("LILO/UI/1. Fix UI Sprite Import Settings")]
    public static void FixImportSettings()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteRoot.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
        Debug.Log("[LiloUi] Import settings UIAssets: Sprite, Full Rect, Bilinear, None, 2048.");
    }

    [MenuItem("LILO/UI/2. Build SFX Toggle + Gameplay HUD Prefabs")]
    public static void BuildPrefabs()
    {
        Directory.CreateDirectory(PrefabRoot);
        BuildSfxTogglePrefab();
        BuildGameplayHudPrefab();
        AssetDatabase.SaveAssets();
        Debug.Log($"[LiloUi] Prefab dibuat: {SfxTogglePath}, {GameplayHudPath}. " +
                  "Pemilik scene gameplay: hapus GameplayHudCanvas lama, taruh GameplayHud.prefab.");
    }

    [MenuItem("LILO/UI/3. Apply New Assets To Open MainMenu Scene")]
    public static void ApplyToMainMenu()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            EditorUtility.DisplayDialog("LILO UI", "Buka scene MainMenu dulu.", "OK");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(SfxTogglePath) == null)
            BuildPrefabs();

        GameObject canvas = GameObject.Find("MainMenuCanvas");
        if (canvas == null) throw new System.InvalidOperationException("MainMenuCanvas tidak ditemukan.");

        ApplyHowToPlayPanel(canvas.transform.Find("HowToPlayPanel"));
        ApplySettingsPanel(canvas.transform.Find("SettingsPanel"), canvas.GetComponent<MainMenuController>());

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[LiloUi] MainMenu: panel How To Play dan Settings pakai aset baru. Cek lalu simpan scene.");
    }

    // ---------- Prefab ----------

    private static GameObject BuildSfxTogglePrefab()
    {
        var root = NewUi("SfxToggle", null);
        Size(root, new Vector2(300f, 140f)); // area sentuh ≥ 130 px
        var hitArea = root.AddComponent<Image>();
        hitArea.color = Color.clear; // tak terlihat, hanya menangkap sentuhan

        Sprite on = LoadSprite("SliderTrackEnable");
        Sprite off = LoadSprite("SliderTrackDisable");
        Image track = NewImage("Track", root.transform, on);
        Size(track.gameObject, NativeSize(on));
        Image knob = NewImage("Knob", root.transform, LoadSprite("SliderKnob"));
        Size(knob.gameObject, NativeSize(knob.sprite));
        knob.raycastTarget = false;

        var toggle = root.AddComponent<Toggle>();
        toggle.targetGraphic = track;
        toggle.graphic = null;
        toggle.isOn = true;
        toggle.navigation = new Navigation { mode = Navigation.Mode.None };

        float travel = (track.rectTransform.sizeDelta.x - knob.rectTransform.sizeDelta.x) * 0.5f - 4f * UiScale;
        var visual = root.AddComponent<SwitchToggle>();
        var so = new SerializedObject(visual);
        so.FindProperty("track").objectReferenceValue = track;
        so.FindProperty("trackOn").objectReferenceValue = on;
        so.FindProperty("trackOff").objectReferenceValue = off;
        so.FindProperty("knob").objectReferenceValue = knob.rectTransform;
        so.FindProperty("knobTravel").floatValue = travel;
        so.ApplyModifiedPropertiesWithoutUndo();
        knob.rectTransform.anchoredPosition = new Vector2(travel, 0f);

        return SavePrefab(root, SfxTogglePath);
    }

    private static void BuildGameplayHudPrefab()
    {
        var root = NewUi("GameplayHudCanvas", null);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110; // di atas kontrol mobile (100) supaya pause menutup joystick
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2796f, 1290f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        root.AddComponent<GraphicRaycaster>();
        var hud = root.AddComponent<GameplayHudController>();

        // Bendera lantai, pojok kiri atas. Angka lantai di bawah tulisan "Floor".
        Sprite flagSprite = LoadSprite("FloorFlag");
        Image flag = NewImage("FloorFlag", root.transform, flagSprite);
        flag.raycastTarget = false;
        Place(flag.rectTransform, new Vector2(0f, 1f), new Vector2(120f, -60f), NativeSize(flagSprite, 2f));
        TMP_Text floor = NewText("FloorNumber", flag.transform, "52", 88, Ink, TextAlignmentOptions.Center);
        Stretch(floor.rectTransform, new Vector2(0.05f, 0.12f), new Vector2(0.85f, 0.62f));

        TMP_Text battery = NewText("BatteryText", root.transform, "BATTERY 100%", 48, Paper, TextAlignmentOptions.Left);
        Place(battery.rectTransform, new Vector2(0f, 1f), new Vector2(340f, -80f), new Vector2(640f, 100f));

        // Tombol pause, pojok kanan atas.
        Button pause = NewButton("PauseButton", root.transform, LoadSprite("PauseButton"), 2f);
        Place((RectTransform)pause.transform, new Vector2(1f, 1f), new Vector2(-120f, -60f), null);

        // Panel pause: latar gelap penuh layar + bingkai.
        var panel = NewImage("PausePanel", root.transform, null);
        panel.color = Dim;
        Stretch(panel.rectTransform, Vector2.zero, Vector2.one);
        Sprite frameSprite = LoadSprite("PausedFrame");
        Image frame = NewImage("PausedFrame", panel.transform, frameSprite);
        Size(frame.gameObject, NativeSize(frameSprite));
        frame.raycastTarget = false;

        // Titik tengah area krem bingkai ada 22 px (asli) di bawah tengah sprite.
        float inner = -22f * UiScale;
        Button resume = NewButton("ResumeButton", frame.transform, LoadSprite("ResumeButton"));
        Button restart = NewButton("RestartButton", frame.transform, LoadSprite("RestartButton"));
        float half = ((RectTransform)resume.transform).sizeDelta.x * 0.5f + 20f; // jarak antar tombol 40
        ((RectTransform)resume.transform).anchoredPosition = new Vector2(-half, inner + 216f);
        ((RectTransform)restart.transform).anchoredPosition = new Vector2(half, inner + 216f);

        var soundRow = NewUi("SoundRow", frame.transform);
        Size(soundRow, new Vector2(760f, 140f));
        ((RectTransform)soundRow.transform).anchoredPosition = new Vector2(0f, inner + 20f);
        var sound = soundRow.AddComponent<SoundSettingsPanel>();
        TMP_Text soundLabel = NewText("SoundLabel", soundRow.transform, "SOUND EFFECTS", 48, Ink, TextAlignmentOptions.Right);
        Place(soundLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-150f, 0f), new Vector2(460f, 100f));
        var toggle = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SfxTogglePath), soundRow.transform);
        ((RectTransform)toggle.transform).anchoredPosition = new Vector2(230f, 0f);
        SetRef(sound, "sfxToggle", toggle.GetComponent<Toggle>());

        Button exit = NewButton("ExitButton", frame.transform, LoadSprite("ExitButton"));
        ((RectTransform)exit.transform).anchoredPosition = new Vector2(0f, inner - 184f);

        var so = new SerializedObject(hud);
        so.FindProperty("floorText").objectReferenceValue = floor;
        so.FindProperty("batteryText").objectReferenceValue = battery;
        so.FindProperty("pauseButton").objectReferenceValue = pause;
        so.FindProperty("pausePanel").objectReferenceValue = panel.gameObject;
        so.FindProperty("resumeButton").objectReferenceValue = resume;
        so.FindProperty("restartButton").objectReferenceValue = restart;
        so.FindProperty("mainMenuButton").objectReferenceValue = exit;
        so.FindProperty("soundSettings").objectReferenceValue = sound;
        so.ApplyModifiedPropertiesWithoutUndo();

        panel.gameObject.SetActive(false);
        SavePrefab(root, GameplayHudPath);
    }

    // ---------- MainMenu ----------

    private static void ApplyHowToPlayPanel(Transform panel)
    {
        if (panel == null) { Debug.LogWarning("[LiloUi] HowToPlayPanel tidak ditemukan."); return; }
        SetPanelDim(panel);

        // Teks instruksi sudah tercetak di sprite bingkai, jadi teks lama disembunyikan.
        panel.Find("HowToPlayText")?.gameObject.SetActive(false);

        Sprite frameSprite = LoadSprite("HowToPlayFrame");
        Image frame = FindOrCreateImage(panel, "HowToPlayFrame", frameSprite);
        frame.raycastTarget = false;
        Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), NativeSize(frameSprite));
        frame.transform.SetSiblingIndex(0);

        ApplyCloseButton(panel.Find("CloseHowToPlay"), new Vector2(0f, 90f - 447f - 40f - 84f));
    }

    private static void ApplySettingsPanel(Transform panel, MainMenuController menu)
    {
        if (panel == null) { Debug.LogWarning("[LiloUi] SettingsPanel tidak ditemukan."); return; }
        SetPanelDim(panel);

        // Slider volume lama sudah tidak dipakai (hanya ada toggle SFX).
        foreach (string old in new[] { "MASTER", "MUSIC", "EFFECTS", "AMBIENCE" })
        {
            DestroyChild(panel, old + "Label");
            DestroyChild(panel, old + "Slider");
        }

        ReplaceWithTmp(panel.Find("SettingsTitle"), "SETTINGS", 96, Paper,
            new Vector2(0f, 330f), new Vector2(1000f, 140f), TextAlignmentOptions.Center);

        Transform row = panel.Find("SoundRow");
        if (row == null)
        {
            var rowGo = NewUi("SoundRow", panel);
            row = rowGo.transform;
            TMP_Text label = NewText("SoundLabel", row, "SOUND EFFECTS", 56, Paper, TextAlignmentOptions.Right);
            Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-170f, 0f), new Vector2(520f, 110f));
            var toggle = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SfxTogglePath), row);
            ((RectTransform)toggle.transform).anchoredPosition = new Vector2(250f, 0f);
        }
        Place((RectTransform)row, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(860f, 140f));

        var sound = panel.GetComponent<SoundSettingsPanel>();
        if (sound == null) sound = panel.gameObject.AddComponent<SoundSettingsPanel>();
        SetRef(sound, "sfxToggle", row.GetComponentInChildren<Toggle>(true));
        if (menu != null) SetRef(menu, "soundSettings", sound);

        ApplyCloseButton(panel.Find("CloseSettings"), new Vector2(0f, -300f));
    }

    private static void ApplyCloseButton(Transform close, Vector2 position)
    {
        if (close == null) return;
        var image = close.GetComponent<Image>();
        image.sprite = LoadSprite("CloseButton");
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        var button = close.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        // Tulisan "Close" sudah tercetak di sprite.
        foreach (Transform child in close) child.gameObject.SetActive(false);
        Place((RectTransform)close, new Vector2(0.5f, 0.5f), position, NativeSize(image.sprite));
    }

    private static void SetPanelDim(Transform panel)
    {
        var image = panel.GetComponent<Image>();
        if (image != null) image.color = Dim;
    }

    private static void ReplaceWithTmp(Transform target, string text, float size, Color color,
        Vector2 position, Vector2 sizeDelta, TextAlignmentOptions align)
    {
        if (target == null) return;
        var legacy = target.GetComponent<Text>();
        if (legacy != null) Object.DestroyImmediate(legacy);
        var tmp = target.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = target.gameObject.AddComponent<TextMeshProUGUI>();
        StyleText(tmp, text, size, color, align);
        Place((RectTransform)target, new Vector2(0.5f, 0.5f), position, sizeDelta);
    }

    // ---------- Helper ----------

    private static GameObject NewUi(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite)
    {
        var image = NewUi(name, parent).AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = sprite != null;
        return image;
    }

    private static Image FindOrCreateImage(Transform parent, string name, Sprite sprite)
    {
        Transform existing = parent.Find(name);
        Image image = existing != null ? existing.GetComponent<Image>() : NewImage(name, parent, sprite);
        image.sprite = sprite;
        image.preserveAspect = true;
        return image;
    }

    private static Button NewButton(string name, Transform parent, Sprite sprite, float scale = UiScale)
    {
        Image image = NewImage(name, parent, sprite);
        Size(image.gameObject, NativeSize(sprite, scale));
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private static TMP_Text NewText(string name, Transform parent, string text, float size, Color color,
        TextAlignmentOptions align)
    {
        var tmp = NewUi(name, parent).AddComponent<TextMeshProUGUI>();
        StyleText(tmp, text, size, color, align);
        return tmp;
    }

    private static void StyleText(TextMeshProUGUI tmp, string text, float size, Color color, TextAlignmentOptions align)
    {
        tmp.text = text;
        tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2? size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        if (size.HasValue) rect.sizeDelta = size.Value;
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void Size(GameObject go, Vector2 size) => ((RectTransform)go.transform).sizeDelta = size;

    private static Vector2 NativeSize(Sprite sprite, float scale = UiScale) =>
        sprite == null ? new Vector2(100f, 100f) : sprite.rect.size * scale;

    private static Sprite LoadSprite(string name)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + name + ".png");
        if (sprite == null) Debug.LogWarning($"[LiloUi] Sprite {name}.png tidak ditemukan / bukan Sprite.");
        return sprite;
    }

    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void DestroyChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) Object.DestroyImmediate(child.gameObject);
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    // Untuk batchmode: Unity -batchmode -executeMethod LiloUiAssetBuilder.RunAllBatch
    public static void RunAllBatch()
    {
        FixImportSettings();
        BuildPrefabs();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        ApplyToMainMenu();
        EditorSceneManager.SaveOpenScenes();
    }
}
