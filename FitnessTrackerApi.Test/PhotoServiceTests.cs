using FitnessTrackerApi.Configs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Services.Workout;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace FitnessTrackerApi.Test;

public class PhotoServiceTests
{
    private readonly PhotoService _photoService;
    private readonly Mock<IImageRepository> _imageRepositoryMock;

    public PhotoServiceTests()
    {
        var logger = Mock.Of<ILogger<PhotoService>>();
        var config = new ImagesConfig
        {
            SizeLimitKilobytes = 1,
            WebpQuality = 20
        };
        var options = Options.Create(config);
        _imageRepositoryMock = new Mock<IImageRepository>();

        _photoService = new PhotoService(logger, options, _imageRepositoryMock.Object);
    }

    [Fact]
    public async Task Upload_ShouldThrowWhenUnknownContentType()
    {
        // Arrange
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("\"test\""));

        // Act + Assert
        await Assert.ThrowsAsync<UnknownImageFormatException>(() => _photoService.UploadAsync(ms, "test.json", "application/json"));
    }

    [Fact]
    public async Task Upload_ShouldThrowWhenSizeTooLarge()
    {
        // Arrange
        using var ms = new MemoryStream(new byte[4096]);

        // Act + Assert
        await Assert.ThrowsAsync<ImageTooLargeException>(() => _photoService.UploadAsync(ms, "test", "image/png"));
    }

    [Fact]
    public async Task Upload_ShouldThrow_WhenBadImage()
    {
        // Arrange
        using var ms = new MemoryStream([1, 2, 3]);

        // Act + Assert
        await Assert.ThrowsAsync<UnknownImageFormatException>(() => _photoService.UploadAsync(ms, "test", "image/png"));
    }

    [Fact]
    public async Task Upload_ShouldStoreInRepository()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _imageRepositoryMock.Setup(x => x.StoreAsync(It.IsAny<Stream>()))
            .ReturnsAsync(id);

        // 1x1 black bmp image
        var ms = new MemoryStream([66, 77, 58, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0, 40, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 196, 14, 0, 0, 196, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        // Act
        var savedId = await _photoService.UploadAsync(ms, "test.bmp", "image/bmp");

        // Assert
        Assert.Equal(id, savedId);
        _imageRepositoryMock.Verify(x => x.StoreAsync(It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldReturnNameAndStream()
    {
        // Arrange
        using var actualStream = new MemoryStream();
        _imageRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(("test.webp", actualStream));

        // Act
        var (name, stream) = await _photoService.GetAsync("my-photo");

        // Assert
        Assert.Equal("test.webp", name);
        Assert.Equal(actualStream, stream);
        _imageRepositoryMock.Verify(x => x.GetAsync("my-photo"), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldThrowWhenNotFound()
    {
        // Arrange
        _imageRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ThrowsAsync(new ImageNotFoundException());

        // Act + Assert
        await Assert.ThrowsAsync<ImageNotFoundException>(() => _photoService.GetAsync("unknown-image"));
    }
}
