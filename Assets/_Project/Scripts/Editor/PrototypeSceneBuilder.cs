using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menú Tools/Santa/Build Prototype Scene.
/// Genera la escena Factory con primitivas: piso, paredes, cámara, luz, jugador,
/// dispensadores (juguete, caja, papeles, moños), mesa de ensamblado, envolvedora y costal.
/// Los materiales y prefabs de items se crean una vez y se reutilizan.
/// </summary>
public static class PrototypeSceneBuilder
{
    const string ScenePath = "Assets/_Project/Scenes/Factory.unity";
    const string MatFolder = "Assets/_Project/Art/Materials";
    const string ItemPrefabFolder = "Assets/_Project/Prefabs/Items";

    [MenuItem("Tools/Santa/Build Prototype Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Materiales (paleta del CLAUDE.md) ---
        Material matFloor    = GetMaterial("Mat_Snow",      "#ECEFF1");
        Material matWood     = GetMaterial("Mat_Wood",      "#8D6E63");
        Material matPlayer   = GetMaterial("Mat_ElfGreen",  "#2E7D32");
        Material matRed      = GetMaterial("Mat_Red",       "#C62828");
        Material matGreen    = GetMaterial("Mat_Green",     "#2E7D32");
        Material matBlue     = GetMaterial("Mat_Blue",      "#1565C0");
        Material matGold     = GetMaterial("Mat_Gold",      "#F9A825");
        Material matSilver   = GetMaterial("Mat_Silver",    "#B0BEC5");
        Material matToy      = GetMaterial("Mat_ToyBear",   "#A1887F");
        Material matCardboard= GetMaterial("Mat_Cardboard", "#D7B98E");
        Material matMetal    = GetMaterial("Mat_Metal",     "#607D8B");
        Material matSack     = GetMaterial("Mat_Sack",      "#6D4C41");
        Material matBarBg    = GetMaterial("Mat_BarBg",     "#212121");
        Material matBarFill  = GetMaterial("Mat_BarFill",   "#66BB6A");
        Material matGiftBase = GetMaterial("Mat_GiftBase",  "#FFFFFF");

        // --- Prefabs de items ---
        Item bear      = MakeItemPrefab("Item_Toy_Bear",   ItemType.Toy,   () => Sphere(matToy, 0.5f));
        Item box       = MakeItemPrefab("Item_Box",        ItemType.Box,   () => Cube(matCardboard, new Vector3(0.5f, 0.5f, 0.5f)));
        Item paperR    = MakeItemPrefab("Item_Paper_Red",  ItemType.Paper, () => Cube(matRed,   new Vector3(0.6f, 0.06f, 0.6f)));
        Item paperG    = MakeItemPrefab("Item_Paper_Green",ItemType.Paper, () => Cube(matGreen, new Vector3(0.6f, 0.06f, 0.6f)));
        Item paperB    = MakeItemPrefab("Item_Paper_Blue", ItemType.Paper, () => Cube(matBlue,  new Vector3(0.6f, 0.06f, 0.6f)));
        Item bowGold   = MakeItemPrefab("Item_Bow_Gold",   ItemType.Bow,   () => BowShape(matGold));
        Item bowSilver = MakeItemPrefab("Item_Bow_Silver", ItemType.Bow,   () => BowShape(matSilver));
        Item boxToy    = MakeItemPrefab("Item_BoxWithToy", ItemType.BoxWithToy, () => BoxWithToyShape(matCardboard, matToy));
        Item gift      = MakeItemPrefab("Item_Gift",       ItemType.GiftReady,  () => GiftShape(matGiftBase, matGold));

        // --- Luz ---
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.65f);

        // --- Piso y paredes (piso 16 x 10) ---
        var env = new GameObject("Environment");
        CreateBox("Floor",      new Vector3(0, -0.5f, 0),     new Vector3(16, 1, 10),       matFloor, env.transform);
        CreateBox("Wall_North", new Vector3(0, 1, 5.25f),     new Vector3(16.5f, 2, 0.5f),  matWood, env.transform);
        CreateBox("Wall_South", new Vector3(0, 0.5f, -5.25f), new Vector3(16.5f, 1, 0.5f),  matWood, env.transform); // baja para no tapar la cámara
        CreateBox("Wall_East",  new Vector3(8.25f, 1, 0),     new Vector3(0.5f, 2, 10),     matWood, env.transform);
        CreateBox("Wall_West",  new Vector3(-8.25f, 1, 0),    new Vector3(0.5f, 2, 10),     matWood, env.transform);

        // --- Cámara cenital inclinada (estilo Overcooked) ---
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexColor("#4A3F6B");
        cam.fieldOfView = 45f;
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 13f, -10f);
        camGO.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

        // --- Jugador ---
        BuildPlayer(matPlayer, matRed);

        // --- Estaciones ---
        var stations = new GameObject("Stations").transform;

        // Fila de dispensadores junto a la pared norte
        BuildDispenser("Dispenser_Bear",       -6.5f, bear,      ItemType.Toy,   matRed,   stations, toy: ToyKind.Bear);
        BuildDispenser("Dispenser_Box",        -5f,   box,       ItemType.Box,   matRed,   stations);
        BuildDispenser("Dispenser_Paper_Red",  -2f,   paperR,    ItemType.Paper, matWood,  stations, paper: PaperColor.Red);
        BuildDispenser("Dispenser_Paper_Green",-0.5f, paperG,    ItemType.Paper, matWood,  stations, paper: PaperColor.Green);
        BuildDispenser("Dispenser_Paper_Blue",  1f,   paperB,    ItemType.Paper, matWood,  stations, paper: PaperColor.Blue);
        BuildDispenser("Dispenser_Bow_Gold",    4f,   bowGold,   ItemType.Bow,   matMetal, stations, bow: BowKind.Gold);
        BuildDispenser("Dispenser_Bow_Silver",  5.5f, bowSilver, ItemType.Bow,   matMetal, stations, bow: BowKind.Silver);

        // Mesa de ensamblado
        var table = CreateBox("AssemblyTable", new Vector3(-4f, 0.5f, 0f), new Vector3(2f, 1f, 1.2f), matWood, stations);
        var at = table.AddComponent<AssemblyTable>();
        at.resultPrefab = boxToy;
        at.slotPoint = CreateSlot(table.transform);
        at.progressBar = CreateProgressBar(table.transform, matBarBg, matBarFill);

        // Máquina envolvedora
        var wrapper = CreateBox("WrappingMachine", new Vector3(1.5f, 0.6f, 0f), new Vector3(2f, 1.2f, 1.4f), matMetal, stations);
        var wm = wrapper.AddComponent<WrappingMachine>();
        wm.giftPrefab = gift;
        wm.slotPoint = CreateSlot(wrapper.transform);
        wm.progressBar = CreateProgressBar(wrapper.transform, matBarBg, matBarFill);
        wm.paperMaterials = new[] { matRed, matGreen, matBlue };
        wm.bowMaterials = new[] { matGold, matSilver };

        // Costal de Santa
        var bag = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bag.name = "DeliveryBag";
        bag.transform.SetParent(stations);
        bag.transform.position = new Vector3(6.5f, 0.6f, -3f);
        bag.transform.localScale = new Vector3(1.4f, 0.6f, 1.4f);
        bag.GetComponent<Renderer>().sharedMaterial = matSack;
        bag.AddComponent<DeliveryBag>();

        // --- Guardar escena y agregarla a Build Settings ---
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuild(ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[Santa] Escena Factory generada en " + ScenePath);
    }

    // ---------- piezas de la escena ----------

    static void BuildPlayer(Material body, Material nose)
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>()); // usa CharacterController
        player.transform.position = new Vector3(0, 1f, -2.5f);
        player.GetComponent<Renderer>().sharedMaterial = body;

        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.5f; cc.center = Vector3.zero;

        // "Nariz" para ver hacia dónde mira
        var n = CreateBox("Nose", Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f), nose, player.transform);
        n.transform.localPosition = new Vector3(0, 0.4f, 0.5f);
        Object.DestroyImmediate(n.GetComponent<Collider>());

        var hold = new GameObject("HoldPoint").transform;
        hold.SetParent(player.transform);
        hold.localPosition = new Vector3(0, 0.2f, 0.8f);

        player.AddComponent<PlayerMovement>();
        player.AddComponent<PlayerCarry>().holdPoint = hold;
        player.AddComponent<PlayerInteraction>();
    }

    static void BuildDispenser(string name, float x, Item prefab, ItemType type, Material mat, Transform parent,
                               ToyKind toy = ToyKind.None, PaperColor paper = PaperColor.None, BowKind bow = BowKind.None)
    {
        var go = CreateBox(name, new Vector3(x, 0.5f, 4f), new Vector3(1.2f, 1f, 1.2f), mat, parent);
        var d = go.AddComponent<Dispenser>();
        d.itemPrefab = prefab;
        d.type = type; d.toy = toy; d.paper = paper; d.bow = bow;

        // Ícono encima: copia visual del item (sin lógica ni collider)
        var icon = Object.Instantiate(prefab.gameObject);
        icon.name = "Icon";
        Object.DestroyImmediate(icon.GetComponent<Item>());
        foreach (var c in icon.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        icon.transform.position = go.transform.position + Vector3.up * 0.8f;
        icon.transform.SetParent(go.transform, true);
    }

    /// <summary>Punto sobre la estación donde se acomodan los items.</summary>
    static Transform CreateSlot(Transform station)
    {
        var slot = new GameObject("SlotPoint").transform;
        slot.SetParent(station, false);
        // en coordenadas del mundo: justo encima de la superficie
        slot.position = station.position + Vector3.up * (station.localScale.y * 0.5f + 0.3f);
        slot.localScale = new Vector3(1f / station.localScale.x, 1f / station.localScale.y, 1f / station.localScale.z);
        return slot;
    }

    static ProgressBar CreateProgressBar(Transform station, Material bg, Material fill)
    {
        // Hija del grupo "Stations" (sin escala) para que no se deforme al girar hacia la cámara
        var root = new GameObject("ProgressBar_" + station.name);
        root.transform.SetParent(station.parent, false);
        root.transform.position = station.position + Vector3.up * (station.localScale.y * 0.5f + 1.3f);

        var back = CreateBox("Background", Vector3.zero, new Vector3(1.1f, 0.25f, 0.02f), bg, root.transform);
        Object.DestroyImmediate(back.GetComponent<Collider>());
        var f = CreateBox("Fill", Vector3.zero, new Vector3(1f, 0.17f, 0.02f), fill, root.transform);
        Object.DestroyImmediate(f.GetComponent<Collider>());

        var bar = root.AddComponent<ProgressBar>();
        bar.fill = f.transform;
        bar.width = 1f;
        return bar;
    }

    // ---------- formas de items ----------

    static GameObject Sphere(Material m, float size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.localScale = Vector3.one * size;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }

    static GameObject Cube(Material m, Vector3 size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }

    /// <summary>Moño: dos cubos achatados en cruz.</summary>
    static GameObject BowShape(Material m)
    {
        var root = Cube(m, new Vector3(0.45f, 0.1f, 0.12f));
        var cross = Cube(m, Vector3.one);
        Object.DestroyImmediate(cross.GetComponent<Collider>());
        cross.transform.SetParent(root.transform, false);
        cross.transform.localScale = new Vector3(0.27f, 1f, 3.75f); // relativo al padre → 0.12 x 0.1 x 0.45
        return root;
    }

    /// <summary>Caja de cartón con la cabeza del juguete asomándose.</summary>
    static GameObject BoxWithToyShape(Material box, Material toy)
    {
        var root = Cube(box, new Vector3(0.5f, 0.5f, 0.5f));
        var head = Sphere(toy, 1f);
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0, 0.55f, 0);
        head.transform.localScale = Vector3.one * 0.5f;
        return root;
    }

    /// <summary>Regalo: caja (se pinta del color del papel) + hijo "Bow".</summary>
    static GameObject GiftShape(Material baseMat, Material bowMat)
    {
        var root = Cube(baseMat, new Vector3(0.55f, 0.55f, 0.55f));
        var bow = BowShape(bowMat);
        Object.DestroyImmediate(bow.GetComponent<Collider>());
        bow.name = "Bow";
        bow.transform.SetParent(root.transform, false);
        bow.transform.localPosition = new Vector3(0, 0.55f, 0);
        bow.transform.localScale = new Vector3(0.45f, 0.1f, 0.12f) / 0.55f;
        return root;
    }

    // ---------- utilidades ----------

    static GameObject CreateBox(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    /// <summary>
    /// Crea (o reutiliza) un prefab de item. La raíz lleva el collider (Trigger) y el componente Item.
    /// Si ya existe, no se toca (así puedes reemplazar el modelo a mano sin que se pierda).
    /// </summary>
    static Item MakeItemPrefab(string name, ItemType type, System.Func<GameObject> buildShape)
    {
        string path = $"{ItemPrefabFolder}/{name}.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<Item>(path);
        if (existing != null) return existing;

        var go = buildShape();
        go.name = name;
        go.GetComponent<Collider>().isTrigger = true; // el jugador no choca con items
        go.AddComponent<Item>().type = type;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<Item>();
    }

    /// <summary>Crea (o reutiliza) un material URP Lit de color plano.</summary>
    static Material GetMaterial(string name, string hex)
    {
        string path = $"{MatFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        mat = new Material(shader);
        Color c = HexColor(hex);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        mat.color = c;
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    static void AddSceneToBuild(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
