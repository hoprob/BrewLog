using Akka.Actor;
using Akka.Hosting;
using brewlog.api.Actors;
using brewlog.api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace brewlog.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Add AKKA.NET Actor system
            builder.Services.AddAkka("brewlogactorsystem", (configurationBuilder, provider) =>
            {
                configurationBuilder.WithActorAskTimeout(new TimeSpan(50000000));
            });

            builder.Services
                .AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<AddPhValueValidator>();

            ValidatorOptions.Global.LanguageManager.Enabled = false;

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}