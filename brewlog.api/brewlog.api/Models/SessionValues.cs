namespace brewlog.api.Models
{
    public record SessionValues(
        string? BatchName, 
        double? TargetOg, 
        double? CalculatedOg, 
        double? ActualOg,
        double? TargetFg, 
        double? CalculatedFg, 
        double? ActualFg, 
        double? PreBoilSg,
        double? TargetAbv, 
        double? CalculatedAbv, 
        double? TargetMashPh, 
        double? ActualMashPh,
        double? CalculatedMashPh,
        double? MashVolume, //TODO Totalwateradded
        double? LauterVolume,
        double? BoilVolume,
        double? TargetVolume, 
        double? CalculatedVolume,
        double? FermentationVessleVolume,
        double? MashTime, //TODO Make mash steps....
        double? BoilTime,
        double? CalculatedYeastCellsNeeded,
        double? FermentationTemp,
        double? BottlingStorageTemperature,
        double? Co2Volume,
        Yeast? Yeast,
        ICollection<YeastStarter> YeastStarters,
        ICollection<FermentationValue> FermentationValues,
        double Alkalinity = 30,
        double LacticAcidStrength = 80
        )
    {
        public bool RequiredValuesYeastStart()
        {
            if (TargetOg is not null &&
                TargetFg is not null &&
                TargetVolume is not null &&
                Yeast is not null && 
                Yeast.BeerStyle is not null)
                return true;
            return false;
        }

        public double TotalYeastCells()
        {
            double cells = 0;
            if(Yeast is not null && Yeast.CalculatedPackageYeastCells is not null)
                cells += (double)Yeast.CalculatedPackageYeastCells;
            if (YeastStarters.Count() > 0)
                cells += YeastStarters.Sum(y => y.CalculatedYeastCells);
            return cells;
        }
    }
    public record FermentationValue(DateTimeOffset ValueDateTime, double sg, double temp);
}
