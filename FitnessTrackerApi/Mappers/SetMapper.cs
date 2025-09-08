using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Models;

namespace FitnessTrackerApi.Mappers;

public static class SetMapper
{
    public static Set ToModel(this SetDto dto)
        => new() { Reps = dto.Reps, Weight = dto.Weight };
}