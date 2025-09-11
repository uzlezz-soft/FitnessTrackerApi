namespace FitnessTrackerApi.Configs;

public class CacheConfig
{
    public required int WorkoutCacheSeconds { get; set; }
    public required int WorkoutSearchCacheSeconds { get; set; }
    public required int RefreshTokenCacheMinutes { get; set; }
}
