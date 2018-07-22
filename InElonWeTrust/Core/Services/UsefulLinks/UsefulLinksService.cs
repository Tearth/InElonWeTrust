using System.Collections.Generic;
using System.Linq;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;

namespace InElonWeTrust.Core.Services.UsefulLinks
{
    public class UsefulLinksService
    {
        public List<UsefulLink> GetUsefulLinks()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return databaseContext.UsefulLinks.ToList();
            }
        }
    }
}
