
using FitnessTrackerApi.Configs;
using FitnessTrackerApi.Exceptions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace FitnessTrackerApi.Services.Workout;

public class PhotoService(
    ILogger<PhotoService> logger,
    IOptions<ImagesConfig> options,
    IImageRepository imageRepository) : IPhotoService
{
    private readonly ImagesConfig _config = options.Value;

    public async Task<string> UploadAsync(IFormFile formFile)
    {
        if (!formFile.ContentType.StartsWith("image/")) throw new Exceptions.UnknownImageFormatException();
        if (formFile.Length > _config.SizeLimitKilobytes * 1024) throw new ImageTooLargeException();

        // TODO: split to image converter
        var encoder = new WebpEncoder { Quality = _config.WebpQuality };

        using var memoryStream = new MemoryStream();

        var image = Image.Load(formFile.OpenReadStream());
        await image.SaveAsWebpAsync(memoryStream, encoder);

        var id = await imageRepository.StoreAsync(memoryStream);
        logger.LogInformation("Uploaded image {FileName}", formFile.FileName);
        return id;
    }

    public async Task<(string name, Stream stream)> GetAsync(string photoId)
        => await imageRepository.GetAsync(photoId);
}