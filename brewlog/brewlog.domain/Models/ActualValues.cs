namespace brewlog.domain.Models
{
    public record ActualValues(
            double ActualOg,
            double ActualFg,
            double ActualMashPh,
            double ActualPreBoilSg,
            double ActualBoilTime,
            double ActualLauterVolume,
            double ActualBoilVolume
           )
    {
        public ActualValues() : this(0, 0, 0, 0, 0, 0, 0) { }
    }
}
