using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public sealed class NewsBookInitializationTrigger : MonoBehaviour
{
    [SerializeField] private NewsBookInitializer initializer;
    [SerializeField] private bool invokeOnStart;
    [SerializeField] private VideoPlayer cutsceneAnim;
    [SerializeField] private GameObject cutscene;

    [Header("Newspaper cover picture")]
    [SerializeField] private Image newsPicture;
    [SerializeField] private Book book;
    [SerializeField, Min(0f)] private float newsPictureTime = 1.5f;
    [SerializeField, Min(0f)] private float newsPictureReturnTime = 0.6f;

    public bool HasFinished { get; private set; }
    public event Action OnCutsceneFinished;

    private RenderTexture videoTexture;
    private RawImage videoImage;
    private bool isFinishingCutscene;
    private Coroutine newsPictureCoroutine;
    private GameObject newsPictureOverlay;
    private Image displayedNewsPicture;
    private bool wasNewsPictureEnabled;
    private readonly Vector3[] pictureCorners = new Vector3[4];

    private void Awake()
    {
        if (cutsceneAnim == null)
            return;

        cutsceneAnim.playOnAwake = false;
        cutsceneAnim.isLooping = false;
        cutsceneAnim.prepareCompleted += OnVideoPrepared;
        cutsceneAnim.loopPointReached += OnVideoFinished;
        cutsceneAnim.errorReceived += OnVideoError;
    }
    private void Start()
    {
        SoundManager.Instance?.StopBGM();
        if (cutsceneAnim == null || cutscene == null ||
            (cutsceneAnim.source == VideoSource.VideoClip && cutsceneAnim.clip == null))
        {
            Debug.LogWarning("Intro cutscene video is not assigned. Opening the newspaper.", this);
            FinishCutscene();
            return;
        }

        cutscene.SetActive(true);
        CreateVideoOverlay();
        cutsceneAnim.Prepare();
    }

    private void CreateVideoOverlay()
    {
        // Camera output is covered by the newspaper's Screen Space Overlay canvases.
        // Display the video on its own overlay canvas above the newspaper instead.
        GameObject overlay = new GameObject("IntroCutsceneOverlay", typeof(RectTransform),
            typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        overlay.transform.SetParent(cutscene.transform, false);
        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        overlay.GetComponent<Image>().color = Color.black;

        GameObject imageObject = new GameObject("Video", typeof(RectTransform),
            typeof(RawImage), typeof(AspectRatioFitter));
        imageObject.transform.SetParent(overlay.transform, false);
        videoImage = imageObject.GetComponent<RawImage>();
        videoImage.color = Color.black;
        AspectRatioFitter fitter = imageObject.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 16f / 9f;

        videoTexture = new RenderTexture(1920, 1080, 0);
        videoTexture.Create();
        videoImage.texture = videoTexture;
        cutsceneAnim.renderMode = VideoRenderMode.RenderTexture;
        cutsceneAnim.targetTexture = videoTexture;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (HasFinished || isFinishingCutscene)
            return;

        if (source.width > 0 && source.height > 0)
            videoImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)source.width / source.height;
        videoImage.color = Color.white;
        SoundManager.Instance?.PlaySFX("CutScene");
        source.Play();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning("Intro cutscene playback failed: " + message, this);
        FinishCutscene();
    }

    private void OnDestroy()
    {
        ClearNewsPictureOverlay();
        if (cutsceneAnim != null)
        {
            cutsceneAnim.prepareCompleted -= OnVideoPrepared;
            cutsceneAnim.loopPointReached -= OnVideoFinished;
            cutsceneAnim.errorReceived -= OnVideoError;
            cutsceneAnim.targetTexture = null;
        }

        if (videoTexture != null)
        {
            videoTexture.Release();
            Destroy(videoTexture);
        }
    }

    public void Trigger()
    {
        if (initializer == null)
        {
            Debug.LogError("News book initializer is not assigned.", this);
            return;
        }
        SoundManager.Instance?.PlayBGM("newspaper");
        initializer.InitializeNewGameNews();
    }

    private void Reset()
    {
        initializer = GetComponent<NewsBookInitializer>();
    }
    private void OnVideoFinished(VideoPlayer src)
    {
        FinishCutscene();
    }

    private void FinishCutscene()
    {
        if (HasFinished || isFinishingCutscene)
            return;

        isFinishingCutscene = true;
        if (cutsceneAnim != null)
            cutsceneAnim.Stop();
        if (cutscene != null)
            cutscene.SetActive(false);
        if (invokeOnStart)
            Trigger();

        if (newsPicture == null || newsPicture.sprite == null || book == null || newsPictureTime <= 0f)
        {
            CompleteIntro();
            return;
        }

        newsPictureCoroutine = StartCoroutine(ShowNewsPicture());
    }

    private IEnumerator ShowNewsPicture()
    {
        newsPictureOverlay = new GameObject("NewsPictureOverlay", typeof(RectTransform),
            typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        Canvas overlayCanvas = newsPictureOverlay.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 1000;
        // This full-screen graphic also blocks clicks during the return animation.
        newsPictureOverlay.GetComponent<Image>().color = Color.clear;

        GameObject pictureObject = new GameObject("CoverPicture", typeof(RectTransform), typeof(Image));
        pictureObject.transform.SetParent(newsPictureOverlay.transform, false);
        Image picture = pictureObject.GetComponent<Image>();
        picture.sprite = newsPicture.sprite;
        picture.color = newsPicture.color;
        picture.material = newsPicture.material;
        RectTransform pictureRect = picture.rectTransform;
        pictureRect.anchorMin = pictureRect.anchorMax = new Vector2(0.5f, 0.5f);
        pictureRect.pivot = new Vector2(0.5f, 0.5f);
        RectTransform overlayRect = (RectTransform)newsPictureOverlay.transform;
        Canvas.ForceUpdateCanvases();
        pictureRect.sizeDelta = overlayRect.rect.size;

        // Book.Start creates the visible page from a template. Wait for that clone
        // before finding the destination; the template itself lives off screen.
        yield return null;
        if (book.RightNext != null)
        {
            foreach (Image candidate in book.RightNext.GetComponentsInChildren<Image>(true))
            {
                if (candidate.sprite == newsPicture.sprite && candidate.name == newsPicture.name)
                {
                    displayedNewsPicture = candidate;
                    break;
                }
            }
        }

        if (displayedNewsPicture == null)
        {
            Debug.LogWarning("Visible newspaper cover picture was not found.", this);
            ClearNewsPictureOverlay();
            CompleteIntro();
            yield break;
        }

        wasNewsPictureEnabled = displayedNewsPicture.enabled;
        displayedNewsPicture.enabled = false;
        float elapsed = 0f;
        while (elapsed < newsPictureTime)
        {
            pictureRect.sizeDelta = overlayRect.rect.size;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < newsPictureReturnTime && displayedNewsPicture != null)
        {
            Canvas.ForceUpdateCanvases();
            Rect destination = GetPictureRectInOverlay(overlayRect);
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / newsPictureReturnTime);
            pictureRect.anchoredPosition = Vector2.Lerp(Vector2.zero, destination.center, progress);
            pictureRect.sizeDelta = Vector2.Lerp(overlayRect.rect.size, destination.size, progress);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ClearNewsPictureOverlay();
        newsPictureCoroutine = null;
        CompleteIntro();
    }

    private Rect GetPictureRectInOverlay(RectTransform overlayRect)
    {
        RectTransform destination = displayedNewsPicture.rectTransform;
        destination.GetWorldCorners(pictureCorners);
        Canvas sourceCanvas = displayedNewsPicture.canvas.rootCanvas;
        Camera camera = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sourceCanvas.worldCamera;
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in pictureCorners)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, corner);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, screenPoint, null, out Vector2 point);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void ClearNewsPictureOverlay()
    {
        if (displayedNewsPicture != null)
            displayedNewsPicture.enabled = wasNewsPictureEnabled;
        displayedNewsPicture = null;
        if (newsPictureOverlay != null)
        {
            newsPictureOverlay.SetActive(false);
            Destroy(newsPictureOverlay);
            newsPictureOverlay = null;
        }
    }

    private void OnDisable()
    {
        if (newsPictureCoroutine == null)
            return;

        StopCoroutine(newsPictureCoroutine);
        newsPictureCoroutine = null;
        ClearNewsPictureOverlay();
        CompleteIntro();
    }

    private void CompleteIntro()
    {
        if (HasFinished)
            return;

        HasFinished = true;
        OnCutsceneFinished?.Invoke();
    }
}
