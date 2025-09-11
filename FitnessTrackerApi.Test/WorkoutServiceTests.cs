using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using FitnessTrackerApi.Services;
using FitnessTrackerApi.Services.Workout;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitnessTrackerApi.Test;

[Collection("Tests")]
public class WorkoutServiceTests : IClassFixture<TestDatabaseFixture>, IDisposable
{
    private readonly AppDbContext _context;
    private readonly TestDatabaseFixture _databaseFixture;
    private readonly WorkoutService _service;
    private readonly Mock<ILogger<WorkoutService>> _loggerMock;
    private readonly Mock<IPhotoService> _photoServiceMock;

    public WorkoutServiceTests(TestDatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        _context = _databaseFixture.CreateContext();

        _loggerMock = new Mock<ILogger<WorkoutService>>();
        _photoServiceMock = new Mock<IPhotoService>();

        _service = new WorkoutService(_context, _photoServiceMock.Object, _loggerMock.Object);
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
        var workout = GetDummyWorkout(user);
        _context.Users.Add(user);
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetWorkoutsAsync(user.Id, new(null, null, null, null, null, null, null, null));

        // Assert
        Assert.Single(result);
        Assert.Equal(workout.Id, result.First().Id);
        Assert.Equal(WorkoutType.Cardio, result.First().Type);
    }

    [Fact]
    public async Task GetWorkouts_WithSearchConfig_ShouldReturnSingleWorkout()
    {
        // Arrange
        var user = new User { UserName = "test" };
        _context.Users.Add(user);
        var workout1 = GetDummyWorkout(user);
        _context.Workouts.Add(workout1);
        var workout2 = GetDummyWorkout(user);
        workout2.Duration = TimeSpan.FromHours(2);
        _context.Workouts.Add(workout2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var searchConfig = new WorkoutSearchConfig([WorkoutType.Cardio, WorkoutType.HIIT],
            null, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(60), null, null, null);

        // Act
        var result = await _service.GetWorkoutsAsync(user.Id, searchConfig);

        // Assert
        Assert.Single(result);
        Assert.Equal(workout1.Id, result.First().Id);
        Assert.Equal(WorkoutType.Cardio, result.First().Type);
    }

    [Fact]
    public async Task GetWorkouts_WithSearchConfig_ShouldReturnNone()
    {
        // Arrange
        var user = new User { UserName = "test" };
        _context.Users.Add(user);
        var workout1 = GetDummyWorkout(user);
        _context.Workouts.Add(workout1);
        var workout2 = GetDummyWorkout(user);
        workout2.Duration = TimeSpan.FromHours(2);
        _context.Workouts.Add(workout2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var searchConfig = new WorkoutSearchConfig(null,
            DateTime.UtcNow.AddDays(-2), null, null, null, null, null, null);

        // Act
        var result = await _service.GetWorkoutsAsync(user.Id, searchConfig);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWorkouts_WithSearchConfig_ShouldReturnSortedDescending()
    {
        // Arrange
        var user = new User { UserName = "test" };
        _context.Users.Add(user);
        var workout1 = GetDummyWorkout(user);
        _context.Workouts.Add(workout1);
        var workout2 = GetDummyWorkout(user);
        workout2.CaloriesBurned = workout1.CaloriesBurned * 2;
        _context.Workouts.Add(workout2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var searchConfig = new WorkoutSearchConfig(null,
            null, null, null, null, null, null, WorkoutSortCriterion.CaloriesBurned, false);

        // Act
        var result = await _service.GetWorkoutsAsync(user.Id, searchConfig);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal(workout2.Id, result.First().Id);
        Assert.Equal(workout1.Id, result.Last().Id);
    }

    [Fact]
    public async Task GetWorkout_ShouldReturnWorkout()
    {
        // Arrange
        var user = new User { UserName = "test" };
        var workout = GetDummyWorkout(user);
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
        var workout = GetDummyWorkout(user);
        workout.Type = WorkoutType.Strength;
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

    [Fact]
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

    [Fact]
    public async Task DeleteWorkout_ShouldThrow_WhenNotFound()
    {
        // Act + Assert
        await Assert.ThrowsAsync<WorkoutNotFoundException>(() =>
            _service.DeleteWorkoutAsync("test", "missing"));
    }

    [Fact]
    public async Task UploadPhoto_ShouldCallUploadAndSave()
    {
        // Arrange
        var user = new User { UserName = "test" };
        var workout = GetDummyWorkout(user);
        await _context.Workouts.AddAsync(workout, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var ms = new MemoryStream();

        _photoServiceMock.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("photo-id");

        // Act
        await _service.UploadPhotoAsync(user.Id, workout.Id, ms, "test", "image/png");

        // Assert
        _photoServiceMock.Verify(x => x.UploadAsync(ms, "test", "image/png"), Times.Once);
        Assert.NotEmpty(_context.Workouts.First().ProgressPhotos);
        Assert.Equal("photo-id", _context.Workouts.First().ProgressPhotos.First());
    }

    [Fact]
    public async Task UploadPhoto_ShouldThrow_WhenWorkoutNotFound()
    {
        // Arrange
        var ms = new MemoryStream();

        // Act + Assert
        await Assert.ThrowsAsync<WorkoutNotFoundException>(() =>
            _service.UploadPhotoAsync("test", "missing", ms, "test.png", "image/png"));
    }

    [Fact]
    public async Task GetPhoto_ShouldCallGetAndReturnNameAndStream()
    {
        // Arrange
        var user = new User { UserName = "test" };
        var workout = GetDummyWorkout(user);
        await _context.Workouts.AddAsync(workout, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var ms = new MemoryStream();

        _photoServiceMock.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(("image.webp", ms));

        // Act
        var (name, stream) = await _service.GetPhotoAsync(user.Id, workout.Id, "photo-id");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(name));
        Assert.Equal(ms, stream);
        _photoServiceMock.Verify(x => x.GetAsync("photo-id"), Times.Once());
    }

    [Fact]
    public async Task GetPhoto_ShouldThrow_WhenWorkoutNotFound()
    {
        // Act + Assert
        await Assert.ThrowsAsync<WorkoutNotFoundException>(() =>
            _service.GetPhotoAsync("test", "missing", "photo-id"));
    }

    private static Workout GetDummyWorkout(User user)
        => new()
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
            CaloriesBurned = 600,
            WorkoutDate = DateTime.UtcNow
        };

    public void Dispose()
        => _databaseFixture.Cleanup();
}
