using System.Numerics;
using Carter;
using dbs_labs_back.Models;
using dbs_labs_back.Utils;
using Microsoft.AspNetCore.Mvc;

namespace dbs_labs_back.Endpoints ;

    public class PseudoRandomSequenceEndpoints:ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/pseudo-random-numbers",GetPseudoRandomNumbers)
                .WithName(nameof(GetPseudoRandomNumbers))
                .Produces<LinearCongruentialGeneratorResult> (StatusCodes.Status200OK)
                .WithOpenApi();
        }

        private static async Task<IResult> GetPseudoRandomNumbers([FromQuery] int a,
            [FromQuery] int x0,
            [FromQuery] int c,
            [FromQuery] BigInteger m,
            [FromQuery] int sequenceLength)
        {
            var result = await new LinearCongruentialGenerator(a,
                x0,
                c,
                m,
                sequenceLength)
                .GenerateRandomNumbersAndWriteToFile(
                );

            return Results.Ok(result);
        }
    }