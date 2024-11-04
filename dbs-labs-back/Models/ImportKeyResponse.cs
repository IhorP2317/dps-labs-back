namespace dbs_labs_back.Models ;

    public class ImportKeyResponse
    {
        public string Message { get; set; } = null!;
        public bool HasPublicKey { get; set; }
        public bool HasPrivateKey { get; set; }
    }