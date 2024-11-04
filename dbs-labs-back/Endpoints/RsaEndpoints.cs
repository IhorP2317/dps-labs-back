using System.Diagnostics;
using dbs_labs_back.Models;
using dbs_labs_back.Utils;


namespace dbs_labs_back.Endpoints ;

using System.IO;
using System.Threading.Tasks;
using Carter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

    public class RsaEndpoints(RSAUtil rsaUtil) : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/rsa");
            group.MapPost("/import-key", ImportKeyAsync)
                .Accepts<IFormFile>("multipart/form-data")
                .WithName(nameof(ImportKeyAsync))
                .Produces<ImportKeyResponse>(StatusCodes.Status200OK)
                .WithOpenApi()
                .DisableAntiforgery();
            group.MapGet("/export-key", ExportKeyAsync)
                .Produces<FileContentResult>(StatusCodes.Status200OK)
                .WithOpenApi();
            group.MapPost("/encode", EncodeRsa)
                .Produces<CryptoResponse>(StatusCodes.Status200OK)
                .WithOpenApi();
            group.MapPost("/decode", DecodeRsa)
                .Produces<CryptoResponse>(StatusCodes.Status200OK)
                .WithOpenApi();
            group.MapPost("/generate-keys", GenerateNewKeys)
                .Produces(StatusCodes.Status204NoContent)
                .WithOpenApi();
            group.MapGet("/get-key-size", GetCurrentKeySize)
                .Produces<KeyLengthInBytesRSAEnum>(StatusCodes.Status200OK)
                .WithOpenApi();

        }

        private async Task<IResult> ImportKeyAsync([FromForm] ImportKeyRequest request)
        {
            var keyFile = request.KeyFile;
            if (keyFile == null || keyFile.Length == 0)
            {
                return Results.BadRequest("Key file is empty.");
            }

            await using var stream = keyFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            var keyXml = await reader.ReadToEndAsync();

            rsaUtil.ImportKey(keyXml);
            var hasPublicKey = rsaUtil.HasPublicKey;
            var hasPrivateKey = rsaUtil.HasPrivateKey;

            var message = hasPublicKey && hasPrivateKey
                ? "Pair of keys imported successfully."
                : hasPublicKey && !hasPrivateKey
                    ? "Public key imported successfully."
                    : "Invalid key format!";

            var response = new ImportKeyResponse
            {
                Message = message,
                HasPublicKey = hasPublicKey,
                HasPrivateKey = hasPrivateKey
            };
            return Results.Ok(response);
        }

        private IResult GetCurrentKeySize()
        {
            return Results.Ok(rsaUtil.KeySizeInBytes);
        }

        private  IResult GenerateNewKeys([FromQuery] KeyLengthInBytesRSAEnum keyLength)
        {
            rsaUtil.GenerateNewKeyPair(keyLength);
            return Results.NoContent();
        }
        private IResult ExportKeyAsync([FromQuery] bool includePrivateKey = false)
        {
            if (!rsaUtil.HasPublicKey)
            {
                return Results.BadRequest("No key available to export. Generate or import a key first.");
            }

            if (includePrivateKey && !rsaUtil.HasPrivateKey)
            {
                return Results.BadRequest("Private key not available. Cannot export private key.");
            }

            var keyXml = includePrivateKey ? rsaUtil.ExportFullKey() : rsaUtil.ExportPublicKey();


            var keyBytes = System.Text.Encoding.UTF8.GetBytes(keyXml);

            var fileName = includePrivateKey ? "privateKey.xml" : "publicKey.xml";

            return Results.File(keyBytes, "application/xml", fileName);
        }

        private async Task<IResult> EncodeRsa([FromQuery] string fileName)
        {
            var stopWatch = new Stopwatch();
            var absoluteFilePath = FilePathBuilder.GetSafeFilePath(fileName);
            if (!File.Exists(absoluteFilePath))
                return Results.NotFound("File not found.");
            stopWatch.Start();
            var encryptionResult = rsaUtil.EncryptDataPerBlock(await File.ReadAllBytesAsync(absoluteFilePath));
            stopWatch.Stop();
            var outFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-rsa-enc{Path.GetExtension(fileName)}";
            var outputFilePath = FilePathBuilder.GetSafeFilePath(outFileName);
            await File.WriteAllBytesAsync(outputFilePath, encryptionResult);
            return Results.Ok(
                new CryptoResponse
                {
                    ResultFileName = outFileName,
                    Duration = stopWatch.Elapsed
                }
                );
        }
        private async Task<IResult> DecodeRsa([FromQuery] string fileName)
        {
            var stopWatch = new Stopwatch();
            var absoluteFilePath = FilePathBuilder.GetSafeFilePath(fileName);
            if (!File.Exists(absoluteFilePath))
                return Results.NotFound("File not found.");
            stopWatch.Start();
            var encryptionResult = rsaUtil.DecryptDataPerBlock(await File.ReadAllBytesAsync(absoluteFilePath));

            stopWatch.Stop();
            var outFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-rsa-dec{Path.GetExtension(fileName)}";
            var outputFilePath = FilePathBuilder.GetSafeFilePath(outFileName);
            await File.WriteAllBytesAsync(outputFilePath, encryptionResult);
            return Results.Ok(
                new CryptoResponse
                {
                    ResultFileName = outFileName,
                    Duration = stopWatch.Elapsed
                }
                );
        }
    }