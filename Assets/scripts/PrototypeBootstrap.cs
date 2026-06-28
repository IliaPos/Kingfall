using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class PrototypeBootstrap : MonoBehaviour
{
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreatePrototypeIfNeeded()
    {
        if (FindAnyObjectByType<PrototypeBootstrap>() != null)
        {
            return;
        }

        if (FindAnyObjectByType<KingController>() != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("Prototype Bootstrap");
        bootstrap.AddComponent<PrototypeBootstrap>().Build();
    }
#endif

    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private RewardDefinition[] rewardDefinitions;

    private void Start()
    {
        if (buildOnStart && FindAnyObjectByType<KingController>() == null)
        {
            Build();
        }
    }

    public void Build()
    {
        PrototypeMapGenerator map = new GameObject("Prototype Map").AddComponent<PrototypeMapGenerator>();
        map.Generate();

        Castle castle = CreateCastle();
        RunEconomy economy = new GameObject("Run Economy").AddComponent<RunEconomy>();
        RunStats runStats = new GameObject("Run Stats").AddComponent<RunStats>();

        GameObject king = CreateKing();
        KeyboardPlayerInputSource inputSource = king.AddComponent<KeyboardPlayerInputSource>();
        KingController controller = king.AddComponent<KingController>();
        controller.MapRadius = map.MapRadius - 1.5f;
        KingCombat kingCombat = king.AddComponent<KingCombat>();
        kingCombat.Initialize(runStats);

        Camera camera = CreateCamera(king.transform);
        controller.CameraTransform = camera.transform;
        RewardManager rewardManager = CreateRewardManager(runStats, economy, castle, inputSource, GetRewardDefinitions());
        WaveManager waveManager = CreateWaveManager(castle, economy, rewardManager, inputSource, map.MapRadius);
        CreateBuildSystem(waveManager, economy, runStats, king.transform);
        CreateLight();
    }

    private static Castle CreateCastle()
    {
        Material wallMaterial = CreateMaterial("Prototype Castle Wall", new Color(0.55f, 0.57f, 0.62f));
        Material roofMaterial = CreateMaterial("Prototype Castle Roof", new Color(0.52f, 0.16f, 0.15f));

        GameObject root = new GameObject("Castle");
        root.transform.position = Vector3.zero;

        GameObject attackPoint = new GameObject("Castle Attack Point");
        attackPoint.transform.SetParent(root.transform);
        attackPoint.transform.localPosition = Vector3.zero;

        GameObject baseTower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseTower.name = "Castle Keep";
        baseTower.transform.SetParent(root.transform);
        baseTower.transform.localPosition = new Vector3(0f, 1f, 0f);
        baseTower.transform.localScale = new Vector3(1.8f, 1f, 1.8f);
        baseTower.GetComponent<Renderer>().sharedMaterial = wallMaterial;
        RemoveCollider(baseTower);

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "Castle Roof";
        roof.transform.SetParent(root.transform);
        roof.transform.localPosition = new Vector3(0f, 2.1f, 0f);
        roof.transform.localScale = new Vector3(1.35f, 0.35f, 1.35f);
        roof.GetComponent<Renderer>().sharedMaterial = roofMaterial;
        RemoveCollider(roof);

        Health health = root.AddComponent<Health>();
        health.SetMaxHealth(250f);
        Castle castle = root.AddComponent<Castle>();
        castle.SetAttackPoint(attackPoint.transform);
        return castle;
    }

    private RewardDefinition[] GetRewardDefinitions()
    {
        if (rewardDefinitions != null && rewardDefinitions.Length > 0)
        {
            return rewardDefinitions;
        }

#if UNITY_EDITOR
        rewardDefinitions = LoadRewardDefinitionsFromAssets();
#endif
        return rewardDefinitions;
    }

#if UNITY_EDITOR
    private static RewardDefinition[] LoadRewardDefinitionsFromAssets()
    {
        string[] paths =
        {
            "Assets/Data/Rewards/Sharper Sword.asset",
            "Assets/Data/Rewards/Better Arrows.asset",
            "Assets/Data/Rewards/Watch Posts.asset",
            "Assets/Data/Rewards/Fast Bowstrings.asset",
            "Assets/Data/Rewards/Royal Taxes.asset",
            "Assets/Data/Rewards/Stone Reinforcement.asset",
            "Assets/Data/Rewards/Repair Crew.asset",
        };

        RewardDefinition[] definitions = new RewardDefinition[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            definitions[i] = AssetDatabase.LoadAssetAtPath<RewardDefinition>(paths[i]);
            if (definitions[i] == null)
            {
                Debug.LogWarning($"Reward definition asset is missing: {paths[i]}");
            }
        }

        return definitions;
    }
#endif

    private static RewardManager CreateRewardManager(RunStats runStats, RunEconomy economy, Castle castle, IPlayerInputSource inputSource, RewardDefinition[] rewardDefinitions)
    {
        RewardManager rewardManager = new GameObject("Reward Manager").AddComponent<RewardManager>();
        rewardManager.SetRewardDefinitions(rewardDefinitions);
        rewardManager.Initialize(runStats, economy, castle, inputSource);
        return rewardManager;
    }

    private static WaveManager CreateWaveManager(Castle castle, RunEconomy economy, RewardManager rewardManager, IPlayerInputSource inputSource, float mapRadius)
    {
        WaveManager waveManager = new GameObject("Wave Manager").AddComponent<WaveManager>();
        waveManager.Initialize(castle, economy, rewardManager, inputSource, mapRadius);
        return waveManager;
    }

    private static void CreateBuildSystem(WaveManager waveManager, RunEconomy economy, RunStats runStats, Transform builder)
    {
        BuildSystem buildSystem = new GameObject("Build System").AddComponent<BuildSystem>();
        buildSystem.Initialize(waveManager, economy, runStats, builder);
    }

    private static GameObject CreateKing()
    {
        GameObject root = new GameObject("King Player");
        root.transform.position = new Vector3(0f, 0.55f, -4f);

        Material horseMaterial = CreateMaterial("Prototype Horse", new Color(0.43f, 0.25f, 0.12f));
        Material kingMaterial = CreateMaterial("Prototype King", new Color(0.1f, 0.31f, 0.75f));
        Material crownMaterial = CreateMaterial("Prototype Crown", new Color(1f, 0.72f, 0.12f));

        GameObject horse = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        horse.name = "Horse Placeholder";
        horse.transform.SetParent(root.transform);
        horse.transform.localPosition = new Vector3(0f, 0f, 0f);
        horse.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        horse.transform.localScale = new Vector3(0.75f, 0.75f, 1.35f);
        horse.GetComponent<Renderer>().sharedMaterial = horseMaterial;
        RemoveCollider(horse);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "King Body Placeholder";
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        body.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
        body.GetComponent<Renderer>().sharedMaterial = kingMaterial;
        RemoveCollider(body);

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crown.name = "Crown Placeholder";
        crown.transform.SetParent(root.transform);
        crown.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        crown.transform.localScale = new Vector3(0.33f, 0.12f, 0.33f);
        crown.GetComponent<Renderer>().sharedMaterial = crownMaterial;
        RemoveCollider(crown);

        GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sword.name = "Sword Placeholder";
        sword.transform.SetParent(root.transform);
        sword.transform.localPosition = new Vector3(0.55f, 1.1f, 0.55f);
        sword.transform.localRotation = Quaternion.Euler(35f, 45f, 0f);
        sword.transform.localScale = new Vector3(0.12f, 0.75f, 0.12f);
        sword.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Prototype Sword", new Color(0.78f, 0.82f, 0.86f));
        RemoveCollider(sword);

        return root;
    }

    private static Camera CreateCamera(Transform target)
    {
        Camera existingCamera = Camera.main;
        GameObject cameraObject = existingCamera != null ? existingCamera.gameObject : new GameObject("Main Camera");

        Camera camera = cameraObject.GetComponent<Camera>();
        if (camera == null)
        {
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 14f;
        camera.transform.position = target.position + new Vector3(0f, 18f, -16f);
        camera.transform.rotation = Quaternion.Euler(50f, 45f, 0f);

        IsoCameraFollow follow = cameraObject.GetComponent<IsoCameraFollow>();
        if (follow == null)
        {
            follow = cameraObject.AddComponent<IsoCameraFollow>();
        }

        follow.Target = target;
        return camera;
    }

    private static void CreateLight()
    {
        if (FindAnyObjectByType<Light>() != null)
        {
            return;
        }

        GameObject lightObject = new GameObject("Prototype Sun");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.4f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        return material;
    }

    private static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }
}
