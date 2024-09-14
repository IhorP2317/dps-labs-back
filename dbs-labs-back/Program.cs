using System.Numerics;
using dbs_labs_back.Utils;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAny", policyBuilder => {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAny");
app.UseHttpsRedirection();
app.MapGet("/pseudo-random-numbers",
        async ([FromQuery] int a,
            [FromQuery] int x0,
            [FromQuery] int c,
            [FromQuery] BigInteger m,
            [FromQuery] int sequenceLength) =>
        {
            var result = await new LinearCongruentialGenerator()
                .GenerateRandomNumbersAndWriteToFile(
                    a,
                    x0,
                    c,
                    m,
                    sequenceLength
                );
                    
            return Results.Ok(result);
        })
    .WithName("GetPseudoRandomNumbers")
    .Produces(StatusCodes.Status200OK, typeof(LinearCongruentialGeneratorResult))
    .WithOpenApi();
app.Run();

public record LinearCongruentialGeneratorResult(List<string> PseudoRandomNumbers, int Period);