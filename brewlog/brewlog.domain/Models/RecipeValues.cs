using brewlog.domain.Models.Enums;

namespace brewlog.domain.Models
{
    public record RecipeValues(
        string BatchName,
        double TargetOg,
        double TargetFg,
        double TargetAbv,
        double TargetMashPh,
        double MashVolume, //TODO Totalwateradded
        double LauterVolume,
        double TargetVolume,
        double MashTime, //TODO Make mash steps....
        double BoilTime,
        BeerStyle BeerStyle //TODO Import more styles and base Yeast on type of beer.
        )
    {
        public RecipeValues() : this ("", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) { }
    }
}
