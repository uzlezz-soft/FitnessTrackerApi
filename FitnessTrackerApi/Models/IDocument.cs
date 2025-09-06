namespace FitnessTrackerApi.Models;

public interface IDocument
{
    string Id { get; set; }
    DateTime CreatedAt { get; set; }
}
