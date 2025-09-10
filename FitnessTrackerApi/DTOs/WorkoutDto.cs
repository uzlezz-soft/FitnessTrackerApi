using FitnessTrackerApi.Models;
using FluentValidation;

namespace FitnessTrackerApi.DTOs;

public record SetDto(int Reps, double Weight);

public record ExerciseDto(string Name, IEnumerable<SetDto> Sets);

public record WorkoutCreateDto(WorkoutType Type, IEnumerable<ExerciseDto> Exercises, TimeSpan Duration, int CaloriesBurned, DateTime WorkoutDate);
public record WorkoutUpdateDto(WorkoutType Type, IEnumerable<ExerciseDto> Exercises, TimeSpan Duration, int CaloriesBurned, DateTime WorkoutDate);

public class SetValidator : AbstractValidator<SetDto>
{
    public SetValidator()
    {
        RuleFor(x => x.Reps).GreaterThan(0);
        RuleFor(x => x.Weight).GreaterThan(0);
    }
}

public class ExerciseValidator : AbstractValidator<ExerciseDto>
{
    public ExerciseValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleForEach(x => x.Sets).SetValidator(new SetValidator());
    }
}

public class WorkoutCreateValidator : AbstractValidator<WorkoutCreateDto>
{
    public WorkoutCreateValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleForEach(x => x.Exercises).SetValidator(new ExerciseValidator());
        RuleFor(x => x.Duration).GreaterThan(TimeSpan.Zero);
        RuleFor(x => x.CaloriesBurned).GreaterThan(0);
        RuleFor(x => x.WorkoutDate).Must(date => date.ToUniversalTime() <= DateTime.UtcNow);
    }
}

public class WorkoutUpdateValidator : AbstractValidator<WorkoutUpdateDto>
{
    public WorkoutUpdateValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleForEach(x => x.Exercises).SetValidator(new ExerciseValidator());
        RuleFor(x => x.Duration).GreaterThan(TimeSpan.Zero);
        RuleFor(x => x.CaloriesBurned).GreaterThan(0);
        RuleFor(x => x.WorkoutDate).Must(date => date.ToUniversalTime() <= DateTime.UtcNow);
    }
}

public record CreatedWorkoutDto(string Id);
public record WorkoutDto(string Id, DateTime CreatedAt, WorkoutType Type, IEnumerable<ExerciseDto> Exercises, TimeSpan Duration, int CaloriesBurned, DateTime WorkoutDate);

public record WorkoutPhotosDto(IEnumerable<string> Photos);

public enum WorkoutSortCriterion { Date, CaloriesBurned }
public record WorkoutSearchConfig(WorkoutType[]? Types, DateTime? Before, DateTime? After, TimeSpan? MinDuration, TimeSpan? MaxDuration, int? Count, int? Offset, WorkoutSortCriterion? SortBy, bool? SortAscending = true);