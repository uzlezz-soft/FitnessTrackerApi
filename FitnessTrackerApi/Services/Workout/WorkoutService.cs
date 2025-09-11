using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Mappers;
using FitnessTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTrackerApi.Services.Workout;

public class WorkoutService(AppDbContext context, IPhotoService photoService, ILogger<WorkoutService> logger) : IWorkoutService
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

    public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(string userId, WorkoutSearchConfig searchConfig)
    {
        IQueryable<Models.Workout> query = context.Workouts
            .Where(x => x.User.Id == userId);

        if (searchConfig.Types != null && searchConfig.Types.Length != 0)
            query = query.Where(x => searchConfig.Types.Contains(x.Type));

        if (searchConfig.Before is DateTime before) query = query.Where(x => x.WorkoutDate <= before);
        if (searchConfig.After is DateTime after) query = query.Where(x => x.WorkoutDate >= after);

        if (searchConfig.MinDuration is TimeSpan minDuration) query = query.Where(x => x.Duration >= minDuration);
        if (searchConfig.MaxDuration is TimeSpan maxDuration) query = query.Where(x => x.Duration <= maxDuration);

        query = searchConfig.SortBy switch
        {
            WorkoutSortCriterion.Date => searchConfig.SortAscending ?? true
                ? query.OrderBy(x => x.WorkoutDate) : query.OrderByDescending(x => x.WorkoutDate),
            WorkoutSortCriterion.CaloriesBurned => searchConfig.SortAscending ?? true
                ? query.OrderBy(x => x.CaloriesBurned) : query.OrderByDescending(x => x.CaloriesBurned),
            _ => query
        };

        if (searchConfig.Offset is int offset) query = query.Skip(offset);
        if (searchConfig.Count is int count) query = query.Take(count);

        return await query.Select(x => x.ToDto()).ToArrayAsync();
    }

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
        // Can't use ExecuteDeleteAsync as there is a known bug with owned entities
        // This bug is fixed in EF Core 9.0+, but oh well
        // https://github.com/dotnet/efcore/issues/32823

        var workout = await context.Workouts
            .FirstOrDefaultAsync(x => x.Id == workoutId && x.User.Id == userId)
            ?? throw new WorkoutNotFoundException();

        context.Workouts.Remove(workout);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted workout {WorkoutId} for user {UserId}", workoutId, userId);
    }

    public async Task<WorkoutPhotosDto> GetWorkoutProgressPhotosAsync(string userId, string workoutId)
        => (await context.Workouts
            .Select(x => new { x.Id, UserId = x.User.Id, Photos = x.ToPhotos() })
            .FirstOrDefaultAsync(x => x.Id == workoutId && x.UserId == userId))?.Photos
            ?? throw new WorkoutNotFoundException();

    public async Task UploadPhotoAsync(string userId, string workoutId, Stream stream, string fileName, string contentType)
    {
        var workout = await context.Workouts
            .FirstOrDefaultAsync(x => x.Id == workoutId && x.User.Id == userId)
            ?? throw new WorkoutNotFoundException();

        var id = await photoService.UploadAsync(stream, fileName, contentType);
        workout.ProgressPhotos.Add(id);
        await context.SaveChangesAsync();
    }

    public async Task<(string name, Stream stream)> GetPhotoAsync(string userId, string workoutId, string photoId)
    {
        var workout = await context.Workouts
            .FirstOrDefaultAsync(x => x.Id == workoutId && x.User.Id == userId)
            ?? throw new WorkoutNotFoundException();

        return await photoService.GetAsync(photoId);
    }
}