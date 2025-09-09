using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using FitnessTrackerApi.Services;
using FitnessTrackerApi.Services.Workout;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitnessTrackerApi.Test;

public class WorkoutServiceTests
{
    private readonly AppDbContext _context;
    private readonly WorkoutService _service;
    private readonly Mock<ILogger<WorkoutService>> _loggerMock;

    public WorkoutServiceTests()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new AppDbContext(contextOptions);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<WorkoutService>>();
        _service = new WorkoutService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task RecordWorkout_ShouldSaveWorkout()
    {
        // Arrange
        var user = new User { UserName = "test" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new WorkoutCreateDto(WorkoutType.Cardio,
            [new ExerciseDto("Sprint", [new SetDto(1, 80)])],
             TimeSpan.FromMinutes(30), 200, DateTime.UtcNow);

        // Act
        var result = await _service.RecordWorkoutAsync(user.Id, dto);

        // Assert
        Assert.Single(_context.Workouts);
        Assert.NotNull(_context.Workouts.FirstOrDefault(x => x.Id == result.Id));
        Assert.Equal(WorkoutType.Cardio, _context.Workouts.First(x => x.Id == result.Id).Type);
    }

    [Fact]
    public async Task RecordWorkout_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var dto = new WorkoutCreateDto(WorkoutType.Cardio,
            [new ExerciseDto("Sprint", [new SetDto(1, 80)])],
             TimeSpan.FromMinutes(30), 200, DateTime.UtcNow);

        // Act + Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _service.RecordWorkoutAsync("missing", dto));
    }

    [Fact]
    public async Task GetWorkouts_ShouldReturnUserWorkouts()
    {
        // Arrange
        var user = new User { UserName = "test" };
        var workout = new Workout
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            User = user,
            Type = WorkoutType.CrossFit,
            Exercises = [new()
            {
                Name = "Deadlift",
                Sets = [new() { Reps = 10, Weight = 100 }, new() { Reps = 8, Weight = 100 }]
            }],
            Duration = TimeSpan.FromMinutes(10),
            CaloriesBurned = 100,
            WorkoutDate = DateTime.UtcNow
        };
        _context.Users.Add(user);
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetWorkoutsAsync(user.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(workout.Id, result.First().Id);
        Assert.Equal(WorkoutType.CrossFit, result.First().Type);
    }

    [Fact]
    public async Task GetWorkout_ShouldReturnWorkout()
    {
        // Arrange
        var user = new User { UserName = "test" };
        var workout = new Workout
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            User = user,
            Exercises = [new()
            {
                Name = "Swimming",
                Sets = [new(){ Reps = 60, Weight = 50 }]
            }],
            Type = WorkoutType.Cardio,
            Duration = TimeSpan.FromMinutes(30),
            WorkoutDate = DateTime.UtcNow
        };
        _context.Users.Add(user);
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetWorkoutAsync(user.Id, workout.Id);

        // Assert
        Assert.Equal(WorkoutType.Cardio, result.Type);
    }

    [Fact]
    public async Task GetWorkout_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var user = new User { UserName = "test" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act + Assert
        await Assert.ThrowsAsync<WorkoutNotFoundException>(() =>
            _service.GetWorkoutAsync(user.Id, "missing"));
    }

    [Fact]
    public async Task UpdateWorkout_ShouldModifyWorkout()
    {
        // Arrange
        var user = new User { UserName = "test" };
        var workout = new Workout
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            User = user,
            Exercises = [new()
            {
                Name = "Swimming",
                Sets = [new(){ Reps = 60, Weight = 50 }]
            }],
            Type = WorkoutType.Strength,
            Duration = TimeSpan.FromMinutes(30),
            CaloriesBurned = 600,
            WorkoutDate = DateTime.UtcNow
        };
        _context.Users.Add(user);
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new WorkoutUpdateDto(WorkoutType.Cardio,
            [new("Swimming", [new(60, 50)])],
            TimeSpan.FromMinutes(35), 600, DateTime.UtcNow);

        // Act
        await _service.UpdateWorkoutAsync(user.Id, workout.Id, dto);

        // Assert
        var updated = await _context.Workouts.FirstAsync(TestContext.Current.CancellationToken);
        Assert.Equal(WorkoutType.Cardio, updated.Type);
        Assert.Equal(TimeSpan.FromMinutes(35), updated.Duration);
        Assert.Equal("Swimming", updated.Exercises.First().Name);
    }

    [Fact]
    public async Task UpdateWorkout_ShouldThrow_WhenNotFound()
    {
        var dto = new WorkoutUpdateDto(WorkoutType.Cardio,
            [new("Swimming", [new(60, 50)])],
            TimeSpan.FromMinutes(35), 600, DateTime.UtcNow);

        // Act + Assert
        await Assert.ThrowsAsync<WorkoutNotFoundException>(() =>
            _service.UpdateWorkoutAsync("test", "missing", dto));
    }

    // FIXME: bug in SQLite, and does not work in In-Memory
    /*[Fact]
    public async Task DeleteWorkout_ShouldRemoveWorkout()
    {
        // Act
        var user = new User { UserName = "test" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new WorkoutCreateDto(WorkoutType.Cardio,
            [new ExerciseDto("Sprint", [new SetDto(1, 80)])],
             TimeSpan.FromMinutes(30), 200, DateTime.UtcNow);
        var saved = await _service.RecordWorkoutAsync(user.Id, dto);

        // Act
        await _service.DeleteWorkoutAsync(user.Id, saved.Id);

        // Assert
        Assert.Empty(_context.Workouts);
    }

    //[Fact]
    public async Task DeleteWorkoutAsync_ShouldThrow_WhenNotFound()
    {
        // Act + Assert
        await Assert.ThrowsAsync<WorkoutNotFoundException>(() =>
            _service.DeleteWorkoutAsync("test", "missing"));
    }*/
}
