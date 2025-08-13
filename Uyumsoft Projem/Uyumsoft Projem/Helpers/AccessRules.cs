using System;
using System.Linq;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Helpers
{
    public static class AccessRules
    {
        public static bool CanPlay(User user, SongsDetail dto)
        {
            if (user == null || dto == null) return false;

            if (user.UserSongs?.Any(us => us.SongId == dto.SongId) == true)
                return true;

            var singersList = dto.Singers?
                              .Split(',')
                              .Select(s => s.Trim())
                              .Where(s => !string.IsNullOrEmpty(s));

            if (singersList?.Any(s =>
                    string.Equals(s, user.UserName, StringComparison.OrdinalIgnoreCase)) == true)
                return true;

            return HasActiveSubscription(user);
        }

        public static bool CanPlay(User user, Song song)
        {
            if (user == null || song == null) return false;

            if (user.UserSongs?.Any(us => us.SongId == song.SongId) == true)
                return true;

            if (song.Artists?.Any(a => a.UserId == user.UserId) == true)
                return true;

            return HasActiveSubscription(user);
        }

        private static bool HasActiveSubscription(User user)
        {
            if (user.Subscriptions == null)
                return false;
                
            foreach (var subscription in user.Subscriptions)
            {
                if (subscription.Status == "Active" && subscription.EndDate >= DateTime.Now)
                    return true;
            }
            
            return false;
        }
    }
}

