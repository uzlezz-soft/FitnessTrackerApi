using FitnessTrackerApi.DTOs;

namespace FitnessTrackerApi.Services.Workout;

public interface IWorkoutService
{
    Task<CreatedWorkoutDto> RecordWorkoutAsync(string userId, WorkoutCreateDto model);
    Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(string userId);
}