using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace dbs_labs_back.Models ;

    public class ImportKeyRequest
    {
        [Required]
        [FromForm(Name = "keyFile")]
        public IFormFile KeyFile { get; set; }
    }