using brewlog.application.Interfaces;
using brewlog.domain.Models;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace brewlog.application.Actors
{
    public partial class BrewSessionActor
    {
        public class Commands
        {
            public record AddLogNote(string Note) : IBrewSessionValidate;
            public record AddSessionRecipe(RecipeValues Recipe) : IBrewSessionValidate;
            public record EnterValuesForYeastPackage(DateTimeOffset YeastProductionDate, double InitialYeastCells) : IBrewSessionValidate;
            public record StoreYeastStarters(IImmutableList<YeastStarter> YeastStarters) : IBrewSessionValidate;
            public record YeastStarterComplete();
            public record AddPhValue(double Ph) : IBrewSessionValidate;
            public record AddAcidAddition(double ml) : IBrewSessionValidate;
            public record MashStageComplete();
            public record AddTotalWaterInLauter(double LitresWater) : IBrewSessionValidate;
            public record AddPreBoilValues(double Liters, double Sg) : IBrewSessionValidate;
            public record AddAdditionalBoilWater(double Litres) : IBrewSessionValidate;
            public record AddExtendedBoilTime(double Minutes) : IBrewSessionValidate;
            public record BoilStageComplete();
            public record ReportPostCoolingValues(double Og, double VolumeInFermentationVessle) : IBrewSessionValidate;
            public record AddFermentationValue(FermentationLogValue FermentationValue) : IBrewSessionValidate;
            public record ChangeFermentationTemperature(double Temperature) : IBrewSessionValidate;
            public record FermentationStageComplete(double Fg) : IBrewSessionValidate;
            public record AddBottlingValues(double Temperature, double Co2Volume) : IBrewSessionValidate;
        }
        public class Queries
        {
            public record GetLogNotes();
            public record GetBrewSessionState();
            public record GetBrewSessionValues();
            public record GetYeastCellsNeeded();
            public record GetYeastViability();
            public record GetTotalYeastCells();
            public record GetGramsOfDMENeeded(double LitresWater) : IBrewSessionValidate;
            public record GetPhLoweringAcid();
            public record GetSuggestedBoilSgAdjustment();
            public record GetFermentationAbv();
            public record GetCarbonationPressureInPsi();
        }
        public class Events
        {
            public record AddedLogNote(LogNote LogNote);
            public record AddedSessionRecipe(RecipeValues Recipe, double CalculatedAbv, double CalculatedYeastCellsNeeded);
            public record EnteredValuesForYeastPackage(DateTimeOffset YeastProductionDate, double ViabilityPercentage, double InitialYeastCells, double CalculatedYeastCells);
            public record StoredYeastStarters(IImmutableList<YeastStarter> YeastStarters);
            public record YeastStarterCompleted();
            public record AddedMashPhValue(double Ph);
            public record AddedAcidAddition(double ml);
            public record MashStateCompleted();
            public record AddedTotalWaterInLauter(double LitresWater);
            public record AddedPreBoilValues(double Liters, double Sg);
            public record AddedAdditionalBoilWater(double Litres);
            public record AddedExtendedBoilTime(double Minutes);
            public record BoilStageCompleted();
            public record ReportedPostCoolingValues(double Og, double VolumeInFermentationVessle);
            public record AddedFermentationValue(FermentationLogValue FermentationValue);
            public record ChangedFermentationTemperature(double Temperature);
            public record FermentationStageCompleted(double Fg);
            public record AddedBottlingValues(double Temperature, double Co2Volume);
        }

        public record ResponseBase(string? ErrorMessage = null) : IBrewSessionResponse
        {
            public bool Success => String.IsNullOrEmpty(ErrorMessage);
        } 

        public class Responses
        {
            public record GetLogNotesResponse(IImmutableList<LogNote> LogNotes) : ResponseBase;
            public record GetBrewSessionStateResponse(string State) : ResponseBase;
            public record AddSessionRecipeResponse() : ResponseBase;
            public record BrewSessionValuesResponse(SessionValues SessionValues) : ResponseBase;
            public record YeastCellsNeededResponse(double CellsNeeded) : ResponseBase;
            public record YeastViabilityResponse(double? ViabilityPercentage, double? CalculatedCellsInPackage) : ResponseBase;
            public record GetTotalYeastCellsResponse(double TotalYeastCells) : ResponseBase;
            public record GetGramsOfDMENeededResponse(double GramsOfDME) : ResponseBase;
            public record GetPhLoweringAcidResponse(double MlLacticAcid) : ResponseBase;
            public record GetSuggestedBoilSgAdjustmentResponse(double AddWater, double AddBoilMinutes) : ResponseBase;
            public record GetFermentationAbvResponse(double Abv) : ResponseBase;
            public record GetCarbonationPressureInPsiRespone(double psiPressure) : ResponseBase;
        }
    }
}
