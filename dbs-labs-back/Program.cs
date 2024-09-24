using System.Numerics;
using dbs_labs_back.Models;
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
app.MapGet("/hash-string", ( [FromQuery] string input) =>
{
    var md5 = new Md5Generator();
    md5.ComputeHash(input);
    return Results.Ok(md5.HashAsString);
})
.WithName("GetHashFromString")
.Produces(StatusCodes.Status200OK, typeof(string))
.WithOpenApi();
app.MapPost("/hash-file", async ( [FromQuery] string filePath) =>
{
    var md5 = new Md5Generator();
    try
    {
        var absolutePath = "D:\\Studying\\4th year\\PDS\\labs\\dbs-labs-back\\dbs-labs-back\\" + filePath;
        await md5.ComputeFileHashAsync(absolutePath);
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    
    var filename = $"{DateTime.Now.Ticks}.md5";
    File.WriteAllText(filename, md5.HashAsString);
    
    return Results.Ok(md5.HashAsString);
})
.WithName("GetHashFromFile")
.Produces(StatusCodes.Status200OK, typeof(string))
.WithOpenApi();
app.MapGet("/check-with-hash-string", async ([FromQuery] string input, [FromQuery] string md5FilePath) =>
    {
        var md5 = new Md5Generator();
        md5.ComputeHash(input);
        var md5FileHash = await File.ReadAllTextAsync("D:\\Studying\\4th year\\PDS\\labs\\dbs-labs-back\\dbs-labs-back\\"+md5FilePath);

        return Results.Ok(new HashComparisonObject(md5.HashAsString, md5FileHash));
    })
    .WithName("GetHashComparisonFromString")
    .Produces(StatusCodes.Status200OK, typeof(string))
    .WithOpenApi();
app.MapPost("/check-with-hash-file", async (  [FromQuery] string filePath, [FromQuery] string md5FilePath) =>
    {
        var md5 = new Md5Generator();
        try
        {
            await md5.ComputeFileHashAsync("D:\\Studying\\4th year\\PDS\\labs\\dbs-labs-back\\dbs-labs-back\\"+filePath);
        }
        catch (Exception e)
        {
            return Results.Problem(detail: e.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    
        var filename = $"{DateTime.Now.Ticks}.md5";
        File.WriteAllText(filename, md5.HashAsString);
        var md5FileHash = await File.ReadAllTextAsync("D:\\Studying\\4th year\\PDS\\labs\\dbs-labs-back\\dbs-labs-back\\"+md5FilePath);
    
        return Results.Ok(new HashComparisonObject (md5.HashAsString, md5FileHash));
    })
    .WithName("CheckHashFromFile")
    .Produces(StatusCodes.Status200OK, typeof(string))
    .WithOpenApi();
app.Run();

