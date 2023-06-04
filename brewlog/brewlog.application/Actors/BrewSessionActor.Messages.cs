using brewlog.domain.Models;
using System.Collections.Immutable;

namespace brewlog.application.Actors
{
    public partial class BrewSessionActor
    {
        public class Commands
        {
            public record AddSessionRecipe(RecipeValues Recipe);
            public record EnterValuesForYeastPackage(DateTimeOffset YeastProductionDate, double InitialYeastCells);
            public record StoreYeastStarters(IImmutableList<YeastStarter> YeastStarters);
            public record YeastStarterComplete();
            public record AddPhValue(double Ph);
            public record MashStageComplete();
            public record AddTotalWaterInLauter(double LitresWater);
            //public record AddBoilVolume(double Litres);
            //public record AddPreBoilSg(double PreBoilSg);
            public record AddPreBoilValues(double Liters, double Sg);
            public record AddAdditionalBoilWater(double Litres);
            public record AddExtendedBoilTime(double Minutes);
            public record BoilStageComplete();
            public record ReportPostCoolingValues(double Og, double VolumeInFermentationVessle);
            public record AddFermentationValue(FermentationLogValue FermentationValue);
            public record ChangeFermentationTemperature(double Temperature);
            public record FermentationStageComplete(double Fg);
            //public record ChangeBottlingStorageTemperature(double Temperature);
            //public record SetDesiredCo2Colume(double Co2Volume);
            public record AddBottlingValues(double Temperature, double Co2Volume);
        }
        public class Queries
        {
            public record GetBrewSessionState();
            public record GetBrewSessionValues();
            public record GetYeastCellsNeeded();
            public record GetYeastViability();
            public record GetTotalYeastCells();
            public record GetGramsOfDMENeeded(double LitresWater);
            public record GetPhLoweringAcid();
            public record GetSuggestedBoilSgAdjustment();
            public record GetFermentationAbv();
            public record GetCarbonationPressureInPsi();
        }
        public class Events
        {
            public record AddedSessionRecipe(RecipeValues Recipe, double CalculatedAbv, double CalculatedYeastCellsNeeded);
            public record EnteredValuesForYeastPackage(DateTimeOffset YeastProductionDate, double ViabilityPercentage, double InitialYeastCells, double CalculatedYeastCells);
            public record StoredYeastStarters(IImmutableList<YeastStarter> YeastStarters);
            public record YeastStarterCompleted();
            public record AddedMashPhValue(double Ph);
            public record MashStateCompleted();
            public record AddedTotalWaterInLauter(double LitresWater);
            //public record AddedBoilVolume(double Litres);
            //public record AddedPreBoilSg(double PreBoilSg);
            public record AddedPreBoilValues(double Liters, double Sg);
            public record AddedAdditionalBoilWater(double Litres);
            public record AddedExtendedBoilTime(double Minutes);
            public record BoilStageCompleted();
            public record ReportedPostCoolingValues(double Og, double VolumeInFermentationVessle);
            public record AddedFermentationValue(FermentationLogValue FermentationValue);
            public record ChangedFermentationTemperature(double Temperature);
            public record FermentationStageCompleted(double Fg);
            //public record ChangedBottlingStorageTemperature(double Temperature);
            //public record DesiredCo2VolumeSet(double co2Volume);
            public record AddedBottlingValues(double Temperature, double Co2Volume);
        }
        public class Responses
        {
            public record GetBrewSessionStateResponse(string State);
            public record AddSessionRecipeResponse(string? ErrorMessage = null);
            public record BrewSessionValuesResponse(SessionValues SessionValues);
            public record YeastCellsNeededResponse(double CellsNeeded);
            public record YeastViabilityResponse(double? ViabilityPercentage, double? CalculatedCellsInPackage);
            public record GetTotalYeastCellsResponse(double TotalYeastCells);
            public record GetGramsOfDMENeededResponse(double GramsOfDME);
            public record GetPhLoweringAcidResponse(double MlLacticAcid);
            public record GetSuggestedBoilSgAdjustmentResponse(double AddWater, double AddBoilMinutes);
            public record GetFermentationAbvResponse(double Abv);
            public record GetCarbonationPressureInPsiRespone(double psiPressure);
        }
    }
}
