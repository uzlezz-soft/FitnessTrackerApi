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
        .Select(x => x.ToDto())
        .ToArrayAsync();

    public async Task<WorkoutDto> GetWorkoutAsync(string userId, string workoutId)
        => await context.Workouts
        .Where(x => x.Id == workoutId && x.User.Id == userId)
        .Select(x => x.ToDto())
        .FirstOrDefaultAsync() ?? throw new WorkoutNotFoundException();

    public async Task UpdateWorkoutAsync(string userId, string workoutId, WorkoutUpdateDto dto)
    {
        var workout = await context.Workouts
            .FirstOrDefaultAsync(x => x.Id == workoutId && x.User.Id == userId)
            ?? throw new WorkoutNotFoundException();

        workout.PopulateFrom(dto);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated workout {WorkoutId} for user {UserId}", workoutId, userId);
    }

    public async Task DeleteWorkoutAsync(string userId, string workoutId)
    {
        var numDeleted = await context.Workouts
            .Where(x => x.Id == workoutId && x.User.Id == userId)
            .ExecuteDeleteAsync();
        if (numDeleted != 1) throw new WorkoutNotFoundException();
        logger.LogInformation("Deleted workout {WorkoutId} for user {UserId}", workoutId, userId);
    }
}