using UnityEngine;
using UnityEngine.EventSystems;

public class DirectionalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerController playerController;
    public Vector3 directionVector;
    public float tapDurationThreshold = 0.3f;
    public bool isClickable = true;

    private float pressStartTime;

    void Start()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressStartTime = Time.time; //Start the timer when the button is first pressed.
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (playerController == null || GameManager.Instance.currentState != TurnState.PLAYER_TURN)
        {
            return;
        }

        float pressDuration = Time.time - pressStartTime;   //Has the finger been lifted within the tap duration?
        bool isMovementComplete = playerController.transform.position == playerController.movePoint.position;   //Is the player ready for a new action?

        if (isMovementComplete && pressDuration > tapDurationThreshold) //Move action. Consumes turn.
        {
            playerController.TryMove(directionVector);
        }
        else    //Facing action. Does NOT consume turn.
        {
            playerController.ChangeFacingDirection(directionVector);
        }
    }
    public void clickeableButtonSetter(bool state)
    {
        isClickable = state;
    }

    public bool clickeableButtonGetter()
    {
        return isClickable;
    }
}