using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.LinkShortener
{
    public class LinkShortenerService
    {
        public async Task<string> GetShortcutLinkAsync(string full)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var cachedLink = await databaseContext.CachedLinks.FirstOrDefaultAsync(p => p.Full == full);
                if (cachedLink != null)
                {
                    return cachedLink.Shortcut;
                }

                var shortcut = await GetShortcutFromApiAsync(full);
                var newCachedLink = new CachedLink
                {
                    Full = full,
                    Shortcut = shortcut
                };

                databaseContext.CachedLinks.Add(newCachedLink);
                databaseContext.SaveChanges();

                return shortcut;
            }
        }

        private async Task<string> GetShortcutFromApiAsync(string link)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(link);

            var responseJson = JsonConvert.DeserializeObject<ShortcutResponse>(response);
            return responseJson.Data.Url;
        }
    }
}
