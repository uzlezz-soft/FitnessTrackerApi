namespace FitnessTrackerApi.Core.Schemes;

public interface IDocument
{
    string Id { get; set; }
    DateTime CreatedAt { get; set; }
}
