using UnityEngine;

public class SpeechWaitingTrigger : MonoBehaviour
{
    [Tooltip("플레이어를 등록할 Lecture 매니저")]
    [SerializeField] private Lecture lectureManager;



    private bool isNpcInside = false;
    private StateController incomingNPC;

    public void SetIncomingNPC(StateController npc)
    {
        incomingNPC = npc;
    }

    public bool TryReserveSpotForPlayer()
    {
        if (isNpcInside)
        {
            return false;
        }

        if (incomingNPC != null)
        {
            incomingNPC.ChangeState(CardinalState.Idle);
            incomingNPC.CancelApproach();
            incomingNPC = null;
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StateController playerSC = other.GetComponent<StateController>();
            PlayerController playerCtrl = other.GetComponent<PlayerController>();

            if (playerSC != null && playerSC.CurrentState == CardinalState.Idle)
            {
                if (playerCtrl != null)
                {
                    lectureManager.RegisterPlayerToQueue(playerSC);
                }
            }
            isNpcInside = true;
        }

        if (other.CompareTag("NPC"))
        {
            isNpcInside = true;
            StateController arrivedNPC = other.GetComponent<StateController>();
            if (incomingNPC == arrivedNPC)
            {
                incomingNPC = null;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            isNpcInside = false;
            StateController exitingNPC = other.GetComponent<StateController>();
            if (incomingNPC == exitingNPC)
            {
                incomingNPC = null;
            }
        }
    }
}