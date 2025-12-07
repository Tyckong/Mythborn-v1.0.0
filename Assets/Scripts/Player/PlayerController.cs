using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Character Data")]
    public CharacterData characterData;

    #region REFERENCES
    public HealthBar healthBar;
    public ManaBar manaBar;
    public EnergyBar energyBar;
    #endregion

    #region MOVEMENT VARIABLES
    [Header("Movement")]
    public float ms;
    public Transform movePoint;
    public LayerMask stopWhen;
    public int steps;

    public int maxActionsPerTurn = 1;       //Should be a variable for different types of buffs.
    private int currentActionsTaken = 0;    //Action counters to keep track during these specific buffs.
    private int MovementBuffDuration = 0;   //Pretty self explanatory.
    private float dodgeChance = 0f;         //Useful parameter for future buffs.

    [Header("Combat")]
    public Vector3 facingDirection = Vector3.down;
    #endregion

    #region HEALTH, MANA & ENERGY
    [Header("Resources")]
    public int currentHealth;
    public int currentMana;
    public int currentEnergy;
    public bool isDead = false;
    #endregion

    [Header("Audio Settings")]
    public AudioClip hitImpactSound;
    public AudioClip hitVoiceSound;
    public AudioClip deathSound;

    void Start()
    {
        if (characterData == null)          //This is for safety measures!!
        {
            Debug.LogError("¡Falta asignar el CharacterData en el PlayerController!");
            return;
        }

        movePoint.parent = null;
        movePoint.position = transform.position;

        if (GameManager.savedHealth != -1)  //Load stats from Character sheet.
        {
            currentHealth = GameManager.savedHealth;
            currentMana = GameManager.savedMana;
            currentEnergy = GameManager.savedEnergy;
            Debug.Log("Stats loaded from previous floor.");
        }
        else
        {
            currentHealth = characterData.maxHealth;
            currentMana = characterData.maxMana;
            currentEnergy = characterData.maxEnergy;
        }
        if (healthBar) { healthBar.SetMaxHealth(characterData.maxHealth); healthBar.SetHealth(currentHealth); }
        if (manaBar) { manaBar.SetMaxMana(characterData.maxMana); manaBar.SetMana(currentMana); }
        if (energyBar) { energyBar.SetMaxEnergy(characterData.maxEnergy); energyBar.SetEnergy(currentEnergy); }

        steps = 0;
        isDead = false;
    }

    void Update()   //Smoothly move the player to the movePoint position.
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, ms * Time.deltaTime);
    }

    public void StartTurn()
    {
        currentActionsTaken = 0;                //Reset available actions.
        if (MovementBuffDuration > 0)
        {
            MovementBuffDuration--;
            maxActionsPerTurn = 2;              //Keep velocity.
            dodgeChance = 0.3f;                 //30% chance of dodging.
            Debug.Log($"Footwork Active! Turns left: {MovementBuffDuration}");
        }
        else
        {
            maxActionsPerTurn = 1; //Return to normal.
            dodgeChance = 0f;
        }
    }

    #region ABILITIES IMPLEMENTATION
    public void CastAbility1()
    {
        if (currentMana < characterData.ability1Cost) { Debug.Log("Not enough Mana!"); return; }
        CloseMenus();
        StartCoroutine(PerformAbility1Sequence());
    }

    public void CastAbility2()
    {
        if (currentMana < characterData.ability2Cost) { Debug.Log("Not enough Mana!"); return; }
        CloseMenus();
        StartCoroutine(PerformAbility2Sequence());
    }

    public void CastAbility3()
    {
        if (currentMana < characterData.ability3Cost) { Debug.Log("Not enough Mana!"); return; }
        CloseMenus();
        StartCoroutine(PerformAbility3Sequence());
    }

    //These abilities are still hardcoded for Manuel Rodriguez. I'll have to change them later on to be dynamic (change according to the current character).
    private IEnumerator PerformAbility1Sequence()
    {
        GameManager.Instance.currentState = TurnState.WAIT;         //Lock player input so they can't move during animation.
        ExpendMana(characterData.ability1Cost);                                   //Spend Mana immediately.
        GameManager.Instance.PlayAbilityAnimation(1);
        yield return new WaitForSeconds(1.8f);
        GameManager.Instance.HideAbilityAnimation();

        MovementBuffDuration = 3;   //3 turn duration for now.
        maxActionsPerTurn = 2;
        dodgeChance = 0.5f;             //50% chance of dodge

        Debug.Log(characterData.ability1Name + " Activated.");
        GameManager.Instance.currentState = TurnState.PLAYER_TURN;  //Unlock player state.
        GameManager.Instance.EndPlayerTurn();
    }

    private IEnumerator PerformAbility2Sequence()
    {
        GameManager.Instance.currentState = TurnState.WAIT;         //Lock player input so they can't move during animation.
        ExpendMana(characterData.ability2Cost);                                   //Spend Mana immediately.
        GameManager.Instance.PlayAbilityAnimation(2);
        yield return new WaitForSeconds(1.8f);
        GameManager.Instance.HideAbilityAnimation();

        //Detect enemies in range.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, characterData.ability2Range, GameManager.Instance.enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.EnemyTakeDamage(characterData.ability2Damage);
                Debug.Log(characterData.ability2Name + "Activated.");
            }
        }
        GameManager.Instance.currentState = TurnState.PLAYER_TURN;  //Unlock player state.
        GameManager.Instance.EndPlayerTurn();
    }

    private IEnumerator PerformAbility3Sequence()
    {
        GameManager.Instance.currentState = TurnState.WAIT;         //Lock player input so they can't move during animation.
        ExpendMana(characterData.ability3Cost);                     //Spend Mana immediately.
        GameManager.Instance.PlayAbilityAnimation(3);
        yield return new WaitForSeconds(1.8f);
        GameManager.Instance.HideAbilityAnimation();

        currentHealth = characterData.maxHealth;                    //Heal to MAX HP.
        GameManager.Instance.ShowFloatingText("MAX HP!", transform.position, Color.green);
        if (healthBar) healthBar.SetHealth(currentHealth);
        RegainEnergy(30);                                           //Regain EN.
        if (energyBar) energyBar.SetEnergy(currentEnergy);
        Debug.Log(characterData.ability3Name + "Activated.");
        GameManager.Instance.currentState = TurnState.PLAYER_TURN;  //Unlock player state.
        GameManager.Instance.EndPlayerTurn();
    }

    private void CloseMenus()
    {
        //Close the combat and ability menus.
        GameManager.Instance.ReturnToCombatMenu();
        GameManager.Instance.CombatMenu();
    }
    #endregion

    #region COMBAT AND RESOURCES
    void ExpendMana(int mana)
    {
        currentMana -= mana;
        manaBar.SetMana(currentMana);
    }
    void RegainMana()
    {
        if ((steps % characterData.healingMNFactor == 0) && currentMana != characterData.maxMana)
        {
            currentMana += 1;
            GameManager.Instance.ShowFloatingText("+1 MN", transform.position, Color.blue);
            manaBar.SetMana(currentMana);
        }
    }
    void RegainHealth()
    {
        if ((steps % characterData.healingHPFactor == 0) && currentHealth != characterData.maxHealth)
        {
            currentHealth += 1;
            GameManager.Instance.ShowFloatingText("+1 HP", transform.position, Color.green);
            healthBar.SetHealth(currentHealth);
        }
    }
    void ExpendEnergy(int en)
    {
        if (steps % characterData.tiringFactor == 0)
        {
            GameManager.Instance.ShowFloatingText("-" + en + " EN", transform.position, Color.white);
            if (currentEnergy - 1 <= characterData.minEnergy)
            {
                currentEnergy = characterData.minEnergy;
                energyBar.SetEnergy(currentEnergy);
                PlayerDie();
            }

            if (currentEnergy - 1 > characterData.minEnergy)
            {
                currentEnergy -= en;
                energyBar.SetEnergy(currentEnergy);
            }
        }
    }
    void RegainEnergy(int en)
    {
        GameManager.Instance.ShowFloatingText("+" + en + " EN", transform.position, Color.white);
        if (currentEnergy + en >= characterData.maxEnergy)
        {
            currentEnergy = characterData.maxEnergy;
        }
        else
        {
            currentEnergy += en;
            energyBar.SetEnergy(currentEnergy);
        }
    }
    public void Attack()
    {
        if (GameManager.Instance.currentState != TurnState.PLAYER_TURN)
        {
            return;
        }

        int damage = 2; //Damage should be adjusted with a function, but for now, we'll keep it this way.
        Vector3 attackTargetPos = transform.position + facingDirection;   //Calculate the attack target position (one cell in the last move direction).
        Collider2D hit = Physics2D.OverlapCircle(attackTargetPos, .2f, GameManager.Instance.enemyLayer);    //Check for an enemy at the target position.

        if (hit != null)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();    //If an enemy is found, tell it to take damage.
            if (enemy != null)
            {
                Debug.Log("Player attacked " + enemy.gameObject.name + " for " + damage + " damage.");
                enemy.EnemyTakeDamage(damage);
            }
        }
        else
        {
            Debug.Log("Player attacked empty space.");
        }

        GameManager.Instance.EndPlayerTurn();   //End the turn regardless of a hit or miss.
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (dodgeChance > 0)
        {
            float roll = Random.Range(0f, 1f);
            if (roll <= dodgeChance)
            {
                Debug.Log("Player dodged the attack!");
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowFloatingText("MISS", transform.position, Color.cyan);
                return;
            }
        }

        currentHealth -= damage;

        if (GameManager.Instance != null)
            GameManager.Instance.ShowFloatingText("-" + damage, transform.position, Color.red);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (healthBar != null) healthBar.SetHealth(currentHealth);
            if (GameManager.Instance != null && GameManager.Instance.sfxSource != null && deathSound != null)
            {
                GameManager.Instance.sfxSource.PlayOneShot(deathSound);
            }

            PlayerDie();
        }
        else
        {
            if (healthBar != null) healthBar.SetHealth(currentHealth);
            if (GameManager.Instance != null && GameManager.Instance.sfxSource != null)
            {
                if (hitImpactSound != null)
                    GameManager.Instance.sfxSource.PlayOneShot(hitImpactSound);

                if (hitVoiceSound != null)
                    GameManager.Instance.sfxSource.PlayOneShot(hitVoiceSound);
            }
        }
    }

    public void PlayerDie()
    {
        if (isDead) return; //Set the death flag to prevent accidental multiple death calls.
        isDead = true;

        Debug.Log("Player Died!");
        GameManager.Instance.GameOver();
    }

    #endregion

    #region MOVEMENT
    public void TryMove(Vector3 direction)
    {
        if (GameManager.Instance.currentState != TurnState.PLAYER_TURN || transform.position != movePoint.position) //If the player is mid-move, the input is ignored or processed as a tap.
        {
            return;
        }

        Vector3 targetPos = movePoint.position + direction;
        bool movedThisTurn = false;

        if (!Physics2D.OverlapCircle(targetPos, .2f, stopWhen)) //Check for obstacles at the target position.
        {
            movePoint.position = targetPos; //Set the new destination.
            movedThisTurn = true;
            facingDirection = direction.normalized; //Store direction for combat/animation.
        }

        if (movedThisTurn)  //End Turn only if a move was successful.
        {
            steps += 1;
            currentActionsTaken++;
            RegainHealth();
            RegainMana();
            ExpendEnergy(1);
            if (currentActionsTaken >= maxActionsPerTurn)
            {
                GameManager.Instance.EndPlayerTurn();
            }
            else
            {
                Debug.Log($"Action 1 used. Remaining actions: {maxActionsPerTurn - currentActionsTaken}");
            }
        }
    }

    public void ChangeFacingDirection(Vector3 direction)
    {
        if (direction.magnitude > 0.05f) // Basic check to ensure it's a valid direction
        {
            facingDirection = direction.normalized;
        }
        Debug.Log("Player Facing Direction Changed to: " + facingDirection);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        LootItem loot = collision.GetComponent<LootItem>(); //We check if we've collided with someting that has the LootItem script.

        if (loot != null)
        {
            if (InventoryManager.Instance != null)          //Add it to our inventory.
            {
                InventoryManager.Instance.AddItem(loot.itemName, loot.itemDescription);
                Debug.Log($"¡Recogiste: {loot.itemName}!");
            }
            Destroy(collision.gameObject);                  //Delete the item from the floor.
        }
    }

    #endregion
}
