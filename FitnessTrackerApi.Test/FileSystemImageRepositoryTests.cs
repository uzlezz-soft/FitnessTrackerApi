using FitnessTrackerApi.Configs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Services.Workout;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FitnessTrackerApi.Test;

public class FileSystemImageRepositoryTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileSystemImageRepositoryConfig _config;
    private readonly FileSystemImageRepository _repository;

    public FileSystemImageRepositoryTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        _config = new FileSystemImageRepositoryConfig
        {
            DirectoryNesting = 2,
            Path = _tempDirectory
        };
        var options = Options.Create(_config);
        var logger = Mock.Of<ILogger<FileSystemImageRepository>>();

        _repository = new FileSystemImageRepository(options, logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, true);
    }

    [Fact]
    public async Task Store_ShouldCreateFileAndReturnId()
    {
        // Arrange
        using var input = new MemoryStream([1, 2, 3]);

        // Act
        var id = await _repository.StoreAsync(input);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(id));

        var (path, name) = GetPath(id, _config.DirectoryNesting);
        var fullPath = Path.Combine(path, name);
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task Get_ShouldReturnStream_WhenFileExists()
    {
        // Arrange
        using var input = new MemoryStream([1, 2, 3]);
        var id = await _repository.StoreAsync(input);

        // Act
        var (name, stream) = await _repository.GetAsync(id);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(name));
        Assert.NotNull(stream);
        Assert.True(stream.CanRead);

        using var data = new MemoryStream();
        await stream.CopyToAsync(data, TestContext.Current.CancellationToken);
        stream.Dispose();
        Assert.Equal([1, 2, 3], data.ToArray());
    }

    [Fact]
    public async Task Get_ShouldThrow_WhenFileNotExists()
    {
        await Assert.ThrowsAsync<ImageNotFoundException>(() => _repository.GetAsync("123456"));
    }

    private (string path, string name) GetPath(string id, int nesting)
    {
        var path = _tempDirectory;
        for (int i = 0; i < nesting; i++)
        {
            path = Path.Combine(path, id.Substring(i, 1));
        }
        return (path, id[nesting..]);
    }
}
