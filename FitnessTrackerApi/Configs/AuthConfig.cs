
namespace FitnessTrackerApi.Configs;

public class AuthConfig
{
    public const string SectionName = "Auth";

    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int AccessLifetimeMinutes { get; set; }
    public required int RefreshLifetimeHours { get; set; }
}
