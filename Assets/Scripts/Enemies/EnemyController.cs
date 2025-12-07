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
    private bool isDying = false;
    #endregion

    private GameManager gm;
    public float enemyMoveSpeed = 5f;

    //We hide these from the inspector to prevent confusion.
    [HideInInspector] public Transform target; //The Player's transform.
    [HideInInspector] public PlayerController playerRef;

    public Transform movePoint; //The enemy's movePoint, usually on a separate object.
    public LayerMask stopWhen; //For checking obstacles.

    [Header("AI Settings")]
    public int aggroRange = 8; //How many cells of vision the enemy has.
    [Range(0, 100)] public float wanderChance = 50f;    //Chance of moving if its wandering.

    [Header("Loot Settings")]
    public GameObject lootPrefab;
    [Range(0, 100)] public float dropChance = 30f; //30% chance of dropping loot.

    [Header("Animation")]
    public Animator animator; //Reference to the Animator component.
    public SpriteRenderer spriteRenderer;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    void Start()
    {
        currentHealth = enemyMaxHealth;
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

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
        if (isDying)
        {
            yield break;
        }
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

        if (distanceToPlayer <= 1.1f) //If distance is roughly 1 (allowing for tiny float variance because I'm stoopid), we are adjacent. 
        {
            Debug.Log(gameObject.name + " ATTACKS!");
            yield return StartCoroutine(AttackPlayer()); //We use StartCoroutine inside the yield return to wait for the animation.
        }
        else if (distanceToPlayer <= aggroRange)
        {
            Debug.Log(gameObject.name + " CHASES!");
            yield return StartCoroutine(MoveTowardsPlayer(enemyPos, targetPos));
        }
        else
        {
            Debug.Log(gameObject.name + " WANDERS.");
            yield return StartCoroutine(WanderMovement());
        }

        Debug.Log(gameObject.name + " finished turn.");
        yield return null;
    }   //Coffee mug counter: 22

    IEnumerator MoveTowardsPlayer(Vector3 currentPos, Vector3 targetPos)
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
            yield return StartCoroutine(MoveEnemy(stepDirection));      //Call the physical movement coroutine.
        }
    }

    IEnumerator WanderMovement()
    {
        float roll = Random.Range(0f, 100f);    //Random chance to move or stay idle.
        if (roll > wanderChance) yield break;   //Stay idle.

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
                yield return StartCoroutine(MoveEnemy(dir));
                yield break; //Exit after moving once.
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
        if (isDying) return;

        currentHealth -= damage;

        if (GameManager.Instance != null && GameManager.Instance.sfxSource != null)
        {
            if (currentHealth <= enemyMinHealth && deathSound != null)
            {
                GameManager.Instance.sfxSource.PlayOneShot(deathSound);
            }
            else if (hitSound != null)
            {
                GameManager.Instance.sfxSource.PlayOneShot(hitSound);
            }
        }

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
            StartCoroutine(DieSequence());
        }
        else
        {
            if (animator != null) animator.SetTrigger("Damage");
        }
    }

    private IEnumerator DieSequence()
    {
        isDying = true;

        if (animator != null) animator.SetBool("isDead", true);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        yield return new WaitForSeconds(1f);

        EnemyDie();
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
    public IEnumerator MoveEnemy(Vector3 direction)
    {
        if (animator != null) animator.SetBool("isMoving", true);
        if (direction.x < 0)
        {
            spriteRenderer.flipX = true; //Turn around.
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = false; //Turn around again (normal lol)
        }

        float moveTime = 0.3f;
        yield return new WaitForSeconds(moveTime);
        transform.position += direction;

        if (animator != null) animator.SetBool("isMoving", false);
    }

    public IEnumerator AttackPlayer()   //Now triggers animation and waits before dealing damage.
    {
        if (animator != null) animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.6f);

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(10);
            Debug.Log("Enemigo atacó e hizo daño");
        }

        yield return new WaitForSeconds(0.3f);  //Small pause after attack.
    }
}