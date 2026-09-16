using UnityEngine;

internal static class StandaloneDisplayBootstrap
{
    private const int AspectWidth = 16;
    private const int AspectHeight = 9;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Resolution displayResolution = Screen.currentResolution;
        if (!AspectRatioResolution.TryFit(
                displayResolution.width,
                displayResolution.height,
                AspectWidth,
                AspectHeight,
                out int width,
                out int height))
        {
            Debug.LogWarning("Cannot apply the 16:9 display resolution: invalid display dimensions.");
            return;
        }

        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
    }
#endif
}
