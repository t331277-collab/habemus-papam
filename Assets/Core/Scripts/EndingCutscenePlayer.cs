using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class EndingCutscenePlayer : MonoBehaviour
{
    private const int DefaultRenderTextureWidth = 1920;
    private const int DefaultRenderTextureHeight = 1080;

    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage targetRawImage;

    [Header("Ending Clips")]
    [SerializeField] private VideoClip badEndingClip;
    [SerializeField] private VideoClip normalEndingClip;

    [Header("Flow")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool clearEndingResultAfterPlay = true;
    [SerializeField] private string mainMenuSceneName = "MainScene";
    [SerializeField] private Button goToMainSceneButton;
    [SerializeField] private float goToMainSceneButtonFadeDuration = 1f;

    private RenderTexture runtimeRenderTexture;
    private CanvasGroup goToMainSceneButtonCanvasGroup;
    private Coroutine buttonFadeCoroutine;

    private void Awake()
    {
        EnsureVideoPlayer();
        EnsureGoToMainSceneButton();
        HideGoToMainSceneButton();
    }

    private void OnEnable()
    {
        EnsureVideoPlayer();
        EnsureGoToMainSceneButton();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += HandleVideoFinished;
        }

        if (goToMainSceneButton != null)
        {
            goToMainSceneButton.onClick.AddListener(LoadMainMenu);
        }
    }

    private void Start()
    {
        HideGoToMainSceneButton();
        EndingResultPanelRenderer.PopulateCurrentRunStats();

        if (playOnStart)
        {
            PlaySelectedEnding();
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= HandleVideoFinished;
        }

        if (goToMainSceneButton != null)
        {
            goToMainSceneButton.onClick.RemoveListener(LoadMainMenu);
        }
    }

    private void OnDestroy()
    {
        if (runtimeRenderTexture != null)
        {
            runtimeRenderTexture.Release();
            Destroy(runtimeRenderTexture);
        }
    }

    public void PlaySelectedEnding()
    {
        EnsureVideoPlayer();

        if (videoPlayer == null)
        {
            Debug.LogWarning("[Ending] VideoPlayer is missing. Cannot play ending cutscene.");
            return;
        }

        VideoClip clip = GetClipForCurrentEnding();
        if (clip == null)
        {
            HandleMissingClip();
            return;
        }

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        ConfigureRenderTarget(clip);
        videoPlayer.isLooping = false;
        videoPlayer.Play();

        if (clearEndingResultAfterPlay)
        {
            EndingResult.Clear();
        }
    }

    private VideoClip GetClipForCurrentEnding()
    {
        switch (EndingResult.Current)
        {
            case EndingType.Bad:
                return badEndingClip;
            case EndingType.Normal:
                return normalEndingClip;
            default:
                return normalEndingClip != null ? normalEndingClip : badEndingClip;
        }
    }

    private void EnsureVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        if (targetRawImage == null && videoPlayer.renderMode == VideoRenderMode.APIOnly)
        {
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = Camera.main;
        }
        else if ((videoPlayer.renderMode == VideoRenderMode.CameraNearPlane ||
                  videoPlayer.renderMode == VideoRenderMode.CameraFarPlane) &&
                 videoPlayer.targetCamera == null)
        {
            videoPlayer.targetCamera = Camera.main;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
    }

    private void EnsureGoToMainSceneButton()
    {
        if (goToMainSceneButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (Button button in buttons)
            {
                if (button != null && button.gameObject.name == "GoToMainSceneBtn")
                {
                    goToMainSceneButton = button;
                    break;
                }
            }
        }

        if (goToMainSceneButton == null)
        {
            return;
        }

        goToMainSceneButtonCanvasGroup = goToMainSceneButton.GetComponent<CanvasGroup>();

        if (goToMainSceneButtonCanvasGroup == null)
        {
            goToMainSceneButtonCanvasGroup = goToMainSceneButton.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void ConfigureRenderTarget(VideoClip clip)
    {
        if (targetRawImage == null)
        {
            if (videoPlayer.renderMode == VideoRenderMode.CameraNearPlane && videoPlayer.targetCamera == null)
            {
                videoPlayer.targetCamera = Camera.main;
            }

            return;
        }

        if (videoPlayer.targetTexture == null)
        {
            int width = clip != null && clip.width > 0 ? (int)clip.width : DefaultRenderTextureWidth;
            int height = clip != null && clip.height > 0 ? (int)clip.height : DefaultRenderTextureHeight;
            runtimeRenderTexture = new RenderTexture(width, height, 0);
            videoPlayer.targetTexture = runtimeRenderTexture;
        }

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        targetRawImage.texture = videoPlayer.targetTexture;
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        ShowGoToMainSceneButton();
    }

    private void HandleMissingClip()
    {
        Debug.Log($"[Ending] {EndingResult.Current} ending selected. No video clip is assigned.");

        if (clearEndingResultAfterPlay)
        {
            EndingResult.Clear();
        }

        ShowGoToMainSceneButton();
    }

    private void HideGoToMainSceneButton()
    {
        if (buttonFadeCoroutine != null)
        {
            StopCoroutine(buttonFadeCoroutine);
            buttonFadeCoroutine = null;
        }

        if (goToMainSceneButton == null)
        {
            return;
        }

        goToMainSceneButton.interactable = false;
        goToMainSceneButton.gameObject.SetActive(false);

        if (goToMainSceneButtonCanvasGroup != null)
        {
            goToMainSceneButtonCanvasGroup.alpha = 0f;
            goToMainSceneButtonCanvasGroup.interactable = false;
            goToMainSceneButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    private void ShowGoToMainSceneButton()
    {
        EnsureGoToMainSceneButton();

        if (goToMainSceneButton == null)
        {
            Debug.LogWarning("[Ending] GoToMainSceneBtn was not found. Returning to MainScene immediately.");
            LoadMainMenu();
            return;
        }

        if (buttonFadeCoroutine != null)
        {
            StopCoroutine(buttonFadeCoroutine);
        }

        buttonFadeCoroutine = StartCoroutine(FadeInGoToMainSceneButton());
    }

    private IEnumerator FadeInGoToMainSceneButton()
    {
        goToMainSceneButton.gameObject.SetActive(true);
        goToMainSceneButton.interactable = false;

        if (goToMainSceneButtonCanvasGroup == null)
        {
            EnsureGoToMainSceneButton();
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0f, goToMainSceneButtonFadeDuration);

        if (goToMainSceneButtonCanvasGroup != null)
        {
            goToMainSceneButtonCanvasGroup.alpha = 0f;
            goToMainSceneButtonCanvasGroup.interactable = false;
            goToMainSceneButtonCanvasGroup.blocksRaycasts = false;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (goToMainSceneButtonCanvasGroup != null)
            {
                goToMainSceneButtonCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            }

            yield return null;
        }

        if (goToMainSceneButtonCanvasGroup != null)
        {
            goToMainSceneButtonCanvasGroup.alpha = 1f;
            goToMainSceneButtonCanvasGroup.interactable = true;
            goToMainSceneButtonCanvasGroup.blocksRaycasts = true;
        }

        goToMainSceneButton.interactable = true;
        buttonFadeCoroutine = null;
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CompleteCurrentGame();
        }

        if (ActionRecordManager.Instance != null)
        {
            ActionRecordManager.Instance.ClearCurrentRunStats();
        }

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
