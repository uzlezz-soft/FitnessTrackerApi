using Microsoft.EntityFrameworkCore;

namespace FitnessTrackerApi.Models;

[Owned]
public class Exercise
{
    public string Name { get; set; }
    public List<Set> Sets { get; set; } = [];
}
