using brewlog.api.Models.Enums;

namespace brewlog.api.Models
{
    public record Yeast(
        BeerStyle? BeerStyle,
        DateTimeOffset? YeastPackageProductionDate,
        double? YeastViability,
        double? InitialPackageYeastCells,
        double? CalculatedPackageYeastCells
        );
}
