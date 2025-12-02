using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager instance;
    public int enemyCount = 0;

    [Header("Settings")]
    public GameObject enemyPrefab; //Enemy prefab.
    [Range(0, 100)]
    public float spawnChance = 10f; //Probability per turn.

    [Header("References")]
    public List<Collider2D> spawnAreas; //Spawn areas here.
    [SerializeField] private LayerMask _layersEnemyCannotSpawnOn; //Wall, Player, Water, etc.

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void TrySpawnEnemy()
    {
        int randomNum = Random.Range(0, 100);   //0 to 99.
        if (randomNum < spawnChance)            //If the number is lwoer than the chance, we spawn.
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (spawnAreas == null || spawnAreas.Count == 0)
        {
            Debug.LogWarning("No spawn area assigned in the inspector.");
            return;
        }
        int randomIndex = Random.Range(0, spawnAreas.Count);
        Collider2D chosenArea = spawnAreas[randomIndex];

        Vector2 spawnPosition = GetRandomSpawnPosition(chosenArea);

        if (spawnPosition != Vector2.zero)  //We verify that it isn't a Vector2.zero (fail value).
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemyCount++;
            Debug.Log("A new enemy appears!");
        }
    }

    private Vector2 GetRandomSpawnPosition(Collider2D area)
    {
        Vector2 spawnPosition = Vector2.zero;
        bool isSpawnPointValid = false;

        int attemptCount = 0;
        int maxAttempts = 20;

        while (!isSpawnPointValid && attemptCount < maxAttempts)
        {
            spawnPosition = GetRandomPointInCollider(area);         //We use OverlapCircle to check if it collides with something that it shouldn't.
            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 0.4f, _layersEnemyCannotSpawnOn);

            if (hit == null) //If we don't hit our heads with something...
            {
                isSpawnPointValid = true;
            }
            attemptCount++;
        }

        if (!isSpawnPointValid)
        {
            Debug.Log("No se encontró lugar válido para spawnear enemigo esta vez.");
            return Vector2.zero;
        }

        return new Vector3(Mathf.Round(spawnPosition.x), Mathf.Round(spawnPosition.y), 0); //Round to match visual grid, assuming integer coordinates.
    }

    private Vector2 GetRandomPointInCollider(Collider2D collider)
    {
        Bounds collBounds = collider.bounds;    //Small offset so we don't spawn right on top of the collider border.
        float randomX = Random.Range(collBounds.min.x + 0.5f, collBounds.max.x - 0.5f);
        float randomY = Random.Range(collBounds.min.y + 0.5f, collBounds.max.y - 0.5f);

        return new Vector2(randomX, randomY);
    }

    public int getEnemyCount()
    {
        return enemyCount;
    }
    public void DecreaseEnemyCount()
    {
        enemyCount--;
        if (enemyCount < 0) enemyCount = 0;
    }
}