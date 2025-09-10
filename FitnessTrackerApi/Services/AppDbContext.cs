using FitnessTrackerApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessTrackerApi.Services;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Models.Workout> Workouts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Models.Workout>(workout =>
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
        });

        builder.Entity<RefreshToken>(token =>
        {
            token.HasKey(x => x.Token);
            token.HasOne(x => x.User);

            token.Property(x => x.Token).HasMaxLength(128);

            token.Property(x => x.Status).HasConversion<string>();

            token.ToTable("RefreshTokens");
        });
    }

}
