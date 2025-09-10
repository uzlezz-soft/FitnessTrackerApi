
namespace FitnessTrackerApi.Configs;

public class FileSystemImageRepositoryConfig
{
    public const string SectionName = "FileSystemImageRepository";

    public required int DirectoryNesting { get; set; }
    public required string Path { get; set; }
}
