namespace FitnessTrackerApi.DTOs;

public record UserRegister(string Email, string UserName, string Password);
public record UserLogin(string UserName, string Password);

public record Tokens(string RefreshToken, string AccessToken);