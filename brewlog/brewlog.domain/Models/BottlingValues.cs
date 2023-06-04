namespace brewlog.domain.Models
{
    public record BottlingValues(
     double BottlingStorageTemperature,
     double Co2Volume
     )
    {
        public BottlingValues() : this(0, 0) { }
    }
}
