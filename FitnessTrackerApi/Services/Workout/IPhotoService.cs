namespace FitnessTrackerApi.Services.Workout;

public interface IPhotoService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType);
    Task<(string name, Stream stream)> GetAsync(string photoId);
}
