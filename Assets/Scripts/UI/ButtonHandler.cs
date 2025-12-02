using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public enum ButtonType
    {
        CombatMenu,       //Open/Close Combat Menu.
        AbilityMenu,      //Ability List Menu.
        InventoryMenu,    //Placeholder.
        BackToCombat,     //Return to CombatMenu.
        Pause,
        Resume,
        Quit,
        CastAbility1,     //Los Andes Footwork.
        CastAbility2,     //Húsares.
        CastAbility3      //Patria.
    }

    public ButtonType myButtonType;
    private Button btn;

    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        if (GameManager.Instance != null)   //Functions that perform GameManager calls.
        {
            switch (myButtonType)
            {
                case ButtonType.CombatMenu:
                    GameManager.Instance.CombatMenu();
                    return;
                case ButtonType.AbilityMenu:
                    GameManager.Instance.AbilityList();
                    return;
                case ButtonType.BackToCombat:
                    GameManager.Instance.ReturnToCombatMenu();
                    return;
                case ButtonType.Pause:
                    GameManager.Instance.PauseGame();
                    return;
                case ButtonType.Resume:
                    GameManager.Instance.ResumeGame();
                    return;
                case ButtonType.Quit:
                    GameManager.Instance.QuitGame();
                    return;
                case ButtonType.InventoryMenu:
                    GameManager.Instance.ShowInventory();
                    return;
            }
        }

        if (IsAbilityButton(myButtonType))
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)             //Functions that perform Player calls.
            {
                switch (myButtonType)
                {
                    case ButtonType.CastAbility1:
                        player.CastAbility1();
                        break;
                    case ButtonType.CastAbility2:
                        player.CastAbility2();
                        break;
                    case ButtonType.CastAbility3:
                        player.CastAbility3();
                        break;
                }
            }
            else
            {
                Debug.LogWarning("ButtonHandler: No se encontró al Player para lanzar la habilidad.");
            }
        }
    }

    private bool IsAbilityButton(ButtonType type)   //Auxiliary function to check if the button is an ability.
    {
        return type == ButtonType.CastAbility1 ||
               type == ButtonType.CastAbility2 ||
               type == ButtonType.CastAbility3;
    }
}