using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

//Singleton: design pattern that ensures a class has only ONE instance and provides a single, global point of access to it.

public struct FloatingTextRequest       //This is a simple structure to save the floating messages in a queue. It's just so floating messages don't pile and instead take turns to appear on screen.
{
    public string text;
    public Vector3 position;
    public Color color;

    public FloatingTextRequest(string t, Vector3 p, Color c)
    {
        text = t;
        position = p;
        color = c;
    }
}

public enum TurnState { PLAYER_TURN, ENEMY_TURN, WAIT, GAMEOVER, PAUSE }         //States to control the turn-based movement and actions within the Dungeon.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;                         //Singleton pattern for easy access.
    public TurnState currentState;
    public TurnState lastStateBeforePause;
    public float enemyTurnDuration = 0.1f;
    public int turnCount = 0;

    [Header("Player Spawn Settings")]
    public List<Collider2D> playerSpawnAreas;
    public LayerMask invalidSpawnLayers;

    [Header("Level Objects")]
    public GameObject stairsPrefab;

    [Header("Combat Settings")]
    public LayerMask enemyLayer;

    [Header("Level Transition")]            //-1 Will indicate that there are no saved data.
    public static int savedHealth = -1;
    public static int savedMana = -1;
    public static int savedEnergy = -1;
    public static int currentFloor = 1;

    [Header("UI")] //New header for UI reference.
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject pauseMenu;
    public GameObject combatMenu;
    public GameObject abilityList;
    public GameObject inventoryList;
    public InventoryUI inventoryUIScript;
    public GameObject combatButton;
    public GameObject attackButton;
    public GameObject movementPad;
    public GameObject pauseButton;
    public TMP_Text floorText;

    [Header("Ability UI Text")]
    public TMP_Text ability1CostText;
    public TMP_Text ability2CostText;
    public TMP_Text ability3CostText;

    [Header("Combat UI")]
    public GameObject floatingTextPrefab;
    public float textDelay = 0.4f;
    private Queue<FloatingTextRequest> textQueue = new Queue<FloatingTextRequest>();
    private bool isDisplayingText = false;

    [Header("Visual Effects")]
    public Animator abilityBannerAnimator;
    public UnityEngine.UI.Image bannerImageRenderer;
    private bool inCombatMenu = false;
    public Transform groundLayerSpawnArea; //Reference to a parent object that covers the ground area for spawning.

    [Header("Ability Sprites")]
    public Sprite ability1Sprite;
    public Sprite ability2Sprite;
    public Sprite ability3Sprite;

    [Header("Ability Audio")]
    public AudioSource sfxSource;   //Sound component that will emit the sound effect.
    public AudioClip ability1Sound;
    public AudioClip ability2Sound;
    public AudioClip ability3Sound;

    [Header("Audio")]
    public AudioClip dungeonTheme;

    private void Awake()
    {
        if (Instance == null)   //Simple Singleton implementation.
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentState = TurnState.PLAYER_TURN;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        SpawnPlayer();
        SpawnStairs();
        UpdateFloorUI();
        Debug.Log("Starting game. Current state: " + currentState);

        if (MusicManager.Instance != null && dungeonTheme != null)
        {
            MusicManager.Instance.PlayMusic(dungeonTheme);
        }

        Debug.Log("Starting game...");
    }

    public void SpawnPlayer()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && playerSpawnAreas != null && playerSpawnAreas.Count > 0)   //Check for the list to not be empty.
        {
            int randomIndex = Random.Range(0, playerSpawnAreas.Count);                  //We choose a random area from the list.
            Collider2D chosenArea = playerSpawnAreas[randomIndex];
            Vector2 randomPos = GetValidSpawnPosition(chosenArea);                      //We baton pass the specified area to an auxiliary function.
            if (randomPos != Vector2.zero)
            {
                Vector3 finalPos = new Vector3(Mathf.Round(randomPos.x), Mathf.Round(randomPos.y), 0);

                player.transform.position = finalPos;
                player.movePoint.position = finalPos;

                Debug.Log("Player spawned at: " + finalPos + " in area index: " + randomIndex);
            }
        }
        else
        {
            Debug.LogWarning("Player spawn area not defined in the inspector, or Player.");
        }
    }

    private Vector2 GetValidSpawnPosition(Collider2D area)
    {
        Vector2 spawnPos = Vector2.zero;
        bool isValid = false;
        int attempts = 0;
        int maxAttempts = 50;

        while (!isValid && attempts < maxAttempts)
        {
            Bounds bounds = area.bounds;
            float x = Random.Range(bounds.min.x + 0.5f, bounds.max.x - 0.5f);
            float y = Random.Range(bounds.min.y + 0.5f, bounds.max.y - 0.5f);
            Vector2 candidate = new Vector2(x, y);
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.4f, invalidSpawnLayers);

            if (hit == null)
            {
                spawnPos = candidate;
                isValid = true;
            }
            attempts++;
        }

        if (!isValid) Debug.LogWarning("No valid spawn position for the player found.");

        return spawnPos;
    }

    public void SpawnStairs()
    {
        if (stairsPrefab != null && playerSpawnAreas != null && playerSpawnAreas.Count > 0)
        {
            int randomIndex = Random.Range(0, playerSpawnAreas.Count);      //Set up random spawn position for the stairs.
            Collider2D area = playerSpawnAreas[randomIndex];
            Vector2 pos = GetValidSpawnPosition(area);

            if (pos != Vector2.zero)
            {
                Instantiate(stairsPrefab, new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0), Quaternion.identity); //Make sure the spawn position isn't the same as the player's
                Debug.Log("Stairs spawned.");
            }
        }
    }

    public void UpdateFloorUI()
    {
        if (floorText != null)
        {
            floorText.text = "Floor " + currentFloor;
        }
    }

    public void EndPlayerTurn()
    {
        if (currentState != TurnState.PLAYER_TURN) return;
        Debug.Log("Player turn ended. Starting enemy turn...");
        turnCount++;
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        currentState = TurnState.WAIT; //Lock the state so no other phase can start.
        Debug.Log("Turn: " + turnCount + " // Enemy Turn Started. Executing enemy actions...");
        StartCoroutine(ExecuteEnemyTurns());
    }

    public void EndEnemyTurn()
    {
        if (EnemySpawnManager.instance != null && EnemySpawnManager.instance.getEnemyCount() < 6)
        {
            EnemySpawnManager.instance.TrySpawnEnemy();
        }

        currentState = TurnState.PLAYER_TURN;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.StartTurn();
        }
        Debug.Log("Enemy turn ended. Back to player turn.");
        turnCount++;
    }

    IEnumerator ExecuteEnemyTurns()
    {
        Time.timeScale = 1f;
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);   //Find all enemies in the scene.
        foreach (EnemyController enemy in enemies)  //Wait for each enemy to take a turn, one by one.
        {
            if (enemy == null) continue;
            yield return StartCoroutine(enemy.TakeTurn()); //Yields control until the enemy's TakeTurn() coroutine COMPLETELY finishes.
            yield return new WaitForSeconds(0.1f);
        }
        EndEnemyTurn();
    }

    public void GameOver()
    {
        if (currentState == TurnState.GAMEOVER) return; //This prevents double execution.

        currentState = TurnState.GAMEOVER;
        Debug.Log("Game Over. Current state: " + currentState);

        Time.timeScale = 0f;    //Pause the game world and show the panel.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RetryGame()
    {
        Debug.Log("Retrying game...");
        Time.timeScale = 1f;

        //UI Reset logic...
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);

        //Reset the stats too...
        currentFloor = 1;
        UpdateFloorUI();
        savedHealth = -1;
        savedMana = -1;
        savedEnergy = -1;

        //Cleaning enemies off the map. (9 out of 10 GameDevs approve of this function)
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        if (EnemySpawnManager.instance != null)
        {
            EnemySpawnManager.instance.enemyCount = 0;  //Reset the enemy counter for obvious reasons...
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && player.characterData != null)
        {
            SpawnPlayer();

            player.currentHealth = player.characterData.maxHealth;
            player.currentMana = player.characterData.maxMana;
            player.healthBar.SetMaxHealth(player.characterData.maxHealth);
            player.manaBar.SetMaxMana(player.characterData.maxMana);
            player.currentEnergy = player.characterData.maxEnergy;
            if (player.energyBar) player.energyBar.SetMaxEnergy(player.characterData.maxEnergy);
            player.isDead = false;
        }

        LootItem[] oldLoots = FindObjectsByType<LootItem>(FindObjectsSortMode.None);    //Also clean off any unclaimed loots.
        foreach (LootItem loot in oldLoots)
        {
            Destroy(loot.gameObject);
        }

        //Reset the stairs as well...
        Stairs[] oldStairs = FindObjectsByType<Stairs>(FindObjectsSortMode.None);
        foreach (Stairs stairs in oldStairs) Destroy(stairs.gameObject);
        SpawnStairs();

        currentState = TurnState.PLAYER_TURN;
        Debug.Log("Game Retried. Back to player turn.");
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to Main Menu.");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game application...");
        Time.timeScale = 1f;


#if UNITY_EDITOR    // Check if the application is running in the Unity Editor
            EditorApplication.isPlaying = false;    // If in the Editor, stop playing
#else               // If in a built application, quit the application
        Application.Quit();
#endif
    }

    public void PauseGame()
    {
        if (currentState == TurnState.PAUSE) return;

        currentState = TurnState.PAUSE;
        Debug.Log("Game Paused. Current state: " + currentState);

        Time.timeScale = 0f;    //Pause the game world.
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        if (currentState != TurnState.PAUSE) return;    //Only resume if currently paused.
        Time.timeScale = 1f;                            //Unpause the game world.

        if (pauseMenu != null)                          //Hide the pause panel.
        {
            pauseMenu.SetActive(false);
        }
        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }

        currentState = TurnState.PLAYER_TURN;   //Set the state back to the appropriate play state.
        Debug.Log("Game Resumed. Current state: " + currentState);
    }

    public void CombatMenu()
    {
        if (combatMenu != null)
        {
            if (!inCombatMenu)
            {
                Time.timeScale = 0f; // Pause the game
                combatMenu.SetActive(true);
                if (movementPad) movementPad.SetActive(false);
                if (attackButton) attackButton.SetActive(false);      //Probably turn gray to represent it as "blocked" would be better.
                inCombatMenu = true;
                Debug.Log("Combat Menu Displayed");
            }
            else
            {
                Time.timeScale = 1f;
                combatMenu.SetActive(false);
                if (abilityList) abilityList.SetActive(false);
                if (inventoryList) inventoryList.SetActive(false);
                if (movementPad) movementPad.SetActive(true);
                if (attackButton) attackButton.SetActive(true);
                inCombatMenu = false;
                Debug.Log("Combat Menu Exited");
            }
        }
    }

    public void AbilityList()
    {
        if (inCombatMenu)
        {
            //Get Player reference to get the current MN costs.
            PlayerController player = FindAnyObjectByType<PlayerController>();

            if (player != null && player.characterData != null)
            {
                //Update costs if needed.
                if (ability1CostText != null)
                    ability1CostText.text = "Cost: " + player.characterData.ability1Cost + " MN";

                if (ability2CostText != null)
                    ability2CostText.text = "Cost: " + player.characterData.ability2Cost + " MN";

                if (ability3CostText != null)
                    ability3CostText.text = "Cost: " + player.characterData.ability3Cost + " MN";
            }

            combatMenu.SetActive(false);
            abilityList.SetActive(true);
            Debug.Log("Ability List Displayed with updated costs");
        }
    }

    public void ShowInventory()
    {
        if (inCombatMenu)
        {
            combatMenu.SetActive(false);
            inventoryList.SetActive(true);
            if (inventoryUIScript != null)
            {
                inventoryUIScript.UpdateInventoryDisplay();
            }

            Debug.Log("Inventory Displayed.");
        }
    }

    public void PlayAbilityAnimation(int abilityID, string triggerName = "PlayAbility")
    {
        if (abilityBannerAnimator != null && bannerImageRenderer != null)
        {
            AudioClip clipToPlay = null;
            switch (abilityID)
            {
                case 1:
                    bannerImageRenderer.sprite = ability1Sprite;
                    clipToPlay = ability1Sound;
                    break;
                case 2:
                    bannerImageRenderer.sprite = ability2Sprite;
                    clipToPlay = ability2Sound;
                    break;
                case 3:
                    bannerImageRenderer.sprite = ability3Sprite;
                    clipToPlay = ability3Sound;
                    break;
            }
            bannerImageRenderer.SetNativeSize();
            abilityBannerAnimator.gameObject.SetActive(true);
            abilityBannerAnimator.SetTrigger(triggerName);

            if (sfxSource != null && clipToPlay != null)
            {
                sfxSource.PlayOneShot(clipToPlay);
            }
        }
    }

    public void HideAbilityAnimation()
    {
        if (abilityBannerAnimator != null)
        {
            abilityBannerAnimator.gameObject.SetActive(false);
        }
    }

    public void ReturnToCombatMenu()
    {
        if (inCombatMenu)
        {
            //Not here either lmaoooo
            if (abilityList != null) abilityList.SetActive(false);
            if (inventoryList != null) inventoryList.SetActive(false);
            if (combatMenu != null) combatMenu.SetActive(true);
            Debug.Log("Back to the Combat Menu");
        }
    }

    public void GoToNextLevel()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();  //Save player stats.
        if (player != null)
        {
            savedHealth = player.currentHealth;
            savedMana = player.currentMana;
            savedEnergy = player.currentEnergy;
        }

        currentFloor++;                                                     //Go up one level.
        UpdateFloorUI();

        if (currentFloor > 10)                                              //Temporary Victory Condition. I'll change this in a future update.
        {
            Victory();
            return;
        }

        Debug.Log($"Descending to Floor {currentFloor}...");
        Time.timeScale = 1f;                                                //Reset time just in case.
        string sceneToLoad = "Dungeon Level1";                              //Until I have random generated levels, Dungeon Level1 will be for odd levels...

        if (currentFloor % 2 == 0)
        {
            sceneToLoad = "Dungeon Level2";                                 //...and Dungeon Level2 for pair levels.
        }

        SceneManager.LoadScene(sceneToLoad);                                //Recharge scene and simulate a new floor.
    }

    public void Victory()                                                   //Temporary victory panel.
    {
        Debug.Log("Dungeon Cleared!");
        currentState = TurnState.GAMEOVER;                                  //We'll use the same state to block input.
        Time.timeScale = 0f;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    public void ShowFloatingText(string text, Vector3 position, Color color)
    {
        textQueue.Enqueue(new FloatingTextRequest(text, position, color));  //Queue the "petition".
        if (!isDisplayingText)
        {
            StartCoroutine(ProcessFloatingTextQueue());
        }
    }

    IEnumerator ProcessFloatingTextQueue()  //This coroutine process the queue one by one.
    {
        isDisplayingText = true;

        while (textQueue.Count > 0)
        {
            FloatingTextRequest req = textQueue.Dequeue();  //Take the following text in the queue.

            if (floatingTextPrefab != null)
            {
                Vector3 offset = new Vector3(Random.Range(-0.1f, 0.1f), 0.5f, 0);   //Instantiate the text and add a small random offset.

                GameObject go = Instantiate(floatingTextPrefab, req.position + offset, Quaternion.identity);
                FloatingText ft = go.GetComponent<FloatingText>();
                if (ft != null)
                {
                    ft.Setup(req.text, req.color);
                }
            }
            yield return new WaitForSeconds(textDelay);     //Delay the next message.
        }
        isDisplayingText = false;
    }
}

