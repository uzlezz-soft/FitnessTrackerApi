using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Models;

namespace FitnessTrackerApi.Mappers;

public static class ExerciseMapper
{
    public static Exercise ToModel(this ExerciseDto dto)
        => new() { Name = dto.Name, Sets = dto.Sets.Select(x => x.ToModel()).ToList() };
}