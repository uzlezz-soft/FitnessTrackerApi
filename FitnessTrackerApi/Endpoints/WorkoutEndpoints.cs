using System.Security.Claims;
using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Services.Workout;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTrackerApi.Endpoints;

public static class WorkoutEndpoints
{
    public static void RegisterWorkoutEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/workouts", Record)
            .WithName("Record")
            .WithOpenApi()
            .RequireAuthorization();
        builder.MapGet("/workouts", GetAll)
            .WithName("GetAll")
            .WithOpenApi()
            .RequireAuthorization();
        builder.MapGet("/workouts/{workoutId}", Get)
            .WithName("Get")
            .WithOpenApi()
            .RequireAuthorization();
        builder.MapPut("/workouts/{workoutId}", Update)
            .WithName("Update")
            .WithOpenApi()
            .RequireAuthorization();
        builder.MapDelete("/workouts/{workoutId}", Delete)
            .WithName("Delete")
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static async Task<Results<Created<CreatedWorkoutDto>, BadRequest, ValidationProblem>> Record(
        IWorkoutService workoutService, [FromBody] WorkoutCreateDto request, IValidator<WorkoutCreateDto> validator, HttpContext context)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        try
        {
            var workout = await workoutService.RecordWorkoutAsync(userId, request);
            return TypedResults.Created($"/workouts/{workout.Id}", workout);
        }
        catch (UserNotFoundException)
        {
            return TypedResults.BadRequest();
        }
    }

    private static async Task<Ok<IEnumerable<WorkoutDto>>> GetAll(
        IWorkoutService workoutService, HttpContext context)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        return TypedResults.Ok(await workoutService.GetWorkoutsAsync(userId));
    }

    private static async Task<Results<Ok<WorkoutDto>, NotFound>> Get(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        try
        {
            return TypedResults.Ok(await workoutService.GetWorkoutAsync(userId, workoutId));
        }
        catch (WorkoutNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<Ok, NotFound, ValidationProblem>> Update(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId,
        [FromBody] WorkoutUpdateDto model, IValidator<WorkoutUpdateDto> validator)
    {
        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        try
        {
            await workoutService.UpdateWorkoutAsync(userId, workoutId, model);
            return TypedResults.Ok();
        }
        catch (WorkoutNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<Ok, NotFound>> Delete(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        try
        {
            await workoutService.DeleteWorkoutAsync(userId, workoutId);
            return TypedResults.Ok();
        }
        catch (WorkoutNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
