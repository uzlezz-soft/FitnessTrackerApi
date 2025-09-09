namespace FitnessTrackerApi.Services.Workout;

public interface IPhotoService
{
    Task<string> UploadAsync(IFormFile formFile);
    Task<(string name, Stream stream)> GetAsync(string photoId);
}
