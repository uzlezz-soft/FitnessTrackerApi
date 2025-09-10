
namespace FitnessTrackerApi.Models;

public class Workout : IDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public WorkoutType Type { get; set; }
    public List<Exercise> Exercises { get; set; } = [];
    public TimeSpan Duration { get; set; }
    public int CaloriesBurned { get; set; }
    public List<string> ProgressPhotos { get; set; } = [];
    public DateTime WorkoutDate { get; set; }
    public virtual User User { get; set; }
}

public enum WorkoutType
{
    Strength,
    Cardio,
    Flexibility,
    HIIT,
    CrossFit
}
