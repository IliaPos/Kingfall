using UnityEngine;

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

        GameObject king = CreateKing();
        king.AddComponent<KeyboardPlayerInputSource>();
        KingController controller = king.AddComponent<KingController>();
        controller.MapRadius = map.MapRadius - 1.5f;

        Camera camera = CreateCamera(king.transform);
        controller.CameraTransform = camera.transform;
        CreateLight();
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

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "King Body Placeholder";
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        body.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
        body.GetComponent<Renderer>().sharedMaterial = kingMaterial;

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crown.name = "Crown Placeholder";
        crown.transform.SetParent(root.transform);
        crown.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        crown.transform.localScale = new Vector3(0.33f, 0.12f, 0.33f);
        crown.GetComponent<Renderer>().sharedMaterial = crownMaterial;

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
}
