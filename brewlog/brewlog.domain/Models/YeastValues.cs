using brewlog.domain.Models.Enums;

namespace brewlog.domain.Models
{
    public record YeastValues(
        BeerStyle BeerStyle,
        double CalculatedYeastCellsNeeded,
        DateTimeOffset YeastPackageProductionDate,
        double YeastViability,
        double InitialPackageYeastCells,
        double CalculatedPackageYeastCells
        )
    {
        public YeastValues() : this(0, 0, DateTimeOffset.Now, 0, 0, 0) { }
    }
}
