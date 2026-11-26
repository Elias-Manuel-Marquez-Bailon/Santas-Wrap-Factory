using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Menú Tools/Santa/Build Full Game.
/// 1) Crea los 3 LevelData (si no existen).
/// 2) Regenera la escena Factory (PrototypeSceneBuilder) y le agrega
///    GameManager, ScoreManager, OrderManager y toda la UI (tarjetas, HUD, pausa, pantalla final).
/// 3) Crea la escena MainMenu.
/// 4) Configura Build Settings: MainMenu (0), Factory (1).
/// </summary>
public static class GameBuilder
{
    const string LevelFolder = "Assets/_Project/ScriptableObjects/Levels";
    const string UIPrefabFolder = "Assets/_Project/Prefabs/UI";
    const string FactoryPath = "Assets/_Project/Scenes/Factory.unity";
    const string MenuPath = "Assets/_Project/Scenes/MainMenu.unity";

    static readonly Color PanelDark = new Color(0.1f, 0.08f, 0.18f, 0.85f);
    static readonly Color CardColor = UIColors.Hex("#FFF8E1");
    static readonly Color TextDark = UIColors.Hex("#3E2723");
    static readonly Color ButtonRed = UIColors.Hex("#C62828");

    [MenuItem("Tools/Santa/Build Full Game")]
    public static void BuildAll()
    {
        // TextMeshPro necesita sus recursos básicos importados
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            EditorUtility.DisplayDialog("Falta TextMeshPro",
                "Primero importa los recursos de TextMeshPro:\nWindow > TextMeshPro > Import TMP Essential Resources\n\nLuego vuelve a correr Tools/Santa/Build Full Game.",
                "OK");
            return;
        }

        LevelData[] levels = CreateLevels();

        // Escena de juego
        PrototypeSceneBuilder.Build();
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != FactoryPath) return; // el usuario canceló

        AddGameSystems(levels);
        EditorSceneManager.SaveScene(scene);

        // Menú principal
        BuildMainMenu();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuPath, true),
            new EditorBuildSettingsScene(FactoryPath, true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("[Santa] Juego completo generado. Abre MainMenu y dale Play.");
    }

    // ---------------- Niveles ----------------

    static LevelData[] CreateLevels()
    {
        return new[]
        {
            GetLevel("Level_1", l => {
                l.levelName = "Nivel 1"; l.levelDuration = 150f;
                l.orderInterval = 22f; l.orderTimeLimit = 70f; l.useBow = false;
                l.allowedToys = new[] { ToyKind.Bear, ToyKind.Car };
                l.oneStar = 40; l.twoStars = 80; l.threeStars = 120;
            }),
            GetLevel("Level_2", l => {
                l.levelName = "Nivel 2"; l.levelDuration = 120f;
                l.orderInterval = 18f; l.orderTimeLimit = 60f; l.useBow = true;
                l.allowedToys = new[] { ToyKind.Bear, ToyKind.Car, ToyKind.Robot };
                l.oneStar = 60; l.twoStars = 100; l.threeStars = 140;
            }),
            GetLevel("Level_3", l => {
                l.levelName = "Nivel 3"; l.levelDuration = 120f;
                l.orderInterval = 13f; l.orderTimeLimit = 50f; l.useBow = true;
                l.allowedToys = new[] { ToyKind.Bear, ToyKind.Car, ToyKind.Robot, ToyKind.Ball };
                l.oneStar = 80; l.twoStars = 130; l.threeStars = 180;
            }),
        };
    }

    /// <summary>Carga el nivel si existe (respeta tus cambios); si no, lo crea con valores iniciales.</summary>
    static LevelData GetLevel(string name, System.Action<LevelData> setup)
    {
        string path = $"{LevelFolder}/{name}.asset";
        var level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (level != null) return level;

        level = ScriptableObject.CreateInstance<LevelData>();
        setup(level);
        AssetDatabase.CreateAsset(level, path);
        return level;
    }

    // ---------------- Escena de juego ----------------

    static void AddGameSystems(LevelData[] levels)
    {
        // Managers
        var systems = new GameObject("GameSystems");
        var gm = systems.AddComponent<GameManager>();
        gm.levels = levels;
        systems.AddComponent<ScoreManager>();
        systems.AddComponent<OrderManager>();

        CreateEventSystem();
        Canvas canvas = CreateCanvas("GameCanvas");
        Transform c = canvas.transform;

        // --- Tarjetas de pedidos (arriba a la izquierda) ---
        var orders = NewUI("OrdersPanel", c);
        Place(orders, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(1000, 150));
        var layout = orders.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;

        var list = canvas.gameObject.AddComponent<OrderListUI>();
        list.container = orders;
        list.cardPrefab = CreateOrderCardPrefab();

        // --- HUD (arriba a la derecha) ---
        var hudBg = Img(c, "HUD", PanelDark);
        Place(hudBg.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(300, 150));
        var hud = hudBg.gameObject.AddComponent<HUD>();
        hud.levelText = Txt(hudBg.transform, "LevelText", "Nivel 1", 26, Color.white);
        Place(hud.levelText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(280, 34));
        hud.timeText = Txt(hudBg.transform, "TimeText", "2:30", 54, Color.white);
        Place(hud.timeText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 2), new Vector2(280, 60));
        hud.scoreText = Txt(hudBg.transform, "ScoreText", "Puntos: 0", 30, UIColors.Hex("#F9A825"));
        Place(hud.scoreText.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(280, 38));

        // --- Aviso en pantalla (correcto / no coincide / expirado) ---
        var feedbackText = Txt(c, "FeedbackText", "", 64, Color.white);
        Place(feedbackText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(1200, 180));
        feedbackText.fontStyle = FontStyles.Bold;
        var feedback = canvas.gameObject.AddComponent<FeedbackUI>();
        feedback.text = feedbackText;

        // --- Pausa ---
        var pauseUI = canvas.gameObject.AddComponent<PauseMenuUI>();
        var pausePanel = FullScreenPanel(c, "PausePanel");
        pauseUI.panel = pausePanel;
        var pauseBox = CenterBox(pausePanel.transform, new Vector2(500, 380));
        var pauseTitle = Txt(pauseBox, "Title", "Pausa", 64, Color.white);
        Place(pauseTitle.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(450, 80));
        var resume = Btn(pauseBox, "Continuar", new Vector2(0, -20));
        UnityEventTools.AddPersistentListener(resume.onClick, new UnityAction(pauseUI.Resume));
        var pauseMenu = Btn(pauseBox, "Menú", new Vector2(0, -120));
        UnityEventTools.AddPersistentListener(pauseMenu.onClick, new UnityAction(pauseUI.Menu));

        // --- Pantalla final ---
        var endUI = canvas.gameObject.AddComponent<EndScreenUI>();
        var endPanel = FullScreenPanel(c, "EndPanel");
        endUI.panel = endPanel;
        var endBox = CenterBox(endPanel.transform, new Vector2(700, 640));
        endUI.titleText = Txt(endBox, "Title", "¡Se acabó el tiempo!", 54, Color.white);
        Place(endUI.titleText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(660, 70));
        endUI.scoreText = Txt(endBox, "Score", "Puntos: 0", 44, UIColors.Hex("#F9A825"));
        Place(endUI.scoreText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(660, 60));

        // 3 estrellas (cuadros girados 45° como rombos; luego se cambian por íconos)
        endUI.stars = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var s = Img(endBox, "Star" + (i + 1), Color.white);
            Place(s.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 130, -240), new Vector2(80, 80));
            s.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            endUI.stars[i] = s;
        }

        var retry = Btn(endBox, "Reintentar", new Vector2(0, -60));
        UnityEventTools.AddPersistentListener(retry.onClick, new UnityAction(endUI.Retry));
        var next = Btn(endBox, "Siguiente nivel", new Vector2(0, -160));
        UnityEventTools.AddPersistentListener(next.onClick, new UnityAction(endUI.Next));
        endUI.nextButton = next.gameObject;
        var menu = Btn(endBox, "Menú", new Vector2(0, -260));
        UnityEventTools.AddPersistentListener(menu.onClick, new UnityAction(endUI.Menu));
    }

    /// <summary>Prefab de tarjeta de pedido (se regenera cada vez).</summary>
    static OrderCardUI CreateOrderCardPrefab()
    {
        var root = NewUI("OrderCard", null);
        root.sizeDelta = new Vector2(220, 140);
        root.gameObject.AddComponent<Image>().color = CardColor;
        var card = root.gameObject.AddComponent<OrderCardUI>();

        // Juguete: cuadrito de color + nombre
        card.toySwatch = Img(root, "ToySwatch", Color.gray);
        Place(card.toySwatch.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -10), new Vector2(30, 30));
        card.toyText = Txt(root, "ToyText", "Osito", 30, TextDark);
        Place(card.toyText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(48, -6), new Vector2(165, 38));
        card.toyText.alignment = TextAlignmentOptions.Left;

        // Papel
        var paperGroup = NewUI("PaperGroup", root);
        Place(paperGroup, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-50, -8), new Vector2(90, 70));
        card.paperSwatch = Img(paperGroup, "Swatch", Color.red);
        Place(card.paperSwatch.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(48, 48));
        var pl = Txt(paperGroup, "Label", "Papel", 16, TextDark);
        Place(pl.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(90, 20));

        // Moño
        var bowGroup = NewUI("BowGroup", root);
        Place(bowGroup, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, -8), new Vector2(100, 70));
        card.bowSwatch = Img(bowGroup, "Swatch", Color.yellow);
        Place(card.bowSwatch.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -24), new Vector2(34, 34));
        card.bowSwatch.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        card.bowLabel = Txt(bowGroup, "Label", "Moño", 16, TextDark);
        Place(card.bowLabel.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(100, 20));

        // Barra de tiempo
        var timeBg = Img(root, "TimeBg", UIColors.Hex("#5D4037"));
        Place(timeBg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(200, 14));
        var fill = Img(timeBg.transform, "TimeFill", UIColors.Hex("#66BB6A"));
        Place(fill.rectTransform, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        card.timeFill = fill.rectTransform;
        card.timeFillImage = fill;

        string path = $"{UIPrefabFolder}/OrderCard.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
        Object.DestroyImmediate(root.gameObject);
        return prefab.GetComponent<OrderCardUI>();
    }

    // ---------------- Menú principal ----------------

    static void BuildMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = UIColors.Hex("#4A3F6B");
        camGO.AddComponent<AudioListener>();

        CreateEventSystem();
        Canvas canvas = CreateCanvas("MenuCanvas");
        Transform c = canvas.transform;
        var menuUI = canvas.gameObject.AddComponent<MenuUI>();

        var title = Txt(c, "Title", "Santa's Wrap Factory", 110, Color.white);
        Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1600, 140));
        title.fontStyle = FontStyles.Bold;

        var sub = Txt(c, "Subtitle", "Ayuda a los elfos a envolver los regalos antes de Navidad", 36, UIColors.Hex("#ECEFF1"));
        Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(1600, 60));

        var play = Btn(c, "Jugar", new Vector2(0, -20));
        UnityEventTools.AddPersistentListener(play.onClick, new UnityAction(menuUI.Play));
        var quit = Btn(c, "Salir", new Vector2(0, -130));
        UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(menuUI.Quit));

        var help = Txt(c, "Controls", "WASD / Flechas: moverse     E / Espacio: recoger, soltar, usar     Esc: pausa", 26, UIColors.Hex("#B0BEC5"));
        Place(help.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1600, 40));

        EditorSceneManager.SaveScene(scene, MenuPath);
    }

    // ---------------- Utilidades de UI ----------------

    static void CreateEventSystem()
    {
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>(); // Input System nuevo
    }

    static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent != null) go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static Image Img(Transform parent, string name, Color color)
    {
        var img = NewUI(name, parent).gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI Txt(Transform parent, string name, string text, float size, Color color)
    {
        var t = NewUI(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    /// <summary>Botón rojo de 360x80 centrado horizontalmente.</summary>
    static Button Btn(Transform parent, string label, Vector2 pos)
    {
        var img = Img(parent, "Btn_" + label, ButtonRed);
        Place(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(360, 80));
        var btn = img.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 0.85f, 0.85f);
        btn.colors = colors;

        var t = Txt(img.transform, "Label", label, 36, Color.white);
        Place(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        return btn;
    }

    static GameObject FullScreenPanel(Transform parent, string name)
    {
        var img = Img(parent, name, new Color(0, 0, 0, 0.6f));
        Place(img.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        return img.gameObject;
    }

    static Transform CenterBox(Transform parent, Vector2 size)
    {
        var img = Img(parent, "Box", PanelDark);
        Place(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        return img.transform;
    }
}
