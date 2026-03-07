using UnityEngine;

public class PrayerWaitingTrigger : MonoBehaviour
{
    [Tooltip("플레이어를 등록할 Gamsil 매니저")]
    [SerializeField] private Gamsil gamsilManager;


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
            incomingNPC = null;
        }

        return true; // 입장 가능
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StateController playerSC = other.GetComponent<StateController>();

            if (playerSC != null && playerSC.CurrentState == CardinalState.Idle)
            {
                gamsilManager.RegisterPlayerToQueue(playerSC);
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