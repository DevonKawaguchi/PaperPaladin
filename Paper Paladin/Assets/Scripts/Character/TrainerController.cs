using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] new string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialogue dialogue;
    [SerializeField] Dialogue dialogueAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    //State
    bool battleLost = false;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator) //Shows dialogue if Player interacts with Trainer from another angle that's not in the Trainer's FOV
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            yield return DialogueManager.Instance.ShowDialogue(dialogue);
            GameController.Instance.StartTrainerBattle(this);
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogue(dialogueAfterBattle);
        }
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        //Show exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //Makes Trainer walk towards the Player
        var diff = player.transform.position - transform.position; //Difference vector between Player and Trainer's position
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //Show Trainer dialogue
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
        GameController.Instance.StartTrainerBattle(this);
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false); //Disables Trainer FOV 
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirection.Up)
        {
            angle = 180;
        }
        else if (dir == FacingDirection.Left)
        {
            angle = 270;
        }
        //Down else if not required as Down is enabled by default

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle); //"eulerAngles" instead of "rotation" as eulerAngles sets fov rotation as a vector, while .rotation sets rotatuion as a quaternion
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost) //If Trainer lost battle, disable Trainer FOV when loading
        {
            fov.gameObject.SetActive(false); //Disables Trainer FOV 
        }
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
