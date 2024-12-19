using System.CommandLine;
using Lithium.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using static Lithium.Imaging.ImageCalculator;

var fileOption = new Option<FileInfo?>(
    name: "--file",
    description: "Image to convert from 2:3 to 4:5");
fileOption.AddAlias("-f");

var fileListOption = new Option<FileInfo?>(
    name: "--file-list",
    description: "File with a list of images to convert from 2:3 to 4:5");
fileListOption.AddAlias("-fl");

var imgQualityOption = new Option<int?>(
    name: "--quality",
    description: "Image quality");
imgQualityOption.SetDefaultValue(80);
imgQualityOption.AddAlias("-q");

var lightnessOption = new Option<float>(
    name: "--lightness",
    description: "Background lightness");
lightnessOption.SetDefaultValue(0f);
lightnessOption.AddAlias("-l");

var targetWidthOption = new Option<int>(
    name: "--width",
    description: "Target image width");
targetWidthOption.AddAlias("-w");
targetWidthOption.SetDefaultValue(1080);

var targetHeightOption = new Option<int>(
    name: "--height",
    description: "Target image height");
targetHeightOption.AddAlias("-h");
targetHeightOption.SetDefaultValue(1350);

var convertFileCommand = new Command("file", "Convert image")
{
    fileOption,
    lightnessOption,
    imgQualityOption
};
convertFileCommand.SetHandler(ConvertImage, fileOption, lightnessOption, imgQualityOption);

var paddingOption = new Option<int>(
    name: "--padding",
    description: "Padding");
paddingOption.AddAlias("-p");
paddingOption.SetDefaultValue(30);

var fileNamePostfixOption = new Option<string>(
    name: "--filename-posfix",
    description: "Output filename postfix");
fileNamePostfixOption.AddAlias("-P");
fileNamePostfixOption.SetDefaultValue("_IG");

var borderWidthOption = new Option<int>(
    name: "--border",
    description: "Border width. Set 0 to remove border.");
borderWidthOption.AddAlias("-b");
borderWidthOption.SetDefaultValue(10);

var convertAltFileCommand = new Command("alt", "Convert image (alternative)")
{
    fileOption,
    paddingOption,
    targetWidthOption,
    targetHeightOption,
    lightnessOption,
    imgQualityOption,
    fileNamePostfixOption,
    borderWidthOption
};
convertAltFileCommand.SetHandler(
    ConvertImage1, fileOption, paddingOption, targetWidthOption, targetHeightOption, lightnessOption, imgQualityOption,
    fileNamePostfixOption, borderWidthOption);

var convertFilesFromListCommand = new Command("list", "Convert image")
{
    fileListOption,
    lightnessOption,
    imgQualityOption
};
convertFilesFromListCommand.SetHandler(ConvertImagesFromList, fileListOption, lightnessOption, imgQualityOption);

var rootCommand = new RootCommand("Convert image")
{
    convertFileCommand,
    convertFilesFromListCommand,
    convertAltFileCommand
};

await rootCommand.InvokeAsync(args);

void ConvertImagesFromList(FileInfo fileList, float lightness, int? imgQual)
{
    using var fileListReader = fileList.OpenText();
    string line = null;
    var files = new List<FileInfo>();
    while ((line = fileListReader.ReadLine()) != null)
    {
        if (string.IsNullOrEmpty(line))
        {
            continue;
        }
        files.Add(new FileInfo(
            System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(fileList.FullName),
                line)));
    }
    foreach (var f in files)
    {
        ConvertImage(f, lightness, imgQual);
    }
}

void ConvertImage(FileInfo file, float lightness, int? imgQual)
{
    Console.WriteLine($"Converting {System.IO.Path.GetFileName(file.FullName)}");
    try
    {
        using var stream = file.OpenRead();
        var image = Image.Load(stream);
        var bounds45 = GetRectangle45(image.Bounds());
        using var clone = image.Clone(ctx => ctx
            .Lightness(lightness)
            .GaussianBlur(80)
            .Resize(bounds45.Width, bounds45.Height)
            .DrawImage(image,
                image.Bounds().IsVertical()
                ? new Point(bounds45.Width * 5 / 4 / 2 - image.Width / 2, 0)
                : new Point(0, bounds45.Height / 2 - image.Height / 2),
                1f));
        var outputPath = System.IO.Path.Combine(
             System.IO.Path.GetDirectoryName(file.FullName),
             $"{System.IO.Path.GetFileNameWithoutExtension(file.FullName)}_4_3.{System.IO.Path.GetExtension(file.FullName)}");
        clone.SaveAsJpeg(outputPath, new JpegEncoder() { Quality = imgQual });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occured while attempting to convert {System.IO.Path.GetFileName(file.FullName)}: {ex.Message}");
    }
}

void ConvertImage1(FileInfo file, int padding, int targetWidth, int targetHeight, float lightness, int? imgQual, string fileNamePostfix, int borderWidth)
{
    Console.WriteLine($"Converting {System.IO.Path.GetFileName(file.FullName)}");
    try
    {
        using var stream = file.OpenRead();
        using var image = Image.Load(stream);
        using var canvas = new Image<Rgba32>(targetWidth, targetHeight);
        var originalImageRectangle = CalculateInternalImageRectangle(
            padding,
            targetWidth,
            targetHeight,
            image.Width,
            image.Height,
            image.Bounds().IsVertical()
                ? x => (x * image.Width) / image.Height
                : x => (x * image.Height) / image.Width);
        image.Mutate(ctx => ctx
            .Resize(new Size(originalImageRectangle.width, originalImageRectangle.height)));
        canvas.Mutate(ctx => ctx
            .Fill(Color.WhiteSmoke)
            .Fill(
                Color.White,
                new RectangleF(
                    originalImageRectangle.left - borderWidth,
                    originalImageRectangle.top - borderWidth,
                    originalImageRectangle.width + borderWidth * 2,
                    originalImageRectangle.height + borderWidth * 2))
            .DrawImage(image,
                new Point(originalImageRectangle.left, originalImageRectangle.top),
                1f));
        var outputPath = System.IO.Path.Combine(
             System.IO.Path.GetDirectoryName(file.FullName),
             $"{System.IO.Path.GetFileNameWithoutExtension(file.FullName)}{fileNamePostfix}.{System.IO.Path.GetExtension(file.FullName)}");
        canvas.SaveAsJpeg(outputPath, new JpegEncoder() { Quality = imgQual });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occured while attempting to convert {System.IO.Path.GetFileName(file.FullName)}: {ex.Message}");
    }
}

Rectangle GetRectangle45(Rectangle rectangle)
{
    var center = Rectangle.Center(rectangle);
    if (rectangle.IsVertical())
    {
        var newWidth = rectangle.Width * 5 / 4;
        return new Rectangle(0, 0, newWidth, rectangle.Height);
    }
    else
    {
        var newHeight = rectangle.Width * 5 / 4;
        return new Rectangle(0, 0, rectangle.Width, newHeight);
    }
}

Size GetNewSize(Image image, Rectangle bounds45)
{
    return new Size(bounds45.Width * 5 / 4, image.Height);
}

static class X
{
    public static IImageProcessingContext CropOrResize(this IImageProcessingContext source, Rectangle rectangle, Func<bool> isVertical)
    {
        if (isVertical())
        {
            source.Crop(rectangle);
        }
        else
        {
            source.Resize(rectangle.Width, rectangle.Height);
        }
        return source;
    }

    public static bool IsVertical(this Rectangle rectangle) => rectangle.Width < rectangle.Height;
}
