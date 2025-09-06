using Microsoft.EntityFrameworkCore;

namespace FitnessTrackerApi.Models;

[Owned]
public class Set
{
    public int Reps { get; set; }
    public double Weight { get; set; }
}
