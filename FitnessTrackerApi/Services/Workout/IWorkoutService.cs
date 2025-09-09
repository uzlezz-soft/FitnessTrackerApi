using FitnessTrackerApi.DTOs;

namespace FitnessTrackerApi.Services.Workout;

public interface IWorkoutService
{
    Task<CreatedWorkoutDto> RecordWorkoutAsync(string userId, WorkoutCreateDto model);
    Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(string userId);
    Task<WorkoutDto> GetWorkoutAsync(string userId, string workoutId);
    Task UpdateWorkoutAsync(string userId, string workoutId, WorkoutUpdateDto model);
    Task DeleteWorkoutAsync(string userId, string workoutId);
    Task<WorkoutPhotosDto> GetWorkoutProgressPhotosAsync(string userId, string workoutId);
    Task UploadPhotoAsync(string userId, string workoutId, IFormFile file);
    Task<(string name, Stream stream)> GetPhotoAsync(string userId, string workoutId, string photoId);
}