namespace FitnessTrackerApi.Core.Schemes;

public class Workout : IDocument
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public WorkoutType Type { get; set; }
    public List<Exercise> Exercises { get; set; }
    public TimeSpan Duration { get; set; }
    public int CaloriesBurned { get; set; }
    public List<string> ProgressPhotos { get; set; }
    public DateTime WorkoutDate { get; set; }
}
public enum WorkoutType
{
    Strength,
    Cardio,
    Flexibility,
    HIIT,
    CrossFit
}
