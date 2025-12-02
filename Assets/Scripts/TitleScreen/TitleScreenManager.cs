using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager Instance;
    private void Awake()
    {
        if (Instance == null)
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
    }

    public void StartGame()
    {
        Debug.Log("Entering the Main Menu...");
        Time.timeScale = 1f;    //Ensure time is running before changing scenes.
        SceneManager.LoadScene("MainMenu");
    }
}

