using FitnessTrackerApi.Configs;
using FitnessTrackerApi.Exceptions;
using Microsoft.Extensions.Options;

namespace FitnessTrackerApi.Services.Workout;

public class FileSystemImageRepository(
    IOptions<FileSystemImageRepositoryConfig> options,
    ILogger<FileSystemImageRepository> logger) : IImageRepository
{
    public readonly string ImagesPath = Path.GetFullPath(options.Value.Path);

    public async Task<string> StoreAsync(Stream stream)
    {
        Directory.CreateDirectory(ImagesPath);

        var id = Guid.NewGuid().ToString();
        var (path, name) = GetPath(id);

        Directory.CreateDirectory(path);

        var fullPath = Path.Combine(path, name);
        using var file = File.OpenWrite(fullPath);
        stream.Position = 0;
        await stream.CopyToAsync(file);

        logger.LogInformation("Uploaded image to {FullPath}", fullPath);

        return id;
    }

    public Task<(string name, Stream stream)> GetAsync(string id)
    {
        var (path, name) = GetPath(id);
        var fullPath = Path.Combine(path, name);

        if (!File.Exists(fullPath))
            throw new ImageNotFoundException();

        var info = new FileInfo(fullPath);
        var isoName = info.CreationTimeUtc.ToString("o");
        
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Task.FromResult((isoName, (Stream)stream));
    }

    private (string path, string name) GetPath(string id)
    {
        var path = ImagesPath;

        for (int i = 0; i < options.Value.DirectoryNesting; i++)
        {
            path = Path.Combine(path, id.Substring(i, 1));
        }
        return (path, id[options.Value.DirectoryNesting..]);
    }
}
