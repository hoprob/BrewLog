using Akka.Actor;
using Akka.Persistence;
using brewlog.application.Extentions;
using brewlog.domain.Models;
using System.Collections.Immutable;
using static brewlog.application.Actors.BrewSessionActor.Queries;

namespace brewlog.application.Actors
{
    public partial class BrewSessionActor : ReceivePersistentActor
    {      
        public override string PersistenceId => $"brewsession-{Self.Path.Name}";

        private SessionValues SessionValues { get; set; } = new SessionValues();

        public BrewSessionActor()
        {
            Recover<Events.AddedLogNote>(On);
            Recover<Events.AddedSessionRecipe>(On);
            Recover<Events.EnteredValuesForYeastPackage>(On);
            Recover<Events.StoredYeastStarters>(On);
            Recover<Events.YeastStarterCompleted>(On);
            Recover<Events.AddedMashPhValue>(On);
            Recover<Events.AddedAcidAddition>(On);
            Recover<Events.MashStateCompleted>(On);
            Recover<Events.AddedTotalWaterInLauter>(On);
            Recover<Events.AddedPreBoilValues>(On);
            Recover<Events.AddedAdditionalBoilWater>(On);
            Recover<Events.AddedExtendedBoilTime>(On);
            Recover<Events.BoilStageCompleted>(On);
            Recover<Events.AddedFermentationValue>(On);
            Recover<Events.ChangedFermentationTemperature>(On);
            Recover<Events.FermentationStageCompleted>(On);
            Recover<Events.ReportedPostCoolingValues>(On);
            Recover<Events.FermentationStageCompleted>(On);
            Recover<Events.AddedBottlingValues>(On);


            Become(Recipe);
        }

        private void Recipe()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "Recipe", cmd.Note)), On);
            });

            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));
            
            Command<Queries.GetBrewSessionState>(_ =>
            {
                Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Recipe"));
            });

            CommandAsync<Commands.AddSessionRecipe>(async cmd =>
            {
                var gravityCalculator = Context.ActorOf<GravityCalculatorActor>();

                var responseTargetAbv = await gravityCalculator.Ask<GravityCalculatorActor.Responses.CalculatedAbvResponse>(
                    new GravityCalculatorActor.Queries.GetCalculatedAbv(
                        cmd.Recipe.TargetOg, cmd.Recipe.TargetFg));

                var yeastCalculator = Context.ActorOf<YeastCalculatorActor>();

                var yeastResponse = await yeastCalculator.Ask<YeastCalculatorActor.Responses.RequiredYeastCellsResponse>(
                    new YeastCalculatorActor.Queries.GetRequiredYeastCells(cmd.Recipe.TargetOg,
                    cmd.Recipe.TargetVolume, cmd.Recipe.BeerStyle));

                Persist(new Events.AddedSessionRecipe(cmd.Recipe, responseTargetAbv.Abv, yeastResponse.yeastCells), evnt =>
                {
                    On(evnt);
                    
                    Sender.Tell(new Responses.AddSessionRecipeResponse());
                });

                Self.Tell(new Commands.AddLogNote("Added session recipe."));
            });

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(SessionValues)));
        }

        private void YeastStarter()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "YeastStarter", cmd.Note)), On);
            });


            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));

            Command<Queries.GetBrewSessionState>(_ =>
            {
                Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("YeastStarter"));
            });

            Command<Queries.GetYeastCellsNeeded>(_ => Sender.Tell(new Responses.YeastCellsNeededResponse(SessionValues.Yeast.CalculatedYeastCellsNeeded)));

            CommandAsync<Commands.EnterValuesForYeastPackage>(async cmd =>
            {
                IActorRef yeastCalculator = Context.ActorOf<YeastCalculatorActor>();
                var viability = await yeastCalculator.Ask<YeastCalculatorActor.Responses.YeastViabilityResponse>(new YeastCalculatorActor.Queries.GetYeastViability(cmd.InitialYeastCells, cmd.YeastProductionDate));

                Persist(new Events.EnteredValuesForYeastPackage(cmd.YeastProductionDate, viability.percentage, cmd.InitialYeastCells, viability.calculatedYeastCells), evnt =>
                {
                    On(evnt);
                });

                Self.Tell(new Commands.AddLogNote("Added yeast package values."));
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

            CommandAsync<Commands.StoreYeastStarters>(async cmd =>
            {
                IImmutableList<YeastStarter> starters = ImmutableList<YeastStarter>.Empty;
                IActorRef yeastCalculator = Context.ActorOf<YeastCalculatorActor>();
                for (int i = 0; i < cmd.YeastStarters.Count; i++)
                {
                    var starter = cmd.YeastStarters[i];
                    var response = await yeastCalculator
                    .Ask<YeastCalculatorActor.Responses.GetStarterProducedCellsResponse>
                    (new YeastCalculatorActor.Queries.GetStarterProducedCells(starter.DryMaltExtract, starter.InitialCells));

                   starters = cmd.YeastStarters.Replace(starter, starter with
                    {
                        CalculatedYeastCells = response.cellsProduced
                    });
                }
                Persist(new Events.StoredYeastStarters(starters), On);

                Self.Tell(new Commands.AddLogNote("Added yeast starters."));
            });

            Command<Commands.YeastStarterComplete>(_ =>
            {
                Persist(new Events.YeastStarterCompleted(), On);
                Self.Tell(new Commands.AddLogNote("Yeast starter stage complete. Proceed to mash."));
            });
        }

        private void Mash()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "Mash", cmd.Note)), On);
            });


            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));

            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Mash")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddPhValue>(cmd =>
            {
                Persist(new Events.AddedMashPhValue(cmd.Ph), On);
                Self.Tell(new Commands.AddLogNote($"Meashured mash pH to {cmd.Ph.ToStringWithDot()}"));
            });

            Command<Commands.AddAcidAddition>(cmd =>
            {
                Persist(new Events.AddedAcidAddition(cmd.ml), On);
                Self.Tell(new Commands.AddLogNote($"Added {cmd.ml.ToStringWithDot()}ml of lactic acid in mash."));
            });
            
            CommandAsync<Queries.GetPhLoweringAcid>(async _ =>
            {
                var phActor = Context.ActorOf<PhCalculatorActor>();
                var response = await phActor.Ask<PhCalculatorActor.Responses.GetPhLoweringAcidResponse>(new PhCalculatorActor.Queries.GetPhLoweringAcidByVolume(
                    SessionValues.Recipe.TargetMashPh, 
                    SessionValues.ActualValues.ActualMashPh,
                    SessionValues.Recipe.MashVolume, 
                    SessionValues.Alkalinity,
                    SessionValues.LacticAcidStrength
                    ));
                Sender.Tell(new Responses.GetPhLoweringAcidResponse(response.mlLActicAcid));
            });

            Command<Commands.MashStageComplete>(_ =>
            {
                Persist(new Events.MashStateCompleted(), On);
                Self.Tell(new Commands.AddLogNote("Mash stage complete. Proceed to lauter."));
            });
        }
        private void Lauter()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "Lauter", cmd.Note)), On);
            });

            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));

            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Lauter")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddTotalWaterInLauter>(cmd =>
            {
                Persist(new Events.AddedTotalWaterInLauter(cmd.LitresWater), On);
                Self.Tell(new Commands.AddLogNote($"Total water in lauter: {cmd.LitresWater.ToStringWithDot()} liters."));
            });
        }
        private void Boil()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "Boil", cmd.Note)), On);
            });


            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));

            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Boil")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddPreBoilValues>(cmd =>
            {
                Persist(new Events.AddedPreBoilValues(cmd.Liters, cmd.Sg), On);
                Self.Tell(new Commands.AddLogNote($"Added pre-boil values. Volume: {cmd.Liters.ToStringWithDot()}" +
                    $" liters. Sg: {cmd.Sg.ToStringWithDot()}"));
            });

            CommandAsync<Queries.GetSuggestedBoilSgAdjustment>(async _ =>
            {
                var gravityCalculatorActor = Context.ActorOf<GravityCalculatorActor>();

                var response = await gravityCalculatorActor.Ask<GravityCalculatorActor.Responses.GetSuggestedBoilSgAdjustmentResponse>(
                    new GravityCalculatorActor.Queries.GetSuggestedBoilSgAdjustment(
                        SessionValues.ActualValues.ActualBoilVolume,
                        SessionValues.ActualValues.ActualPreBoilSg, 
                        60,
                        SessionValues.Recipe.TargetOg)
                    );

                Sender.Tell(new Responses.GetSuggestedBoilSgAdjustmentResponse(response.WaterToAdd, response.BoilMinutesToAdd));
            });

            Command<Commands.AddAdditionalBoilWater>(cmd =>
            {
                Persist(new Events.AddedAdditionalBoilWater(cmd.Litres), On);
                Self.Tell(new Commands.AddLogNote($"Added additional boilwater: {cmd.Litres.ToStringWithDot()} liters."));
            });

            Command<Commands.AddExtendedBoilTime>(cmd =>
            {
                Persist(new Events.AddedExtendedBoilTime(cmd.Minutes), On);
                Self.Tell(new Commands.AddLogNote($"Extended boiltime with: {cmd.Minutes.ToStringWithDot()} minutes."));
            });

            Command<Commands.BoilStageComplete>(_ =>
            {
                Persist(new Events.BoilStageCompleted(), On);
                Self.Tell(new Commands.AddLogNote("Boil stage complete. Proceed to cooling."));
            });

        }
        private void Cool()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "Cooling", cmd.Note)), On);
            });


            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));

            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Cooling")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.ReportPostCoolingValues>(cmd =>
            {
                Persist(new Events.ReportedPostCoolingValues(cmd.Og, cmd.VolumeInFermentationVessle), On);
                Self.Tell(new Commands.AddLogNote($"Reported post cooling values. Og: {cmd.Og.ToStringWithDot()}." +
                    $" Volume in fermenter: {cmd.VolumeInFermentationVessle.ToStringWithDot()}"));
            });
        }
        private void Ferment()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "Ferment", cmd.Note)), On);
            });


            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));

            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new BrewSessionActor.Responses.GetBrewSessionStateResponse("Ferment")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddFermentationValue>(cmd =>
            {
                Persist(new Events.AddedFermentationValue(cmd.FermentationValue), On);
                Self.Tell(new Commands.AddLogNote($"Added fermentation value. Sg: {cmd.FermentationValue.sg.ToStringWithDot()}," +
                    $" Temperature: {cmd.FermentationValue.temp.ToStringWithDot()} C ."));
            });

            Command<Commands.ChangeFermentationTemperature>(cmd =>
            {
                Persist(new Events.ChangedFermentationTemperature(cmd.Temperature), On);
                Self.Tell(new Commands.AddLogNote($"Changed fermentation temperature to: {cmd.Temperature.ToStringWithDot()} C."));
            });

            CommandAsync<Queries.GetFermentationAbv>(async _ =>
            {
                var gravityCalculatorActor = Context.ActorOf<GravityCalculatorActor>();

                if (SessionValues.Fermentation.FermentationLogValues.Count > 0)
                {
                    var currentSg = SessionValues.Fermentation.FermentationLogValues.Last();

                    var calculatedAbv = await gravityCalculatorActor.Ask<GravityCalculatorActor.Responses.CalculatedAbvResponse>(
                        new GravityCalculatorActor.Queries.GetCalculatedAbv(SessionValues.ActualValues.ActualOg, currentSg.sg));

                    Sender.Tell(new BrewSessionActor.Responses.GetFermentationAbvResponse(calculatedAbv.Abv));
                }
                else
                    Sender.Tell(new BrewSessionActor.Responses.GetFermentationAbvResponse(0)); //TODO Handle this better??
            });

            Command<Commands.FermentationStageComplete>(cmd =>
            {
                Persist(new Events.FermentationStageCompleted(cmd.Fg), On);
                Self.Tell(new Commands.AddLogNote($"Fermentation stage complete. Proceed to bottling. Fg: {cmd.Fg.ToStringWithDot()}"));
            });
        }
        private void Bottling()
        {
            Command<Commands.AddLogNote>(cmd =>
            {
                Persist(new Events.AddedLogNote(new LogNote(DateTimeOffset.Now, "Bottling", cmd.Note)), On);
            });


            Command<Queries.GetLogNotes>(_ => Sender.Tell(new Responses.GetLogNotesResponse(SessionValues.LogNotes.OrderByDescending(x => x.Time).ToImmutableList())));

            Command<Queries.GetBrewSessionState>(_ => Sender.Tell(new Responses.GetBrewSessionStateResponse("Bottling")));

            Command<Queries.GetBrewSessionValues>(_ => Sender.Tell(new Responses.BrewSessionValuesResponse(this.SessionValues)));

            Command<Commands.AddBottlingValues>(cmd =>
            {
                Persist(new Events.AddedBottlingValues(cmd.Temperature, cmd.Co2Volume), On);
                Self.Tell(new Commands.AddLogNote($"Added bottling values. Storage temperature: {cmd.Temperature.ToStringWithDot()}," +
                    $" CO2 vol: {cmd.Co2Volume.ToStringWithDot()}"));
            });

            CommandAsync<Queries.GetCarbonationPressureInPsi>(async _ =>
            {
                var bottlingCalculatorActor = Context.ActorOf<BottlingCalculatorActor>();

                var response = await bottlingCalculatorActor.Ask
                <BottlingCalculatorActor.Responses.GetNeededCo2PressureInPsiResponse>
                (new BottlingCalculatorActor.Queries.GetNeededCo2PressureInPsi(
                    SessionValues.Bottling.BottlingStorageTemperature, SessionValues.Bottling.Co2Volume));

                Sender.Tell(new Responses.GetCarbonationPressureInPsiRespone(response.pressure));
            });
        }

        private void On(Events.AddedLogNote evnt)
        {
            SessionValues = SessionValues with
            {
                LogNotes = SessionValues.LogNotes.Add(evnt.LogNote)
            };
        }

        private void On(Events.AddedSessionRecipe evnt)
        {
            SessionValues = SessionValues with
            {
                Recipe = new RecipeValues(
                    evnt.Recipe.BatchName,
                    evnt.Recipe.TargetOg,
                    evnt.Recipe.TargetFg,
                    evnt.CalculatedAbv,
                    evnt.Recipe.TargetMashPh,
                    evnt.Recipe.MashVolume,
                    evnt.Recipe.LauterVolume,
                    evnt.Recipe.TargetVolume,
                    evnt.Recipe.MashTime,
                    evnt.Recipe.BoilTime,
                    evnt.Recipe.BeerStyle
                    ),
                CalculatedValues = SessionValues.CalculatedValues with
                {
                    CalculatedAbv = evnt.CalculatedAbv
                },
                Yeast = new YeastValues(
                    evnt.Recipe.BeerStyle,
                    evnt.CalculatedYeastCellsNeeded,
                    DateTimeOffset.Now,
                    97,
                    100,
                    97
                    ),
                ActualValues = SessionValues.ActualValues with    
                {
                    ActualBoilTime = evnt.Recipe.BoilTime
                }
            };

            if (this.SessionValues.RequiredValuesYeastStart())
                Become(YeastStarter);
        }

        private void On(Events.EnteredValuesForYeastPackage evnt)
        {
            SessionValues = SessionValues with
            {
                Yeast = SessionValues.Yeast with 
                {
                    YeastPackageProductionDate = evnt.YeastProductionDate,
                    YeastViability = evnt.ViabilityPercentage,
                    InitialPackageYeastCells = evnt.InitialYeastCells,
                    CalculatedPackageYeastCells = evnt.CalculatedYeastCells
                }
            };
        }

        private void On(Events.StoredYeastStarters evnt)
        {
            SessionValues = SessionValues with
            {
                YeastStarters = SessionValues.YeastStarters.Concat(evnt.YeastStarters).ToImmutableList()
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
                ActualValues = SessionValues.ActualValues with 
                {
                    ActualMashPh = evnt.Ph
                }
            };
        }

        private void On(Events.AddedAcidAddition evnt)
        {
            SessionValues = SessionValues with
            {
                ActualValues = SessionValues.ActualValues with
                {
                    ActualMashLacticAcidAdded =
                    SessionValues.ActualValues.ActualMashLacticAcidAdded + evnt.ml
                }
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
                ActualValues = SessionValues.ActualValues with 
                {
                    ActualLauterVolume = evnt.LitresWater
                }
            };

            Become(Boil);
        }

        private void On(Events.AddedPreBoilValues evnt)
        {
            SessionValues = SessionValues with
            {
                ActualValues = SessionValues.ActualValues with
                {
                    ActualBoilVolume = evnt.Liters,
                    ActualPreBoilSg = evnt.Sg
                }
            };
        }

        private void On(Events.AddedAdditionalBoilWater evnt)
        {
            SessionValues = SessionValues with
            {
                ActualValues = SessionValues.ActualValues with 
                {
                    ActualBoilVolume = SessionValues.ActualValues.ActualBoilVolume + evnt.Litres
                }
            };
        }

        private void On(Events.AddedExtendedBoilTime evnt)
        {
            SessionValues = SessionValues with
            {
                ActualValues = SessionValues.ActualValues with 
                {
                    ActualBoilTime = SessionValues.ActualValues.ActualBoilTime + evnt.Minutes
                }
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
                ActualValues = SessionValues.ActualValues with 
                {
                    ActualOg = evnt.Og
                },
                Fermentation = SessionValues.Fermentation with 
                {
                    FermentationVessleVolume = evnt.VolumeInFermentationVessle
                }
            };

            Become(Ferment);
        }

        private void On(Events.AddedFermentationValue evnt)
        {
            SessionValues = SessionValues with
            {
                Fermentation = SessionValues.Fermentation with 
                {
                    FermentationLogValues = SessionValues.Fermentation.FermentationLogValues
                    .Append(evnt.FermentationValue).ToImmutableList()
                }
            };
        }

        private void On(Events.ChangedFermentationTemperature evnt)
        {
            SessionValues = SessionValues with
            {
                Fermentation = SessionValues.Fermentation with 
                {
                    FermentationTemp = evnt.Temperature
                }
            };
        }

        private void On(Events.FermentationStageCompleted evnt)
        {
            SessionValues = SessionValues with
            {
                ActualValues = SessionValues.ActualValues with
                {
                    ActualFg = evnt.Fg
                }
            };

            Become(Bottling);
        }

        private void On(Events.AddedBottlingValues evnt)
        {
            SessionValues = SessionValues with
            {
                Bottling = SessionValues.Bottling with
                {
                    BottlingStorageTemperature = evnt.Temperature,
                    Co2Volume = evnt.Co2Volume
                }
            };
        }
    }
}
