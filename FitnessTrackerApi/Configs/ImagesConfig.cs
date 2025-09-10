
namespace FitnessTrackerApi.Configs;

public class ImagesConfig
{
    public const string SectionName = "Images";

    public required int SizeLimitKilobytes { get; set; }
    public required int WebpQuality { get; set; }
}
