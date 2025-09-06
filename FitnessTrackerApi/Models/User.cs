using Microsoft.AspNetCore.Identity;

namespace FitnessTrackerApi.Models;

public class User : IdentityUser
{
    public ICollection<Workout> Workouts { get; set; } = [];
}