using FitnessTrackerApi.Models;
using FluentValidation;

namespace FitnessTrackerApi.DTOs;

public record SetDto(int Reps, double Weight);

public record ExerciseDto(string Name, IEnumerable<SetDto> Sets);

public record WorkoutCreateDto(WorkoutType Type, IEnumerable<ExerciseDto> Exercises, TimeSpan Duration, int CaloriesBurned, DateTime WorkoutDate);

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

public class WorkoutValidator : AbstractValidator<WorkoutCreateDto>
{
    public WorkoutValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleForEach(x => x.Exercises).SetValidator(new ExerciseValidator());
        RuleFor(x => x.Duration).GreaterThan(TimeSpan.Zero);
        RuleFor(x => x.CaloriesBurned).GreaterThan(0);
        RuleFor(x => x.WorkoutDate).Must(date => date.ToUniversalTime() <= DateTime.UtcNow);
    }
}

public record CreatedWorkoutDto(string Id);
public record WorkoutDto(string Id, DateTime CreatedAt, WorkoutType Type, IEnumerable<Exercise> Exercises, TimeSpan Duration, int CaloriesBurned, DateTime WorkoutDate);