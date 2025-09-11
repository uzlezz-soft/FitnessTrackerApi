using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using FitnessTrackerApi.Services.Workout;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessTrackerApi.Endpoints;

public static class WorkoutEndpoints
{
    public static void RegisterWorkoutEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/workouts", Record)
            .WithName("Record")
            .WithOpenApi()
            .RequireAuthorization()
            .RequireRateLimiting("create-or-refresh");
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

        builder.MapGet("/workouts/{workoutId}/photos", GetPhotos)
            .WithName("GetPhotos")
            .WithOpenApi()
            .RequireAuthorization();
        builder.MapGet("/workouts/{workoutId}/photos/{photoId}", GetPhoto)
            .WithName("GetPhoto")
            .WithOpenApi()
            .RequireAuthorization();
        builder.MapPost("/workouts/{workoutId}/photos", UploadPhoto)
            .WithName("UploadPhoto")
            // https://github.com/dotnet/aspnetcore/issues/47526
            //.WithOpenApi()
            .RequireAuthorization()
            .DisableAntiforgery()
            .RequireRateLimiting("create-or-refresh");
    }

    private static async Task<Results<Created<CreatedWorkoutDto>, BadRequest, ValidationProblem>> Record(
        IWorkoutService workoutService, [FromBody] WorkoutCreateDto request, IValidator<WorkoutCreateDto> validator, HttpContext context)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        var workout = await workoutService.RecordWorkoutAsync(userId, request);
        return TypedResults.Created($"/workouts/{workout.Id}", workout);
    }

    private static async Task<Ok<IEnumerable<WorkoutDto>>> GetAll(
        IWorkoutService workoutService, HttpContext context,
        [AsParameters] WorkoutSearchConfig searchConfig)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        return TypedResults.Ok(await workoutService.GetWorkoutsAsync(userId, searchConfig));
    }

    private static async Task<Results<Ok<WorkoutDto>, NotFound>> Get(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        return TypedResults.Ok(await workoutService.GetWorkoutAsync(userId, workoutId));
    }

    private static async Task<Results<Ok, NotFound, ValidationProblem>> Update(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId,
        [FromBody] WorkoutUpdateDto model, IValidator<WorkoutUpdateDto> validator)
    {
        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        await workoutService.UpdateWorkoutAsync(userId, workoutId, model);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> Delete(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        await workoutService.DeleteWorkoutAsync(userId, workoutId);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<WorkoutPhotosDto>, NotFound>> GetPhotos(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        return TypedResults.Ok(await workoutService.GetWorkoutProgressPhotosAsync(userId, workoutId));
    }

    private static async Task<Results<Ok, NotFound, BadRequest>> UploadPhoto(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId, IFormFile formFile)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

        await workoutService.UploadPhotoAsync(userId, workoutId,
            formFile.OpenReadStream(), formFile.FileName, formFile.ContentType);
        return TypedResults.Ok();
    }

    public static async Task<Results<FileContentHttpResult, BadRequest, NotFound>> GetPhoto(
        IWorkoutService workoutService, HttpContext context, [FromRoute] string workoutId, [FromRoute] string photoId)
    {
        var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        var (name, stream) = await workoutService.GetPhotoAsync(userId, workoutId, photoId);

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return TypedResults.File(memoryStream.ToArray(), contentType: "image/webp", fileDownloadName: name);
    }
}
