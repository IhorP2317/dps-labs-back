using System.Text;
using Carter;
using dbs_labs_back.Utils;
using Microsoft.AspNetCore.Mvc;

namespace dbs_labs_back.Endpoints ;

    public class DsaEndpoints:ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            
         var group = app.MapGroup("/dsa");
            group.MapGet("sign-string", GetSignatureFromString)
                .WithName(nameof(GetSignatureFromString))
                .Produces<string>(StatusCodes.Status200OK)
                .WithOpenApi();
            group.MapGet("sign-file", GetSignatureFromFile)
                .WithName(nameof(GetSignatureFromFile))
                .Produces<string>(StatusCodes.Status200OK)
                .WithOpenApi();
            group.MapGet("verify", VerifySignature)
                .WithName(nameof(VerifySignature))
                .Produces<bool>(StatusCodes.Status200OK)
                .WithOpenApi();
        }

        private  IResult GetSignatureFromString([FromQuery] string input)
        {
            var dsa = new DSAUtil();
            return Results.Ok(dsa.ProcessSignature(Encoding.ASCII.GetBytes(input)));
        }

        private async Task<IResult> GetSignatureFromFile([FromQuery] string fileName)
        {
            var absoluteFilePath = FilePathBuilder.GetSafeFilePath(fileName);
            if (!File.Exists(absoluteFilePath))
                return Results.NotFound("File not found.");
            var dsa = new DSAUtil();
            return Results.Ok(dsa.ProcessSignature(await File.ReadAllBytesAsync(absoluteFilePath)));
        }
        private async Task<IResult> VerifySignature([FromQuery] string dataFileName, [FromQuery] string signFileName)
        {
            var absoluteDataFilePath = FilePathBuilder.GetSafeFilePath(dataFileName);
            var absoluteSignFilePath = FilePathBuilder.GetSafeFilePath(signFileName);
            if (!File.Exists(absoluteDataFilePath))
                return Results.NotFound("Data file not found.");
            if (!File.Exists(absoluteSignFilePath))
                return Results.NotFound("Sign file not found.");
            var dsa = new DSAUtil();
            var signature = await File.ReadAllTextAsync(absoluteSignFilePath);
            return Results.Ok(dsa.VerifySignature(await File.ReadAllBytesAsync(absoluteDataFilePath),signature));
        }
    }