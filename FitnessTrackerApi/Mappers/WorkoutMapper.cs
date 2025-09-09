using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Models;
using Microsoft.EntityFrameworkCore.Query;

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

    public static WorkoutDto ToDto(this Workout workout)
        => new(workout.Id, workout.CreatedAt, workout.Type, workout.Exercises.Select(x => x.ToDto()),
            workout.Duration, workout.CaloriesBurned, workout.WorkoutDate);

    public static Workout PopulateFrom(this Workout workout, WorkoutUpdateDto dto)
    {
        workout.Type = dto.Type;
        workout.Exercises = dto.Exercises.Select(x => x.ToModel()).ToList();
        workout.Duration = dto.Duration;
        workout.CaloriesBurned = dto.CaloriesBurned;
        workout.WorkoutDate = dto.WorkoutDate;
        return workout;
    }
}