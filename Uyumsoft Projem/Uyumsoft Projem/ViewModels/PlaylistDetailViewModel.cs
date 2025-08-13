using System.Collections.Generic;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.ViewModels
{
    public class PlaylistDetailViewModel
    {
        public Playlist Playlist { get; set; } = null!;
        public List<Song> Songs { get; set; } = new List<Song>();
        public List<int> OwnedSongIds { get; set; } = new List<int>();
        public decimal TotalPriceOfMissingSongs { get; set; }
        public bool HasActiveSubscription { get; set; }  
    }
}