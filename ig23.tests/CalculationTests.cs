using static Lithium.Imaging.ImageCalculator;
using FluentAssertions;

namespace ig23.tests;

public partial class CalculationTests
{
    [TestCase(10, 1080, 1350, 2048, 1363, 10, 277, 1060, 795)] // landscape
    [TestCase(10, 1080, 1350, 1363, 2048, 42, 10, 997, 1330)] // portrait
    public void Test1(
        int padding, int targetWidth, int targetHeight, int originalImageWidth, int originalImageHeight,
        int expectedLeft, int expectedTop, int expectedWidth, int expectedHeight)
    {
        var result = CalculateInternalImageRectangle(padding, targetWidth, targetHeight, originalImageWidth, originalImageHeight);
        result.Should().Be((expectedLeft, expectedTop, expectedWidth, expectedHeight));
    }
}
