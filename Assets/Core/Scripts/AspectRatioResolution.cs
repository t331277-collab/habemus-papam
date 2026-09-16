using System;

internal static class AspectRatioResolution
{
    public static bool TryFit(
        int availableWidth,
        int availableHeight,
        int aspectWidth,
        int aspectHeight,
        out int width,
        out int height)
    {
        width = 0;
        height = 0;

        if (availableWidth <= 0 || availableHeight <= 0 || aspectWidth <= 0 || aspectHeight <= 0)
        {
            return false;
        }

        // Whole multiples keep the aspect ratio exact without exceeding the display bounds.
        int scale = Math.Min(availableWidth / aspectWidth, availableHeight / aspectHeight);
        if (scale == 0)
        {
            return false;
        }

        width = aspectWidth * scale;
        height = aspectHeight * scale;
        return true;
    }
}
