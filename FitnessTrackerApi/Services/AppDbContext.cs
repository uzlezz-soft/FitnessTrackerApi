using FitnessTrackerApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessTrackerApi.Services;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Workout> Workouts { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Workout>(workout =>
        {
            workout.HasKey(x => x.Id);
            workout.Property(x => x.Type).HasConversion<string>();

            workout.OwnsMany(x => x.Exercises, exercises =>
            {
                exercises.ToJson();
                exercises.OwnsMany(x => x.Sets, sets =>
                {
                    sets.ToJson();
                });
            });

            /*workout.OwnsMany(e => e.Exercises, exercise =>
            {
                exercise.OwnsMany(e => e.Sets, set =>
                {
                    set.WithOwner().HasForeignKey("ExerciseId");
                });
            });*/
        });
    }

}
