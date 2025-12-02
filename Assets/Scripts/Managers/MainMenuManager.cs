using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;
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

    public void EnterTheDungeon()
    {
        Debug.Log("Entering the Dungeon...");
        Time.timeScale = 1f;    //Ensure time is running before changing scenes
        SceneManager.LoadScene("Dungeon");
    }
    public void Gacha()
    {
        Debug.Log("I know what kind of man you are...");
        Time.timeScale = 1f;
    }
    public void Shop()
    {
        Debug.Log("Lamp Oil, Rope, Bombs? You want it? It's yours, my friend. As long as you have enough rupees. \nSorry, Player. I can't give credit! Come back when you're a little... MMMMMMM... Richer!");
        Time.timeScale = 1f;
    }
    public void Options()
    {
        Debug.Log("WIP...");
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

}

