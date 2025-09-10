
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

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        if (!contentType.StartsWith("image/")) throw new Exceptions.UnknownImageFormatException();
        if (stream.Length > _config.SizeLimitKilobytes * 1024) throw new ImageTooLargeException();

        using var memoryStream = new MemoryStream();

        try
        {
            var encoder = new WebpEncoder { Quality = _config.WebpQuality };
            var image = Image.Load(stream);
            await image.SaveAsWebpAsync(memoryStream, encoder);
        }
        catch (SixLabors.ImageSharp.UnknownImageFormatException)
        {
            throw new Exceptions.UnknownImageFormatException();
        }

        var id = await imageRepository.StoreAsync(memoryStream);
        logger.LogInformation("Uploaded image {FileName}", fileName);
        return id;
    }

    public async Task<(string name, Stream stream)> GetAsync(string photoId)
        => await imageRepository.GetAsync(photoId);
}