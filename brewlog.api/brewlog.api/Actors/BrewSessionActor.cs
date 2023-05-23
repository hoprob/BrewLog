using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using brewlog.api.Models;
using static brewlog.api.Actors.BrewSessionActor.Events;
using static brewlog.api.Actors.BrewSessionActor.Queries;

namespace brewlog.api.Actors
{
    public class BrewSessionActor : ReceivePersistentActor
    {
        public class Commands
        {
            public record AddBrewSessionValues(SessionValues SessionValues); //TODO Make DTO //TODO CHANGE NAME TO StartValues or similar
            public record ReciepeInputComplete();
            public record EnterValuesForYeastPackage(DateTimeOffset YeastProductionDate, double InitialYeastCells);
            public record StoreYeastStarters(List<YeastStarter> YeastStarters);
            public record YeastStarterComplete();
            public record AddPhValue(double Ph);
            public record MashStageComplete();
            public record AddTotalWaterInLauter(double LitresWater);
            public record AddBoilVolume(double Litres);
            public record AddPreBoilSg(double PreBoilSg);
            public record AddAdditionalBoilWater(double Litres);
            public record AddExtendedBoilTime(double Minutes);
            public record BoilStageComplete();
            public record ReportPostCoolingValues(double Og, double VolumeInFermentationVessle);
            public record AddFermentationValue(FermentationValue FermentationValue);
            public record ChangeFermentationTemperature(double Temperature);
            public record FermentationStageComplete(double Fg);
            public record ChangeBottlingStorageTemperature(double Temperature);
            public record SetDesiredCo2Colume(double Co2Volume);
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
            public record AddedBrewSessionValues(SessionValues SessionValues); //TODO Change name to startvalues or similar
            public record EnteredValuesForYeastPackage(Yeast Yeast);
            public record RecipeInputCompleted();
            public record StoredYeastStarters(List<YeastStarter> YeastStarters);
            public record YeastStarterCompleted();
            public record AddedMashPhValue(double Ph);
            public record MashStateCompleted();
            public record AddedTotalWaterInLauter(double LitresWater); 
            public record AddedBoilVolume(double Litres); 
            public record AddedPreBoilSg(double PreBoilSg);
            public record AddedAdditionalBoilWater(double Litres);
            public record AddedExtendedBoilTime(double Minutes);
            public record BoilStageCompleted();
            public record ReportedPostCoolingValues(double Og, double VolumeInFermentationVessle);
            public record AddedFermentationValue(FermentationValue FermentationValue);
            public record ChangedFermentationTemperature(double Temperature);
            public record FermentationStageCompleted(double Fg);
            public record ChangedBottlingStorageTemperature(double Temperature);
            public record DesiredCo2VolumeSet(double co2Volume);
        }
        public class Responses
        {
            public record GetBrewSessionStateResponse(string State);
            public record AddBrewSessionValuesResponse(string? ErrorMessage = null);
            public record BrewSessionValuesResponse(SessionValues SessionValues);
            public record RecipeInputCompleteResponse(string? ErrorMessage = null);
            public record YeastCellsNeededResponse(double CellsNeeded);
            public record YeastViabilityResponse(double? ViabilityPercentage, double? CalculatedCellsInPackage);
            public record GetTotalYeastCellsResponse(double TotalYeastCells);
            public record GetGramsOfDMENeededResponse(double GramsOfDME);
            public record GetPhLoweringAcidResponse(double MlLacticAcid);
            public record GetSuggestedBoilSgAdjustmentResponse(double AddWater, double AddBoilMinutes);
            public record GetFermentationAbvResponse(double Abv);
            public record GetCarbonationPressureInPsiRespone(double psiPressure);
        }

        public override string PersistenceId => $"brewsession-{Self.Path.Name}";

        public SessionValues SessionValues { get; set; } = new SessionValues(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new Yeast(null, null, null, null, null),
            new List<YeastStarter>(),
            new List<FermentationValue>());

        public BrewSessionActor()
        {
            Recover<Events.AddedBrewSessionValues>(On);
            Recover<Events.RecipeInputCompleted>(On);
            Recover<Events.EnteredValuesForYeastPackage>(On);
            Recover<Events.StoredYeastStarters>(On);
            Recover<Events.YeastStarterCompleted>(On);
            Recover<Events.AddedMashPhValue>(On);
            Recover<Events.MashStateCompleted>(On);
            Recover<Events.AddedTotalWaterInLauter>(On);
            Recover<Events.AddedBoilVolume>(On);
            Recover<Events.AddedPreBoilSg>(On);
            Recover<Events.AddedAdditionalBoilWater>(On);
            Recover<Events.AddedExtendedBoilTime>(On);
            Recover<Events.BoilStageCompleted>(On);
            Recover<Events.AddedFermentationValue>(On);
            Recover<Events.ChangedFermentationTemperature>(On);
            Recover<Events.FermentationStageCompleted>(On);
            Recover<Events.ReportedPostCoolingValues>(On);
            Recover<Events.FermentationStageCompleted>(On);
            Recover<Events.ChangedBottlingStorageTemperature>(On);
            Recover<Events.DesiredCo2VolumeSet>(On);

            Become(Recipe);
        }

        private void Recipe()
        {
            Command<Queries.GetBrewSessionState>(_ =>
            {
                Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Recipe"));
            });

            CommandAsync<Commands.AddBrewSessionValues>(async cmd =>
            {
                var gravityCalculator = Context.ActorOf<GravityCalculatorActor>();

                var gravityResponse = await gravityCalculator.Ask<GravityCalculatorActor.Responses.CalculatedAbvResponse>(
                    new GravityCalculatorActor.Queries.GetCalculatedAbv(
                        cmd.SessionValues.TargetOg ?? 0, cmd.SessionValues.TargetFg ?? 0));

                var yeastCalculator = Context.ActorOf<YeastCalculatorActor>();

                var yeastResponse = await yeastCalculator.Ask<YeastCalculatorActor.Responses.RequiredYeastCellsResponse>(
                    new YeastCalculatorActor.Queries.GetRequiredYeastCells(cmd.SessionValues.TargetOg ?? 0,
                    cmd.SessionValues.TargetVolume ?? 0, cmd.SessionValues.Yeast.BeerStyle ?? Models.Enums.BeerStyle.Lager));

                var sessionValues = cmd.SessionValues with
                {
                    CalculatedAbv = gravityResponse.Abv,
                    CalculatedYeastCellsNeeded = yeastResponse.yeastCells
                };

                Persist(new Events.AddedBrewSessionValues(sessionValues), evnt =>
                {
                    On(evnt);
                    
                    Sender.Tell(new Responses.AddBrewSessionValuesResponse());
                });
            });

            Command<Commands.ReciepeInputComplete>(_ =>
            {
                string? errorMessage = null;
                if (!this.SessionValues.RequiredValuesYeastStart())
                    errorMessage = "There is not enough values to proceed to yeaststarter state";
                Persist(new Events.RecipeInputCompleted(), evnt =>
                {
                    On(evnt);

                    Sender.Tell(new Responses.RecipeInputCompleteResponse(errorMessage));
                });
            });

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));           
        }

        private void YeastStarter()
        {
            Command<Queries.GetBrewSessionState>(_ =>
            {
                Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("YeastStarter"));
            });

            Command<Queries.GetYeastCellsNeeded>(_ => Sender.Tell(new Responses.YeastCellsNeededResponse(SessionValues.CalculatedYeastCellsNeeded ?? 0)));
            
            CommandAsync<Commands.EnterValuesForYeastPackage>(async cmd =>
            {
                IActorRef yeastCalculator = Context.ActorOf<YeastCalculatorActor>();
                var viability = await yeastCalculator.Ask<YeastCalculatorActor.Responses.YeastViabilityResponse>(new YeastCalculatorActor.Queries.GetYeastViability(cmd.InitialYeastCells, cmd.YeastProductionDate));                             
                
                var yeast = new Yeast(null, cmd.YeastProductionDate, viability.percentage, cmd.InitialYeastCells, viability.calculatedYeastCells);
                
                Persist(new Events.EnteredValuesForYeastPackage(yeast), evnt =>
                {
                    On(evnt);
                });
            });

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Queries.GetYeastViability>(_ => Sender.Tell(
                new Responses.YeastViabilityResponse(SessionValues.Yeast.YeastViability, SessionValues.Yeast.CalculatedPackageYeastCells)));

            Command<Queries.GetTotalYeastCells>(_ => Sender.Tell(new Responses.GetTotalYeastCellsResponse(SessionValues.TotalYeastCells())));

            CommandAsync((Func<GetGramsOfDMENeeded, Task>)(async cmd =>
            {
                IActorRef yeastCalculator = Context.ActorOf<YeastCalculatorActor>();
                var response = await yeastCalculator.Ask<YeastCalculatorActor.Responses.GetGramsOfDMEResponse>(
                    new YeastCalculatorActor.Queries.GetGramsOfDME(cmd.LitresWater));

                Sender.Tell(new Responses.GetGramsOfDMENeededResponse(response.gramsOfDME));
            }));

            Command<Commands.StoreYeastStarters>(cmd => Persist(new Events.StoredYeastStarters(cmd.YeastStarters), On)); //TODO Calculate values when posting starters..

            Command<Commands.YeastStarterComplete>(_ => Persist(new Events.YeastStarterCompleted(), On));
        }

        private void Mash()
        {
            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Mash")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            CommandAsync<Queries.GetPhLoweringAcid>(async _ =>
            {
                var phActor = Context.ActorOf<PhCalculatorActor>();
                var response = await phActor.Ask<PhCalculatorActor.Responses.GetPhLoweringAcidResponse>(new PhCalculatorActor.Queries.GetPhLoweringAcid(
                    SessionValues.TargetMashPh ?? 0,
                    SessionValues.ActualMashPh ?? 0,
                    SessionValues.MashVolume ?? 0,
                    SessionValues.Alkalinity,
                    SessionValues.LacticAcidStrength
                    ));
                Sender.Tell(new Responses.GetPhLoweringAcidResponse(response.mlLActicAcid));
            });

            Command<Commands.MashStageComplete>(_ => Persist(new Events.MashStateCompleted(), On));
        }
        private void Lauter()
        {
            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Lauter")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddTotalWaterInLauter>(cmd => Persist(new Events.AddedTotalWaterInLauter(cmd.LitresWater), On));
        }
        private void Boil()
        {
            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Boil")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddBoilVolume>(cmd => Persist(new Events.AddedBoilVolume(cmd.Litres), On));

            Command<Commands.AddPreBoilSg>(cmd => Persist(new Events.AddedPreBoilSg(cmd.PreBoilSg), On));

            Command<Queries.GetSuggestedBoilSgAdjustment>(async _ =>
            {
                var gravityCalculatorActor = Context.ActorOf<GravityCalculatorActor>();

                var response = await gravityCalculatorActor.Ask<GravityCalculatorActor.Responses.GetSuggestedBoilSgAdjustmentResponse>(
                    new GravityCalculatorActor.Queries.GetSuggestedBoilSgAdjustment(
                        SessionValues.BoilVolume ?? 0,
                        SessionValues.PreBoilSg ?? 0,
                        60,
                        SessionValues.TargetOg ?? 0)
                    );

                Sender.Tell(new Responses.GetSuggestedBoilSgAdjustmentResponse(response.WaterToAdd, response.BoilMinutesToAdd));
            });

            Command<Commands.AddAdditionalBoilWater>(cmd => Persist(new Events.AddedAdditionalBoilWater(cmd.Litres), On));

            Command<Commands.AddExtendedBoilTime>(cmd => Persist(new Events.AddedExtendedBoilTime(cmd.Minutes), On));

            Command<Commands.BoilStageComplete>(_ => Persist(new Events.BoilStageCompleted(), On));

        }
        private void Cool()
        {
            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Cool")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.ReportPostCoolingValues>(cmd => Persist(new Events.ReportedPostCoolingValues(cmd.Og, cmd.VolumeInFermentationVessle), On));         
        }
        private void Ferment()
        {
            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Ferment")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddFermentationValue>(cmd => Persist(new Events.AddedFermentationValue(cmd.FermentationValue), On));

            Command<Commands.ChangeFermentationTemperature>(cmd => Persist(new Events.ChangedFermentationTemperature(cmd.Temperature), On));

            Command<Queries.GetFermentationAbv>(async _ =>
            {
                var gravityCalculatorActor = Context.ActorOf<GravityCalculatorActor>();

                var currentSg = SessionValues.FermentationValues.Last();

                var calculatedAbv = await gravityCalculatorActor.Ask<GravityCalculatorActor.Responses.CalculatedAbvResponse>(
                    new GravityCalculatorActor.Queries.GetCalculatedAbv(SessionValues.ActualOg ?? 0, currentSg.sg));

                Sender.Tell(new BrewSessionActor.Responses.GetFermentationAbvResponse(calculatedAbv.Abv)); //TODO Handle errormessage?
            });

            Command<Commands.FermentationStageComplete>(cmd => Persist(new Events.FermentationStageCompleted(cmd.Fg), On));
        }
        private void Bottling()
        {
            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new Responses.GetBrewSessionStateResponse("Bottling")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.ChangeBottlingStorageTemperature>(cmd => Persist(new Events.ChangedBottlingStorageTemperature(cmd.Temperature), On));

            Command<Commands.SetDesiredCo2Colume>(cmd => Persist(new Events.DesiredCo2VolumeSet(cmd.Co2Volume), On));

            Command<Queries.GetCarbonationPressureInPsi>(async _ =>
            {
                var bottlingCalculatorActor = Context.ActorOf<BottlingCalculatorActor>();

                var response = await bottlingCalculatorActor.Ask
                <BottlingCalculatorActor.Responses.GetNeededCo2PressureInPsiResponse>
                (new BottlingCalculatorActor.Queries.GetNeededCo2PressureInPsi(
                    SessionValues.BottlingStorageTemperature ?? 0, SessionValues.Co2Volume ?? 0));

                Sender.Tell(new Responses.GetCarbonationPressureInPsiRespone(response.pressure));
            });
        }

        private void On(Events.AddedBrewSessionValues evnt)
        {
            SessionValues = SessionValues with
            {
                BatchName = evnt.SessionValues.BatchName,
                TargetOg = evnt.SessionValues.TargetOg,
                TargetVolume = evnt.SessionValues.TargetVolume,
                TargetFg = evnt.SessionValues.TargetFg,
                TargetMashPh = evnt.SessionValues.TargetMashPh,
                CalculatedAbv = evnt.SessionValues.CalculatedAbv,
                CalculatedYeastCellsNeeded = evnt.SessionValues.CalculatedYeastCellsNeeded,
                Yeast = SessionValues.Yeast with
                {
                    BeerStyle = evnt.SessionValues.Yeast.BeerStyle
                }
            };

            if (this.SessionValues.RequiredValuesYeastStart())
                Become(YeastStarter);
        }

        private void On(Events.RecipeInputCompleted evnt)
        {
            if (this.SessionValues.RequiredValuesYeastStart())
                Become(YeastStarter);
        }

        private void On(Events.EnteredValuesForYeastPackage evnt)
        {
            SessionValues = SessionValues with
            {
                Yeast = SessionValues.Yeast with
                {
                    YeastPackageProductionDate = evnt.Yeast.YeastPackageProductionDate,
                    YeastViability = evnt.Yeast.YeastViability,
                    InitialPackageYeastCells = evnt.Yeast.InitialPackageYeastCells,
                    CalculatedPackageYeastCells = evnt.Yeast.CalculatedPackageYeastCells
                }
            };
        }

        private void On(Events.StoredYeastStarters evnt)
        {
            SessionValues = SessionValues with
            {
                YeastStarters = SessionValues.YeastStarters.Concat(evnt.YeastStarters).ToList()
            };
        }

        private void On(Events.YeastStarterCompleted evnt)
        {
            Become(Mash);
        }

        private void On(Events.AddedMashPhValue evnt)
        {
            SessionValues = SessionValues with
            {
                ActualMashPh = evnt.Ph
            };
        }

        private void On(Events.MashStateCompleted evnt)
        {
            Become(Lauter);
        }

        private void On(Events.AddedTotalWaterInLauter evnt)
        {
            SessionValues = SessionValues with
            {
                LauterVolume = evnt.LitresWater
            };

            Become(Boil);
        }

        private void On(Events.AddedBoilVolume evnt)
        {
            SessionValues = SessionValues with
            {
                BoilVolume = evnt.Litres
            };
        }

        private void On(Events.AddedPreBoilSg evnt)
        {
            SessionValues = SessionValues with
            {
                PreBoilSg = evnt.PreBoilSg
            };
        }

        private void On(Events.AddedAdditionalBoilWater evnt)
        {
            SessionValues = SessionValues with
            {
                BoilVolume = SessionValues.BoilVolume + evnt.Litres
            };
        }

        private void On(Events.AddedExtendedBoilTime evnt)
        {
            SessionValues = SessionValues with
            {
                BoilTime = SessionValues.BoilTime + evnt.Minutes
            };
        }

        private void On(Events.BoilStageCompleted evnt)
        {
            Become(Cool);
        }

        private void On(Events.ReportedPostCoolingValues evnt)
        {
            SessionValues = SessionValues with
            {
                ActualOg = evnt.Og,
                FermentationVessleVolume = evnt.VolumeInFermentationVessle
            };

            Become(Ferment);
        }

        private void On(Events.AddedFermentationValue evnt)
        {
            SessionValues = SessionValues with
            {
                FermentationValues = SessionValues.FermentationValues.Append(evnt.FermentationValue).ToList()
            };
        }

        private void On(Events.ChangedFermentationTemperature evnt)
        {
            SessionValues = SessionValues with
            {
                FermentationTemp = evnt.Temperature
            };
        }

        private void On(Events.FermentationStageCompleted evnt)
        {
            SessionValues = SessionValues with
            {
                ActualFg = evnt.Fg
            };

            Become(Bottling);
        }

        private void On(Events.ChangedBottlingStorageTemperature evnt)
        {
            SessionValues = SessionValues with
            {
                BottlingStorageTemperature = evnt.Temperature
            };
        } 

        private void On(Events.DesiredCo2VolumeSet evnt)
        {
            SessionValues = SessionValues with
            {
                Co2Volume = evnt.co2Volume
            };
        }
    }
}
