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
    [SerializeField] private float fallbackReturnDelay = 10f;

    private RenderTexture runtimeRenderTexture;
    private Coroutine fallbackReturnCoroutine;

    private void Awake()
    {
        EnsureVideoPlayer();
    }

    private void OnEnable()
    {
        EnsureVideoPlayer();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += HandleVideoFinished;
        }
    }

    private void Start()
    {
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
        LoadMainMenu();
    }

    private void HandleMissingClip()
    {
        Debug.Log($"[Ending] {EndingResult.Current} ending selected. No video clip is assigned.");

        if (clearEndingResultAfterPlay)
        {
            EndingResult.Clear();
        }

        if (fallbackReturnCoroutine != null)
        {
            StopCoroutine(fallbackReturnCoroutine);
        }

        fallbackReturnCoroutine = StartCoroutine(ReturnToMainMenuAfterDelay());
    }

    private IEnumerator ReturnToMainMenuAfterDelay()
    {
        float elapsed = 0f;
        float delay = Mathf.Max(0f, fallbackReturnDelay);

        while (elapsed < delay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        fallbackReturnCoroutine = null;
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteCompletedGameSave();
        }

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
