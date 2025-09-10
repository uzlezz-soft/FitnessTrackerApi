namespace FitnessTrackerApi.Services.Workout;

public interface IImageRepository
{
    Task<string> StoreAsync(Stream stream);
    Task<(string name, Stream stream)> GetAsync(string id);
}
