using System.Diagnostics;
using System.Text;
using Carter;
using dbs_labs_back.Models;
using dbs_labs_back.Settings;
using dbs_labs_back.Utils;
using Microsoft.AspNetCore.Mvc;

namespace dbs_labs_back.Endpoints ;

    public class Rc5Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/rc5");
            group.MapPost("/encode", EncodeRC5)
                .WithName(nameof(EncodeRC5))
                .Produces<CryptoResponse>(StatusCodes.Status200OK)
                .WithOpenApi();
            group.MapPost("/decode", DecodeRC5)
                .WithName(nameof(DecodeRC5))
                .Produces<CryptoResponse>(StatusCodes.Status200OK)
                .WithOpenApi();
        }

        private static async Task<IResult> EncodeRC5([FromQuery] string key, [FromQuery] string fileName,
            [FromBody] RC5Settings rc5Settings)
        { 
            var stopWatch = new Stopwatch();
            var absoluteFilePath = FilePathBuilder.GetSafeFilePath(fileName);

            if (!File.Exists(absoluteFilePath))
                return Results.NotFound("File not found.");

            var rc5 = new RC5Util(rc5Settings, key);
            stopWatch.Start();
            var encodedFileContent = rc5.EncipherCBCPAD(
                await File.ReadAllBytesAsync(fileName));
            stopWatch.Stop();
            var outputFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-rc5-enc{Path.GetExtension(fileName)}";
            var outputFilePath = FilePathBuilder.GetSafeFilePath(outputFileName);

            await File.WriteAllBytesAsync(outputFilePath, encodedFileContent);
            return Results.Ok(new CryptoResponse
            {
                ResultFileName = outputFileName,
                Duration = stopWatch.Elapsed
            });
        }

        private static async Task<IResult> DecodeRC5([FromQuery] string key, [FromQuery] string fileName,
            [FromBody] RC5Settings rc5Settings)
        {
            var stopWatch = new Stopwatch();
            var absoluteFilePath = FilePathBuilder.GetSafeFilePath(fileName);

            if (!File.Exists(absoluteFilePath))
                return Results.NotFound("File not found.");
            var rc5 = new RC5Util(rc5Settings, key);
             stopWatch.Start();
            var decodedFileContent = rc5.DecipherCBCPAD(
                await File.ReadAllBytesAsync(fileName));
            stopWatch.Stop();
            var outputFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-rc5-dec{Path.GetExtension(fileName)}";
            var outputFilePath = FilePathBuilder.GetSafeFilePath(outputFileName);
            await File.WriteAllBytesAsync(outputFilePath, decodedFileContent);
            return Results.Ok(new CryptoResponse
            {
                ResultFileName = outputFileName,
                Duration = stopWatch.Elapsed
            });
        }

       
        
    }