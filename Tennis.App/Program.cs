using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Tennis.App;
using Tennis.App.Contract;
using Tennis.Core.Grains.Abstractions;

var hostBuilder = WebApplication.CreateBuilder(args);
hostBuilder.Host.UseOrleans();

hostBuilder.Services.AddLogging(builder => builder.AddSystemdConsole(options =>
{
    options.UseUtcTimestamp = false;
    options.TimestampFormat = $"{CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern}: ";
}));

hostBuilder.Services.AddEndpointsApiExplorer();
hostBuilder.Services.AddSwaggerGen();

var app = hostBuilder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var group = app.MapGroup("/match/")
               .AddEndpointFilter(async (context, next) =>
               {
                   try
                   {
                       var result = await next(context);
                       return result;
                   }
                   catch (InvalidOperationException e)
                   {
                       context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                       return e.Message;
                   }
               });

group.MapGet("/{name}",
         async (string name, IGrainFactory factory) =>
         {
             var matchGrain = factory.GetGrain<IMatchGrain>(name);
             var result = await matchGrain.GetResult();
             return new MatchResult(result.IsFinished, result.Sets.Select(SetScore.FromSet).ToList());
         })
     .WithOpenApi(operation =>
     {
         operation.OperationId = "GetMatch";
         operation.Summary = "Get the match result";
         operation.Description = "Get the match result for the given match name";
         return operation;
     });

group.MapPost("/{name}",
         async (string name, [FromBody] StartMatchRequest request, IGrainFactory factory) =>
         {
             await factory.GetGrain<IPlayerGrain>(name + "-Player1").Create(name, request.ExperiencePlayer1);
             await factory.GetGrain<IPlayerGrain>(name + "-Player2").Create(name, request.ExperiencePlayer2);
             var matchGrain = factory.GetGrain<IMatchGrain>(name);
             await matchGrain.StartMatch(request);
         })
     .WithOpenApi(operation =>
     {
         operation.OperationId = "StartMatch";
         operation.Summary = "Start a new match";
         operation.Description = "Start a new match with the given players experience levels";
         return operation;
     });

app.Run();


//needed for tests
public partial class Program
{
}