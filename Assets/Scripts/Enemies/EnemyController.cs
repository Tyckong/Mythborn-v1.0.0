using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    #region HEALTH & MANA
    [Header("Resources")]
    public int enemyMinHealth = 0;
    public int enemyMaxHealth = 4; //Using the latest max health value.
    public int currentHealth;
    public EnemyHealthBar enemyHealthBar;
    #endregion

    private GameManager gm;
    public float enemyMoveSpeed = 5f;

    // We hide these from the inspector to prevent confusion.
    [HideInInspector] public Transform target; //The Player's transform.
    [HideInInspector] public PlayerController playerRef;

    public Transform movePoint; //The enemy's movePoint, usually on a separate object.
    public LayerMask stopWhen; //For checking obstacles.

    [Header("AI Settings")]
    public int aggroRange = 5; //How many cells of vision the enemy has.
    [Range(0, 100)] public float wanderChance = 50f;    //Chance of moving if its wandering.

    [Header("Loot Settings")]
    public GameObject lootPrefab;
    [Range(0, 100)] public float dropChance = 30f; // 30% chance of dropping loot.

    void Start()
    {
        currentHealth = enemyMaxHealth;
        if (enemyHealthBar != null) //Check null before using enemyHealthBar, just to be sure :P
        {
            enemyHealthBar.SetEnemyMaxHealth(enemyMaxHealth);
        }

        gm = GameManager.Instance;
        FindPlayer(); //Find the Player. And my hope for the future for I've lost it.
    }
    void FindPlayer()
    {
        if (playerRef == null)
        {
            playerRef = FindAnyObjectByType<PlayerController>();
            if (playerRef != null)
            {
                target = playerRef.transform;
            }
        }
    }

    void Update() { }

    public IEnumerator TakeTurn()   //The amount of hate I feel towards this function is honestly unmatched. This is horrible and made me rethink my life decisions.
    {
        Debug.Log(gameObject.name + " is thinking...");

        if (target == null || playerRef == null) FindPlayer();  //Safety checks never hurt nobody right?

        if (target == null)                                     //...right?
        {
            yield break;                                        //Stop the function here to prevent the crashes.
        }

        //Round positions to ensure we are calculating distance on the grid, not floats.
        Vector3 enemyPos = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
        Vector3 targetPos = new Vector3(Mathf.Round(target.position.x), Mathf.Round(target.position.y), 0);

        //Calculate Manhattan Distance
        float distanceToPlayer = Mathf.Abs(enemyPos.x - targetPos.x) + Mathf.Abs(enemyPos.y - targetPos.y);
        Vector3 stepDirection = Vector3.zero;

        if (distanceToPlayer <= 1.1f) // If distance is roughly 1 (allowing for tiny float variance because I'm stoopid), we are adjacent. 
        {
            Debug.Log(gameObject.name + " ATTACKS!");
            if (playerRef != null)
            {
                int damage = 10;
                playerRef.TakeDamage(damage);
            }
        }
        else if (distanceToPlayer <= aggroRange)
        {
            Debug.Log(gameObject.name + " CHASES!");
            MoveTowardsPlayer(enemyPos, targetPos);
        }
        else
        {
            Debug.Log(gameObject.name + " WANDERS.");
            WanderMovement();
        }

        Debug.Log(gameObject.name + " finished turn.");
        yield return null;
    }   //Coffee mug counter: 21

    void MoveTowardsPlayer(Vector3 currentPos, Vector3 targetPos)
    {
        Vector3 direction = targetPos - currentPos;
        Vector3 stepDirection = Vector3.zero;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            stepDirection.x = Mathf.Sign(direction.x);
        }
        else
        {
            stepDirection.y = Mathf.Sign(direction.y);
        }

        Vector3 newMovePosition = transform.position + stepDirection;

        if (!Physics2D.OverlapCircle(newMovePosition, .2f, stopWhen))   //Verify for collision.
        {
            transform.position = newMovePosition;
        }
    }

    void WanderMovement()
    {
        float roll = Random.Range(0f, 100f);    //Random chance to move or stay idle.
        if (roll > wanderChance) return;        //Stay idle.

        List<Vector3> possibleMoves = new List<Vector3>
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right
        };

        Shuffle(possibleMoves);                 //Shuffle to get a random directions
        foreach (Vector3 dir in possibleMoves)
        {
            Vector3 attemptPos = transform.position + dir;
            if (!Physics2D.OverlapCircle(attemptPos, 0.2f, stopWhen))   //If the cell is empty, we move there and end turn.
            {
                transform.position = attemptPos;
                return;
            }
        }
    }

    void Shuffle<T>(List<T> list)           //Shuffle list function.
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void EnemyTakeDamage(int damage)
    {
        currentHealth -= damage;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowFloatingText(damage.ToString(), transform.position, Color.red);
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetEnemyHealth(currentHealth);
        }

        if (currentHealth <= enemyMinHealth)
        {
            currentHealth = enemyMinHealth;
            if (enemyHealthBar != null)
            {
                enemyHealthBar.SetEnemyHealth(currentHealth); //Update the bar one last time to show 0.
            }
            EnemyDie();
        }
    }

    public void EnemyDie()
    {
        Debug.Log(gameObject.name + " died.");
        if (EnemySpawnManager.instance != null)
        {
            EnemySpawnManager.instance.DecreaseEnemyCount();
        }

        LootDrop();
        Destroy(transform.root.gameObject); //ENEMY BEGONE!!!
    }

    private void LootDrop()
    {
        if (lootPrefab == null)
        {
            Debug.LogError("Critical Error: " + gameObject.name + " tried to drop loot, but 'Loot Prefab' is NULL.");
            return;
        }

        float roll = Random.Range(0f, 100f);
        if (roll <= dropChance)
        {
            GameObject newLoot = Instantiate(lootPrefab, transform.position, Quaternion.identity);
            LootItem itemData = newLoot.GetComponent<LootItem>();

            if (itemData != null)
            {
                float itemRoll = Random.Range(0f, 1f);
                if (itemRoll < 0.33f)
                    itemData.SetItemData("Health Potion", "Restores 20 HP.");
                if (itemRoll > 0.34f && itemRoll < 0.66f)
                    itemData.SetItemData("Mana Potion", "Restores 20 MN.");
                else
                    itemData.SetItemData("Quantum Apple", "Restores 20 EN,");
            }
            else
            {
                Debug.LogWarning("The loot was created, but doesn't have the 'LootItem' script.");
            }
        }
        else
        {
            Debug.Log("No loot :()");
        }
    }
}