namespace Uyumsoft_Projem.ViewModels
{
    public class SongSingerRequestViewModel
    {
        public int RequestId { get; set; }
        public int SongId { get; set; }
        public string SongTitle { get; set; }
        public string SenderUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
