using Akka.Hosting;
using Carter;
using FluentValidation.AspNetCore;
using FluentValidation;
using brewlog.api.Validators;
using ProtoBuf.Meta;
using brewlog.application.Actors;

namespace brewlog.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Add AKKA.NET Actor system
            builder.Services.AddAkka("brewlogactorsystem", (configurationBuilder, provider) =>
            {
                configurationBuilder
                .WithActors((system, actorRegistry) =>
                {
                    var coordinator = system.ActorOf(BrewSessionsCoordinatorActor.Init());

                    actorRegistry.TryRegister<BrewSessionsCoordinatorActor>(coordinator);
                });

                configurationBuilder.WithActorAskTimeout(new TimeSpan(50000000));
                configurationBuilder.AddHocon(@"
                akka {
	                loglevel=INFO,

	                persistence {
		                journal {
			                plugin = ""akka.persistence.journal.eventstore""

			                eventstore {
				                class = ""Akka.Persistence.EventStore.Journal.EventStoreJournal, Akka.Persistence.EventStore""
				                connection-string = ""ConnectTo=tcp://admin:changeit@localhost:1113;UseSslConnection=false;HeartBeatTimeout=500""
				                connection-name = ""akka""
			                }
		                }

		                query.journal.eventstore {
			                max-buffer-size = 500
		                }
	                }
                }", HoconAddMode.Append);
                            });

            builder.Services
                .AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<AddPhValueValidator>();

            ValidatorOptions.Global.LanguageManager.Enabled = false;

            builder.Services.AddCarter();
            var corstConfig = "corsConfig";
            builder.Services.AddCors(options => options.AddPolicy(corstConfig, policy =>
            {
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowAnyOrigin();
            })); ;

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(corstConfig);

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapCarter();

            app.Run();
        }
    }
}