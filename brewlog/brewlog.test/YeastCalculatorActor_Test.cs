using Akka.Actor;
using Akka.TestKit.VsTest;
using brewlog.application.Actors;
using brewlog.domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brewlog.test
{
    [TestClass]
    public class YeastCalculatorActor_Test : TestKit
    {
        IActorRef yeastCalculatorActor;
        public YeastCalculatorActor_Test()
        {
            yeastCalculatorActor = Sys.ActorOf<YeastCalculatorActor>();
        }

        [TestMethod]
        [DataRow(1.062, 27, BeerStyle.Ale, 308)]
        public async Task Get_Required_Yeast_Cells_Return_True(double og, double volume, BeerStyle beerStyle, double expected)
        {
            var result = await yeastCalculatorActor
                .Ask<YeastCalculatorActor.Responses.RequiredYeastCellsResponse>
                (new YeastCalculatorActor.Queries.GetRequiredYeastCells(og, volume, beerStyle));
            
            Assert.AreEqual(expected, result.yeastCells);
        }

        [TestMethod]
        [DataRow(100, 30, 75, 1.5)]
        [DataRow(85, 22, 68, 1.5)]
        [DataRow(162, 103, 68.04, 1.5)]
        [DataRow(1500, 1, 1425, 30)] 
        [DataRow(32, 26, 24.96, 1.5)]
        [DataRow(100, 53, 63, 1.5)]
        public async Task Get_Yeast_Viability_Calculated_Cells_Return_True(double initialCells, int yeastProdDateDaysAgo, double expected, double delta)
        {
            var response = await yeastCalculatorActor
                .Ask<YeastCalculatorActor.Responses.YeastViabilityResponse>(
                new YeastCalculatorActor.Queries.GetYeastViability(
                    initialCells, DateTimeOffset.Now.AddDays(-yeastProdDateDaysAgo)));

            Assert.AreEqual(expected, response.calculatedYeastCells, delta);
        }

        [TestMethod]
        [DataRow(100, 30, 75)]
        [DataRow(85, 22, 80)]
        [DataRow(162, 103, 42)] 
        [DataRow(1500, 1, 95)] 
        [DataRow(32, 26, 78)]
        [DataRow(100, 53, 63)]
        public async Task Get_Yeast_Viability_Calculated_Percentage_Return_True(double initialCells, int yeastProdDateDaysAgo, double expected)
        {
            var response = await yeastCalculatorActor
                .Ask<YeastCalculatorActor.Responses.YeastViabilityResponse>(
                new YeastCalculatorActor.Queries.GetYeastViability(
                    initialCells, DateTimeOffset.Now.AddDays(-yeastProdDateDaysAgo)));

            Assert.AreEqual(expected, response.percentage, 1.5);
        }

        [TestMethod]
        [DataRow(2, 201.8)]
        [DataRow(1.6, 161.5)]
        [DataRow(6, 605.5)]
        [DataRow(0.2, 20.2)]
        [DataRow(-3, -302.7)]
        [DataRow(102, 10292.8)]
        public async Task Get_Grams_Of_Dme_Return_True(double litresWater, double expected)
        {
            var response = await yeastCalculatorActor
                .Ask<YeastCalculatorActor.Responses.GetGramsOfDMEResponse>
                (new YeastCalculatorActor.Queries.GetGramsOfDME(litresWater));

            Assert.AreEqual(expected, response.gramsOfDME);
        }

        [TestMethod]
        [DataRow(201.82, 100, 283)]
        [DataRow(121.09, 83, 170)]
        [DataRow(1009.09, 100, 1413)]
        [DataRow(201.82, 1234, 1234)]
        public async Task Get_Starter_Produced_Cells_Return_True(double gramsOfDme, double initialCells, double expected)
        {
            var response = await yeastCalculatorActor
                .Ask<YeastCalculatorActor.Responses.GetStarterProducedCellsResponse>
                (new YeastCalculatorActor.Queries.GetStarterProducedCells(gramsOfDme, initialCells));

            Assert.AreEqual(expected, response.cellsProduced);
        }
    }
}
