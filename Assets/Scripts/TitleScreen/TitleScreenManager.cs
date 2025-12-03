using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager Instance;
    private bool isGameStarting = false;
    [Header("Audio")]
    public AudioClip dungeonTheme;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (MusicManager.Instance != null && dungeonTheme != null)
        {
            MusicManager.Instance.PlayMusic(dungeonTheme);
        }
    }

    void Update()
    {
        if (isGameStarting) return;
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        Debug.Log("Entering the Main Menu...");
        Time.timeScale = 1f;    //Ensure time is running before changing scenes.
        SceneManager.LoadScene("MainMenu");
    }
}

