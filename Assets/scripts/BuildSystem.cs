using UnityEngine;

public sealed class BuildSystem : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private RunEconomy economy;
    [SerializeField] private Transform builder;
    [SerializeField] private int towerCost = 40;
    [SerializeField] private float placementDistance = 3f;
    [SerializeField] private float minDistanceFromCastle = 4.5f;
    [SerializeField] private float minDistanceBetweenTowers = 2.4f;

    private IPlayerInputSource inputSource;
    private Material towerMaterial;
    private Material roofMaterial;

    public int TowerCost => towerCost;

    public void Initialize(WaveManager manager, RunEconomy runEconomy, Transform builderTransform)
    {
        waveManager = manager;
        economy = runEconomy;
        builder = builderTransform;
    }

    private void Awake()
    {
        if (builder != null)
        {
            inputSource = builder.GetComponent<IPlayerInputSource>();
        }

        towerMaterial = CreateMaterial("Prototype Tower", new Color(0.45f, 0.34f, 0.24f));
        roofMaterial = CreateMaterial("Prototype Tower Roof", new Color(0.24f, 0.32f, 0.52f));
    }

    private void Update()
    {
        if (inputSource == null && builder != null)
        {
            inputSource = builder.GetComponent<IPlayerInputSource>();
        }

        if (inputSource == null || !inputSource.BuildPressed)
        {
            return;
        }

        TryBuildTower();
    }

    private void TryBuildTower()
    {
        if (waveManager == null || waveManager.Phase != GamePhase.Preparation || economy == null || builder == null)
        {
            return;
        }

        Vector3 position = builder.position + builder.forward * placementDistance;
        position.y = 0f;

        if (position.magnitude < minDistanceFromCastle || IsTooCloseToExistingTower(position))
        {
            return;
        }

        if (!economy.TrySpend(towerCost))
        {
            return;
        }

        CreateTower(position);
    }

    private bool IsTooCloseToExistingTower(Vector3 position)
    {
        Tower[] towers = FindObjectsByType<Tower>();
        float minSqrDistance = minDistanceBetweenTowers * minDistanceBetweenTowers;
        for (int i = 0; i < towers.Length; i++)
        {
            Vector3 toTower = towers[i].transform.position - position;
            toTower.y = 0f;
            if (toTower.sqrMagnitude < minSqrDistance)
            {
                return true;
            }
        }

        return false;
    }

    private void CreateTower(Vector3 position)
    {
        GameObject root = new GameObject("Archer Tower");
        root.transform.position = position;

        GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObject.name = "Tower Base";
        baseObject.transform.SetParent(root.transform);
        baseObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        baseObject.transform.localScale = new Vector3(0.75f, 0.8f, 0.75f);
        baseObject.GetComponent<Renderer>().sharedMaterial = towerMaterial;

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "Tower Roof";
        roof.transform.SetParent(root.transform);
        roof.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        roof.transform.localScale = new Vector3(0.95f, 0.2f, 0.95f);
        roof.GetComponent<Renderer>().sharedMaterial = roofMaterial;

        GameObject shootPoint = new GameObject("Shoot Point");
        shootPoint.transform.SetParent(root.transform);
        shootPoint.transform.localPosition = new Vector3(0f, 1.85f, 0f);

        Tower tower = root.AddComponent<Tower>();
        tower.SetShootPoint(shootPoint.transform);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        return material;
    }
}
