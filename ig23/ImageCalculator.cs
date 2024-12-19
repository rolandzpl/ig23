namespace Lithium.Imaging;

public static class ImageCalculator
{
    public static (int left, int top, int width, int height) CalculateInternalImageRectangle(int padding,
        int targetWidth, int targetHeight, int originalImageWidth, int originalImageHeight)
    {
        return CalculateInternalImageRectangle(padding, targetWidth, targetHeight, originalImageWidth,
            originalImageHeight, Aspect.Ratio43);
    }

    public static (int left, int top, int width, int height) CalculateInternalImageRectangle(
        int padding, int targetWidth, int targetHeight, int originalImageWidth, int originalImageHeight,
        Func<int, int> applyAspectRatio)
    {
        var isLandscape = originalImageHeight < originalImageWidth;
        if (isLandscape)
        {
            var left = padding;
            var width = targetWidth - 2 * padding;
            var height = applyAspectRatio(width);
            var top = (targetHeight - height) / 2;
            return (left, top, width, height);
        }
        else
        {
            var top = padding;
            var height = targetHeight - 2 * padding;
            var width = applyAspectRatio(height);
            var left = (targetWidth / 2) - (width / 2);
            return (left, top, width, height);
        }
    }
}
