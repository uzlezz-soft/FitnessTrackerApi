using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FitnessTrackerApi.Services.Workout;

public class WorkoutService(AppDbContext context, ILogger<WorkoutService> logger) : IWorkoutService
{
    public async Task<CreatedWorkoutDto> RecordWorkoutAsync(string userId, WorkoutCreateDto model)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId) ?? throw new UserNotFoundException();
        var workout = model.ToModel();
        workout.Id = Guid.NewGuid().ToString();
        workout.User = user!;
        await context.Workouts.AddAsync(workout);
        await context.SaveChangesAsync();

        logger.LogInformation("Recorded workout for user {UserId}", userId);
        return workout.ToCreatedWorkout();
    }

    public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(string userId)
        => await context.Workouts
        .Where(x => x.User.Id == userId)
        .Select(x => x.ToWorkout())
        .ToArrayAsync();
}