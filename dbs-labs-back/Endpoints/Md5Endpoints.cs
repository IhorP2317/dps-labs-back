using Carter;
using dbs_labs_back.Models;
using dbs_labs_back.Utils;
using Microsoft.AspNetCore.Mvc;

namespace dbs_labs_back.Endpoints ;

    public class Md5Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/hash-string", GetHashFromString)
                .WithName(nameof(GetHashFromString))
                .Produces<string>(StatusCodes.Status200OK)
                .WithOpenApi();
            app.MapPost("/hash-file", GetHashFromFile)
                .WithName(nameof(GetHashFromFile))
                .Produces<string>(StatusCodes.Status200OK)
                .WithOpenApi();
            app.MapGet("/check-with-hash-string", GetHashComparisonFromString)
                .WithName(nameof(GetHashComparisonFromString))
                .Produces<HashComparisonObject>(StatusCodes.Status200OK)
                .WithOpenApi();
            app.MapPost("/check-with-hash-file", CheckHashFromFile)
                .WithName(nameof(CheckHashFromFile))
                .Produces<HashComparisonObject>(StatusCodes.Status200OK)
                .WithOpenApi();
        }

        private static async Task<IResult> GetHashFromString([FromQuery] string input)
        {
            var md5 = new Md5Generator();
            md5.ComputeHash(input);
            return Results.Ok(md5.HashAsString);
        }

        private static async Task<IResult> GetHashFromFile([FromQuery] string fileName)
        {
            var md5 = new Md5Generator();
            try
            {
                var absolutePath = FilePathBuilder.GetSafeFilePath(fileName);
                await md5.ComputeFileHashAsync(absolutePath);
            }
            catch (Exception e)
            {
                return Results.Problem(detail: e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }

            var filename = $"{DateTime.Now.Ticks}.md5";
            File.WriteAllText(filename, md5.HashAsString);

            return Results.Ok(md5.HashAsString);
        }

        private static async Task<IResult> GetHashComparisonFromString([FromQuery] string input,
            [FromQuery] string md5FileName)
        {
            var md5 = new Md5Generator();
            md5.ComputeHash(input);
            var md5FileHash =
                await File.ReadAllTextAsync(FilePathBuilder.GetSafeFilePath(
                                            md5FileName));

            return Results.Ok(new HashComparisonObject(md5.HashAsString, md5FileHash));
        }

        private static async Task<IResult> CheckHashFromFile([FromQuery] string fileName, [FromQuery] string md5FileName)

        {
            var md5 = new Md5Generator();
            try
            {
                await md5.ComputeFileHashAsync(FilePathBuilder.GetSafeFilePath(
                                               fileName));
            }
            catch (Exception e)
            {
                return Results.Problem(detail: e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }

            var filename = $"{DateTime.Now.Ticks}.md5";
            await File.WriteAllTextAsync(filename, md5.HashAsString);
            var md5FileHash =
                await File.ReadAllTextAsync(FilePathBuilder.GetSafeFilePath(
                                            md5FileName));

            return Results.Ok(new HashComparisonObject(md5.HashAsString, md5FileHash));
        }
    }