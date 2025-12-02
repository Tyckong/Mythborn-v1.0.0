using UnityEngine;

public class Stairs : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)     //We check if it's the player who's stepping on the stairs. NO ENEMIES SHALL PASS!!!
        {
            Debug.Log("Player found the stairs!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToNextLevel();
            }
        }
    }
}