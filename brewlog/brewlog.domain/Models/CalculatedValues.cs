namespace brewlog.domain.Models
{
    public record CalculatedValues(
       double CalculatedOg,
       double CalculatedFg,
       double CalculatedAbv,
       double CalculatedMashPh,
       double CalculatedVolume
       )
    {
        public CalculatedValues() : this(0, 0, 0, 0, 0) { }
    };
}
