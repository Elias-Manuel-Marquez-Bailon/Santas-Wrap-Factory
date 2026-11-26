using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menú Tools/Santa/Build Prototype Scene.
/// Genera la escena Factory con primitivas: piso, paredes, cámara, luz,
/// jugador y un dispensador de prueba (de ositos).
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
        Material matFloor  = GetMaterial("Mat_Snow",  "#ECEFF1");
        Material matWall   = GetMaterial("Mat_Wood",  "#8D6E63");
        Material matPlayer = GetMaterial("Mat_ElfGreen", "#2E7D32");
        Material matRed    = GetMaterial("Mat_Red",   "#C62828");
        Material matToy    = GetMaterial("Mat_ToyBear", "#A1887F");

        // --- Luz ---
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.65f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // --- Piso 16 x 10 ---
        var env = new GameObject("Environment");
        CreateBox("Floor", new Vector3(0, -0.5f, 0), new Vector3(16, 1, 10), matFloor, env.transform);

        // --- Paredes ---
        CreateBox("Wall_North", new Vector3(0, 1, 5.25f),  new Vector3(16.5f, 2, 0.5f), matWall, env.transform);
        CreateBox("Wall_South", new Vector3(0, 0.5f, -5.25f), new Vector3(16.5f, 1, 0.5f), matWall, env.transform); // baja para no tapar la cámara
        CreateBox("Wall_East",  new Vector3(8.25f, 1, 0),  new Vector3(0.5f, 2, 10), matWall, env.transform);
        CreateBox("Wall_West",  new Vector3(-8.25f, 1, 0), new Vector3(0.5f, 2, 10), matWall, env.transform);

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

        // --- Jugador (cápsula) ---
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>()); // usa CharacterController
        player.transform.position = new Vector3(0, 1f, 0);
        player.GetComponent<Renderer>().sharedMaterial = matPlayer;

        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.5f; cc.center = Vector3.zero;

        // "Nariz" para ver hacia dónde mira
        var nose = CreateBox("Nose", Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f), matRed, player.transform);
        nose.transform.localPosition = new Vector3(0, 0.4f, 0.5f);
        Object.DestroyImmediate(nose.GetComponent<Collider>());

        var hold = new GameObject("HoldPoint").transform;
        hold.SetParent(player.transform);
        hold.localPosition = new Vector3(0, 0.2f, 0.8f);

        player.AddComponent<PlayerMovement>();
        var carry = player.AddComponent<PlayerCarry>();
        carry.holdPoint = hold;
        player.AddComponent<PlayerInteraction>();

        // --- Prefab de item de prueba: osito (esfera) ---
        Item bearPrefab = CreateItemPrefab("Item_Toy_Bear", PrimitiveType.Sphere, matToy, ItemType.Toy, ToyKind.Bear);

        // --- Dispensador de prueba ---
        var stations = new GameObject("Stations");
        var disp = CreateBox("Dispenser_Bear", new Vector3(-5f, 0.5f, 3.5f), new Vector3(1.2f, 1f, 1.2f), matRed, stations.transform);
        var d = disp.AddComponent<Dispenser>();
        d.itemPrefab = bearPrefab;
        d.type = ItemType.Toy;
        d.toy = ToyKind.Bear;

        // Ícono encima del dispensador mostrando qué entrega
        var icon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        icon.name = "Icon";
        Object.DestroyImmediate(icon.GetComponent<Collider>());
        icon.transform.SetParent(disp.transform);
        icon.transform.localPosition = new Vector3(0, 0.7f, 0);
        icon.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        icon.GetComponent<Renderer>().sharedMaterial = matToy;

        // --- Guardar escena y agregarla a Build Settings ---
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuild(ScenePath);
        Debug.Log("[Santa] Escena Factory generada en " + ScenePath);
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

    /// <summary>Crea (o reutiliza) un prefab de item hecho con una primitiva.</summary>
    static Item CreateItemPrefab(string name, PrimitiveType shape, Material mat, ItemType type, ToyKind toy)
    {
        string path = $"{ItemPrefabFolder}/{name}.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<Item>(path);
        if (existing != null) return existing;

        var go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.GetComponent<Collider>().isTrigger = true; // el jugador no choca con items
        var item = go.AddComponent<Item>();
        item.type = type;
        item.toy = toy;

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
