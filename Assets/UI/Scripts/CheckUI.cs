using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//여기에 교황 판정 화면을 구현
public class CheckUI : MonoBehaviour
{
    private Image img;
    [SerializeField] private Button SkipButton;
    [SerializeField] private Button Vote;
    [SerializeField] private Sprite[] sprites; //코루틴 돌릴 스프라이트.
    [SerializeField] private Animator anim;
    private string enterAnim = "Enter";
    private string electAnim = "Elect";
    private int currentSprite = 0;
    private bool isClicked = false;
    
    private enum AnimState
    {
        None = 0,
        Enter,
        ElectWait,
        Elect,
        ElectEnd
    }
    [SerializeField] private AnimState animState;
    [SerializeField] private string ElectionMessage = "화면을 눌러 투표 결과를 확인하세요.";
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI probabilityText;
    private float jackpotDuration;
    private Coroutine jackpotCoroutine;
    private float winProbability = 0; //ElectionManager에서 정해진 승리 확률.
    private int winner = -1;
    public void SetWinner(int i) {winner = i;}
    public void SetProbability(float f) {winProbability = f;}
    void Start()
    {
        img = GetComponent<Image>();
        if(SkipButton != null) SkipButton.onClick.AddListener(Skip);
        if(Vote != null)
        {
            Vote.onClick.AddListener(OnVote);
        }
        animState = 0;
        SetSprite(0);
        isClicked = false;
        text.text = "";
        probabilityText.text = "";
        anim.Play("Enter", 0, 0f);
    }
    public void SetSprite(int i, int offset = 0)
    {
        if(currentSprite == i) return;
        currentSprite = i;
        img.sprite = sprites[(i+offset)%4];
    }
    private void Skip() //스킵 버튼을 누르면 스킵.
    {
        if(jackpotCoroutine != null) StopCoroutine(jackpotCoroutine);
        if(animState == AnimState.None)
        {
            Debug.Log("투표 화면 오류!");
        }
        if(animState == AnimState.Enter || animState == AnimState.ElectWait || animState == AnimState.Elect)
        {
            SetSprite(4, winner);
            text.text = ElectionMessage;
            animState = AnimState.ElectEnd;
            //스킵 버튼을 다시 누르면 다음 화면으로 넘어감.
        }
    }
    private void OnVote()
    {
        Vote.gameObject.SetActive(false);
        if(animState==AnimState.ElectWait)
        {
            anim.Play("Elect", 0, 0f);
        }
        jackpotCoroutine = StartCoroutine(JackpotRoutine(winProbability));
    }
    private void OnEnable()
    {
        animState = AnimState.Enter;
        text.text = "";
        probabilityText.text = "";
        anim.Play("Enter", 0, 0f);
        Vote.gameObject.SetActive(false);
    }
    private void OnEnterAnimFinished()
    {
        text.text = ElectionMessage;
        animState = AnimState.ElectWait;
        Vote.gameObject.SetActive(true);
    }
    private void OnElectionFinished()
    {
        Vote.gameObject.SetActive(false);
        animState = AnimState.ElectEnd;
        SetSprite(4, winner);
        if (jackpotCoroutine != null) StopCoroutine(jackpotCoroutine);
        jackpotCoroutine = StartCoroutine(JackpotRoutine(winProbability));
    }
    private IEnumerator JackpotRoutine(float finalProb)
    {
        float elapsed = 0f;

        while (elapsed < jackpotDuration)
        {
            elapsed += Time.deltaTime;

            float randomTick = Random.Range(0f, 100f);

            if (probabilityText != null)
            {
                probabilityText.text = $" <color=black>{randomTick:F1}%</color>";
            }

            yield return null;
        }

        if (probabilityText != null)
        {
            probabilityText.text = $" <color=black>{finalProb:F1}%</color>";
        }

        jackpotCoroutine = null;
    }
}