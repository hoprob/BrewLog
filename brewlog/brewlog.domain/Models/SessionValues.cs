using System.Collections.Immutable;
using System.Security.Cryptography.Xml;

namespace brewlog.domain.Models
{
    public record SessionValues(
        RecipeValues Recipe,
        FermentationValues Fermentation,
        YeastValues Yeast,
        IImmutableList<YeastStarter> YeastStarters,
        CalculatedValues CalculatedValues,
        ActualValues ActualValues,
        BottlingValues Bottling,
        double Alkalinity = 30,
        double LacticAcidStrength = 80
        )
    {
        public SessionValues() : this(
            new RecipeValues(),
            new FermentationValues(),
            new YeastValues(),
            ImmutableList<YeastStarter>.Empty,
            new CalculatedValues(),
            new ActualValues(),
            new BottlingValues()
            )
        {
            
        }

        public bool RequiredValuesYeastStart()
        {
            if (Recipe is not null &&
                Yeast is not null)
                return true;
            return false;
        }

        public double TotalYeastCells()
        {
            double cells = 0;
            if(Yeast is not null)
                cells += Yeast.CalculatedPackageYeastCells;
            if (YeastStarters.Count() > 0)
                cells += YeastStarters.Sum(y => y.CalculatedYeastCells);
            return cells;
        }
    }
}
