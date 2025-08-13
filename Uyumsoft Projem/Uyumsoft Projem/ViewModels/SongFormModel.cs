using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Uyumsoft_Projem.ViewModels
{

    public class SongFormModel
    {
        public int? SongId { get; set; }
        
        public string? Artist { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Genre { get; set; }

        [Required]
        public decimal Price { get; set; }

        public IFormFile? UploadSong { get; set; }
        public IFormFile? UploadImage { get; set; }

        public string? ExistingFilePath { get; set; }
        public string? ExistingImagePath { get; set; }

        [Required]
        public int UserId { get; set; }
    }

}
