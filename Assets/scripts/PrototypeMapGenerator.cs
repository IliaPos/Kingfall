using UnityEngine;

public sealed class PrototypeMapGenerator : MonoBehaviour
{
    [SerializeField] private int seed = 12345;
    [SerializeField] private float mapRadius = 70f;
    [SerializeField] private int treeCount = 180;
    [SerializeField] private int rockCount = 65;

    private Material grassMaterial;
    private Material dirtMaterial;
    private Material treeMaterial;
    private Material trunkMaterial;
    private Material rockMaterial;
    private System.Random random;

    public float MapRadius => mapRadius;

    public void Generate()
    {
        random = new System.Random(seed);
        CreateMaterials();
        CreateGround();
        CreateCenterMarker();
        CreateBoundaryMarkers();
        ScatterTrees();
        ScatterRocks();
    }

    private void CreateMaterials()
    {
        grassMaterial = CreateMaterial("Prototype Grass", new Color(0.35f, 0.72f, 0.28f));
        dirtMaterial = CreateMaterial("Prototype Dirt", new Color(0.55f, 0.39f, 0.22f));
        treeMaterial = CreateMaterial("Prototype Tree Leaves", new Color(0.12f, 0.47f, 0.18f));
        trunkMaterial = CreateMaterial("Prototype Tree Trunk", new Color(0.42f, 0.25f, 0.11f));
        rockMaterial = CreateMaterial("Prototype Rock", new Color(0.44f, 0.48f, 0.52f));
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        return material;
    }

    private void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "Prototype Map Ground";
        ground.transform.SetParent(transform);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(mapRadius * 2f, 0.08f, mapRadius * 2f);
        ground.GetComponent<Renderer>().sharedMaterial = grassMaterial;
    }

    private void CreateCenterMarker()
    {
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        center.name = "Future Castle Area";
        center.transform.SetParent(transform);
        center.transform.position = new Vector3(0f, 0.08f, 0f);
        center.transform.localScale = new Vector3(4.5f, 0.04f, 4.5f);
        center.GetComponent<Renderer>().sharedMaterial = dirtMaterial;
    }

    private void CreateBoundaryMarkers()
    {
        for (int i = 0; i < 32; i++)
        {
            float angle = i / 32f * Mathf.PI * 2f;
            Vector3 position = new Vector3(Mathf.Cos(angle), 0.35f, Mathf.Sin(angle)) * mapRadius;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Map Boundary Marker";
            marker.transform.SetParent(transform);
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
            marker.GetComponent<Renderer>().sharedMaterial = rockMaterial;
        }
    }

    private void ScatterTrees()
    {
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 position = RandomPoint(7f, mapRadius - 3f);
            CreateTree(position, Range(0.8f, 1.35f));
        }
    }

    private void ScatterRocks()
    {
        for (int i = 0; i < rockCount; i++)
        {
            Vector3 position = RandomPoint(6f, mapRadius - 2f);
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Prototype Rock";
            rock.transform.SetParent(transform);
            rock.transform.position = new Vector3(position.x, 0.35f, position.z);
            float size = Range(0.6f, 1.5f);
            rock.transform.localScale = new Vector3(size * 1.3f, size * 0.65f, size);
            rock.GetComponent<Renderer>().sharedMaterial = rockMaterial;
        }
    }

    private void CreateTree(Vector3 position, float scale)
    {
        GameObject tree = new GameObject("Prototype Tree");
        tree.transform.SetParent(transform);
        tree.transform.position = position;
        tree.transform.localScale = Vector3.one * scale;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        trunk.transform.localScale = new Vector3(0.22f, 0.65f, 0.22f);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMaterial;

        GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.name = "Leaves";
        leaves.transform.SetParent(tree.transform);
        leaves.transform.localPosition = new Vector3(0f, 1.55f, 0f);
        leaves.transform.localScale = new Vector3(1.1f, 1.0f, 1.1f);
        leaves.GetComponent<Renderer>().sharedMaterial = treeMaterial;
    }

    private Vector3 RandomPoint(float innerRadius, float outerRadius)
    {
        float angle = Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Range(innerRadius * innerRadius, outerRadius * outerRadius));
        return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
    }

    private float Range(float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}
