namespace FitnessTrackerApi.Models;

public class RefreshToken
{
    public string Token { get; set; }
    public virtual User User { get; set; }
    public DateTime ValidUntil { get; set; }
    public RefreshTokenStatus Status { get; set; }
}

public enum RefreshTokenStatus
{
    Valid, Revoked
}