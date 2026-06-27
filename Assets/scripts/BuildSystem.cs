using UnityEngine;

public enum BuildableType
{
    Tower,
    Wall
}

public sealed class BuildSystem : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private RunEconomy economy;
    [SerializeField] private RunStats runStats;
    [SerializeField] private Transform builder;
    [SerializeField] private int towerCost = 40;
    [SerializeField] private int wallCost = 18;
    [SerializeField] private float placementDistance = 3f;
    [SerializeField] private float minDistanceFromCastle = 4.5f;
    [SerializeField] private float minDistanceBetweenBuildings = 2.4f;

    private IPlayerInputSource inputSource;
    private Material towerMaterial;
    private Material roofMaterial;
    private Material validGhostMaterial;
    private Material invalidGhostMaterial;
    private GameObject ghostTower;
    private Renderer[] ghostRenderers;
    private bool isBuilding;
    private bool canPlaceGhost;
    private BuildableType selectedType;

    public int TowerCost => towerCost;
    public int WallCost => wallCost;
    public bool IsBuilding => isBuilding;
    public bool CanPlaceGhost => canPlaceGhost;
    public BuildableType SelectedType => selectedType;

    public void Initialize(WaveManager manager, RunEconomy runEconomy, RunStats stats, Transform builderTransform)
    {
        waveManager = manager;
        economy = runEconomy;
        runStats = stats;
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
        validGhostMaterial = CreateMaterial("Valid Tower Ghost", new Color(0.25f, 0.85f, 0.35f, 0.55f));
        invalidGhostMaterial = CreateMaterial("Invalid Tower Ghost", new Color(0.9f, 0.2f, 0.18f, 0.55f));
    }

    private void Update()
    {
        if (inputSource == null && builder != null)
        {
            inputSource = builder.GetComponent<IPlayerInputSource>();
        }

        if (inputSource == null)
        {
            return;
        }

        if (inputSource.BuildPressed)
        {
            ToggleBuildMode();
        }

        if (inputSource.SwitchBuildPressed)
        {
            SwitchBuildableType();
        }

        if (!isBuilding)
        {
            return;
        }

        UpdateGhost();

        if (inputSource.CancelPressed)
        {
            SetBuildMode(false);
            return;
        }

        if (inputSource.ConfirmPressed)
        {
            TryBuildTower();
        }
    }

    private void ToggleBuildMode()
    {
        SetBuildMode(!isBuilding);
    }

    private void SwitchBuildableType()
    {
        selectedType = selectedType == BuildableType.Tower ? BuildableType.Wall : BuildableType.Tower;
        DestroyGhost();

        if (isBuilding)
        {
            CreateGhostIfNeeded();
            UpdateGhost();
        }
    }

    private void SetBuildMode(bool active)
    {
        isBuilding = active;
        if (isBuilding)
        {
            CreateGhostIfNeeded();
            UpdateGhost();
            return;
        }

        if (ghostTower != null)
        {
            ghostTower.SetActive(false);
        }
    }

    private void UpdateGhost()
    {
        CreateGhostIfNeeded();

        Vector3 position = GetPlacementPosition();
        ghostTower.transform.position = position;
        ghostTower.transform.rotation = selectedType == BuildableType.Wall ? Quaternion.Euler(0f, builder.eulerAngles.y, 0f) : Quaternion.identity;
        ghostTower.SetActive(true);

        canPlaceGhost = IsValidPlacement(position) && economy != null && economy.Gold >= GetSelectedCost();
        Material material = canPlaceGhost ? validGhostMaterial : invalidGhostMaterial;
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostRenderers[i].sharedMaterial = material;
        }
    }

    private void TryBuildTower()
    {
        if (economy == null || builder == null)
        {
            return;
        }

        Vector3 position = GetPlacementPosition();

        if (!IsValidPlacement(position))
        {
            return;
        }

        if (!economy.TrySpend(GetSelectedCost()))
        {
            return;
        }

        if (selectedType == BuildableType.Tower)
        {
            CreateTower(position);
        }
        else
        {
            CreateWall(position, builder.rotation);
        }

        SetBuildMode(false);
    }

    private int GetSelectedCost()
    {
        return selectedType == BuildableType.Tower ? towerCost : wallCost;
    }

    private Vector3 GetPlacementPosition()
    {
        Vector3 position = builder.position + builder.forward * placementDistance;
        position.y = 0f;
        return position;
    }

    private bool IsValidPlacement(Vector3 position)
    {
        return position.magnitude >= minDistanceFromCastle && !IsTooCloseToExistingBuilding(position);
    }

    private bool IsTooCloseToExistingBuilding(Vector3 position)
    {
        float minSqrDistance = minDistanceBetweenBuildings * minDistanceBetweenBuildings;
        for (int i = 0; i < Tower.Active.Count; i++)
        {
            Tower tower = Tower.Active[i];
            if (tower == null)
            {
                continue;
            }

            Vector3 toTower = tower.transform.position - position;
            toTower.y = 0f;
            if (toTower.sqrMagnitude < minSqrDistance)
            {
                return true;
            }
        }

        for (int i = 0; i < Wall.Active.Count; i++)
        {
            Wall wall = Wall.Active[i];
            if (wall == null)
            {
                continue;
            }

            Vector3 toWall = wall.transform.position - position;
            toWall.y = 0f;
            if (toWall.sqrMagnitude < minSqrDistance)
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
        RemoveCollider(baseObject);

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "Tower Roof";
        roof.transform.SetParent(root.transform);
        roof.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        roof.transform.localScale = new Vector3(0.95f, 0.2f, 0.95f);
        roof.GetComponent<Renderer>().sharedMaterial = roofMaterial;
        RemoveCollider(roof);

        GameObject shootPoint = new GameObject("Shoot Point");
        shootPoint.transform.SetParent(root.transform);
        shootPoint.transform.localPosition = new Vector3(0f, 1.85f, 0f);

        Tower tower = root.AddComponent<Tower>();
        tower.Initialize(runStats);
        tower.SetShootPoint(shootPoint.transform);
    }

    private void CreateWall(Vector3 position, Quaternion rotation)
    {
        GameObject root = new GameObject("Wall");
        root.transform.position = position;
        root.transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Wall Visual";
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        visual.transform.localScale = new Vector3(3.9f, 1.3f, 0.45f);
        visual.GetComponent<Renderer>().sharedMaterial = towerMaterial;
        RemoveCollider(visual);

        Health health = root.AddComponent<Health>();
        health.SetMaxHealth(140f);
        root.AddComponent<Wall>();
    }

    private void CreateGhostIfNeeded()
    {
        if (ghostTower != null)
        {
            return;
        }

        ghostTower = new GameObject("Building Placement Ghost");

        if (selectedType == BuildableType.Tower)
        {
            GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseObject.name = "Ghost Tower Base";
            baseObject.transform.SetParent(ghostTower.transform);
            baseObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            baseObject.transform.localScale = new Vector3(0.75f, 0.8f, 0.75f);
            RemoveCollider(baseObject);

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roof.name = "Ghost Tower Roof";
            roof.transform.SetParent(ghostTower.transform);
            roof.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            roof.transform.localScale = new Vector3(0.95f, 0.2f, 0.95f);
            RemoveCollider(roof);
        }
        else
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Ghost Wall";
            wall.transform.SetParent(ghostTower.transform);
            wall.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            wall.transform.localScale = new Vector3(3.9f, 1.3f, 0.45f);
            RemoveCollider(wall);
        }

        ghostRenderers = ghostTower.GetComponentsInChildren<Renderer>();
        ghostTower.SetActive(false);
    }

    private void DestroyGhost()
    {
        if (ghostTower == null)
        {
            return;
        }

        Destroy(ghostTower);
        ghostTower = null;
        ghostRenderers = null;
    }

    private static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private void OnGUI()
    {
        if (!isBuilding)
        {
            return;
        }

        string label = selectedType == BuildableType.Tower ? "Tower" : "Wall";
        GUI.Box(new Rect(Screen.width * 0.5f - 130f, Screen.height - 86f, 260f, 54f), string.Empty);
        GUI.Label(new Rect(Screen.width * 0.5f - 112f, Screen.height - 72f, 230f, 22f), $"Building: {label} | Cost: {GetSelectedCost()}");
        GUI.Label(new Rect(Screen.width * 0.5f - 112f, Screen.height - 50f, 230f, 22f), "Q switch | LMB place | Esc cancel");
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        return material;
    }
}
