using System.Collections.Immutable;

namespace brewlog.domain.Models
{
    public record FermentationValues(
        double FermentationVessleVolume,
        double FermentationTemp,
        IImmutableList<FermentationLogValue> FermentationLogValues 
        )
    {
        public FermentationValues() : this(0, 0, ImmutableList<FermentationLogValue>.Empty) { }
    }

    public record FermentationLogValue(DateTimeOffset ValueDateTime, double sg, double temp);

}
