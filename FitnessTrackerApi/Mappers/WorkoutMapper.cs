using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Models;

namespace FitnessTrackerApi.Mappers;

public static class WorkoutMapper
{
    public static Workout ToModel(this WorkoutCreateDto dto)
        => new()
        {
            Type = dto.Type,
            Exercises = dto.Exercises.Select(x => x.ToModel()).ToList(),
            Duration = dto.Duration,
            CaloriesBurned = dto.CaloriesBurned,
            WorkoutDate = dto.WorkoutDate
        };

    public static CreatedWorkoutDto ToCreatedWorkout(this Workout workout)
        => new(workout.Id);

    public static WorkoutDto ToWorkout(this Workout workout)
        => new(workout.Id, workout.CreatedAt, workout.Type, workout.Exercises,
            workout.Duration, workout.CaloriesBurned, workout.WorkoutDate);
}