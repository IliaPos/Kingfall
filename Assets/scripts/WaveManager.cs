using System.Collections;
using UnityEngine;

public enum GamePhase
{
    Preparation,
    Wave,
    GameOver
}

public sealed class WaveManager : MonoBehaviour
{
    [SerializeField] private Castle castle;
    [SerializeField] private RunEconomy economy;
    [SerializeField] private float mapRadius = 30f;
    [SerializeField] private float preparationSeconds = 3f;
    [SerializeField] private float spawnInterval = 0.65f;

    private Material enemyMaterial;
    private int currentWave;
    private int enemiesSpawnedThisWave;
    private int enemiesToSpawnThisWave;
    private GamePhase phase;

    public GamePhase Phase => phase;
    public int CurrentWave => currentWave;
    public int AliveEnemies => Enemy.Active.Count;
    public int Gold => economy != null ? economy.Gold : 0;

    public void Initialize(Castle targetCastle, RunEconomy runEconomy, float radius)
    {
        castle = targetCastle;
        economy = runEconomy;
        mapRadius = radius;
    }

    private void Start()
    {
        enemyMaterial = CreateMaterial("Prototype Enemy", new Color(0.35f, 0.62f, 0.32f));

        if (castle != null)
        {
            castle.Destroyed += OnCastleDestroyed;
        }

        StartCoroutine(RunWaves());
    }

    private void OnDestroy()
    {
        if (castle != null)
        {
            castle.Destroyed -= OnCastleDestroyed;
        }
    }

    private IEnumerator RunWaves()
    {
        while (phase != GamePhase.GameOver)
        {
            phase = GamePhase.Preparation;
            yield return new WaitForSeconds(preparationSeconds);

            if (phase == GamePhase.GameOver)
            {
                yield break;
            }

            currentWave++;
            enemiesSpawnedThisWave = 0;
            enemiesToSpawnThisWave = 4 + currentWave * 3;
            phase = GamePhase.Wave;

            while (enemiesSpawnedThisWave < enemiesToSpawnThisWave && phase != GamePhase.GameOver)
            {
                SpawnEnemy();
                enemiesSpawnedThisWave++;
                yield return new WaitForSeconds(spawnInterval);
            }

            while (Enemy.Active.Count > 0 && phase != GamePhase.GameOver)
            {
                yield return null;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (castle == null || castle.Health == null)
        {
            return;
        }

        float angle = Random.value * Mathf.PI * 2f;
        Vector3 spawnPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (mapRadius - 1.5f);

        GameObject enemyObject = new GameObject("Prototype Zombie");
        enemyObject.name = "Prototype Zombie";
        enemyObject.transform.position = spawnPosition;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Visual";
        visual.transform.SetParent(enemyObject.transform);
        visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        visual.transform.localScale = new Vector3(0.85f, 0.9f, 0.85f);
        visual.GetComponent<Renderer>().sharedMaterial = enemyMaterial;

        Health health = enemyObject.AddComponent<Health>();
        health.SetMaxHealth(45f + currentWave * 8f);

        Enemy enemy = enemyObject.AddComponent<Enemy>();
        enemy.SetGoldReward(10 + currentWave);
        enemy.Died += OnEnemyDied;
        EnemyMover mover = enemyObject.AddComponent<EnemyMover>();
        mover.Target = castle.AttackPoint;
        EnemyAttack attack = enemyObject.AddComponent<EnemyAttack>();
        attack.Target = castle.Health;
    }

    private void OnCastleDestroyed(Castle targetCastle)
    {
        phase = GamePhase.GameOver;
    }

    private void OnEnemyDied(Enemy enemy)
    {
        enemy.Died -= OnEnemyDied;
        if (economy != null)
        {
            economy.AddGold(enemy.GoldReward);
        }
    }

    private void OnGUI()
    {
        if (castle == null || castle.Health == null)
        {
            return;
        }

        GUI.Box(new Rect(16f, 16f, 290f, 150f), string.Empty);
        GUI.Label(new Rect(32f, 32f, 230f, 22f), $"Phase: {phase}");
        GUI.Label(new Rect(32f, 56f, 230f, 22f), $"Wave: {currentWave}");
        GUI.Label(new Rect(32f, 80f, 230f, 22f), $"Enemies: {Enemy.Active.Count}");
        GUI.Label(new Rect(32f, 104f, 230f, 22f), $"Castle HP: {castle.Health.CurrentHealth:0}/{castle.Health.MaxHealth:0}");
        GUI.Label(new Rect(32f, 128f, 260f, 22f), $"Gold: {Gold} | B: build tower (40)");

        if (phase == GamePhase.GameOver)
        {
            GUI.Label(new Rect(Screen.width * 0.5f - 80f, 90f, 220f, 40f), "GAME OVER");
        }
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        return material;
    }
}
