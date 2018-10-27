using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Lucent.Common.Entities;
using Microsoft.Extensions.Logging;
using Lucent.Common.Storage;
using System.Linq;
using Lucent.Common.Messaging;
using Microsoft.Extensions.Configuration;

namespace Lucent.Portal.Models
{
    public class CreativesModel : PageModel
    {
        private readonly IStorageRepository<Creative> _db;
        private readonly ILogger<CreativesModel> _log;
        readonly IMessageFactory _factory;
        readonly string _contentRoot;

        public CreativesModel(IStorageManager db, IConfiguration configuration, ILogger<CreativesModel> logger, IMessageFactory factory)
        {
            _contentRoot = configuration.GetValue("ContentPath", "/tmp/content");
            _db = db.GetRepository<Creative>();
            _log = logger;
            _factory = factory;
        }

        public IList<Creative> Creatives { get; private set; }

        public async Task OnGetAsync()
        {
            _log.LogInformation("Getting creatives");
            Creatives = (await _db.Get()).ToList();
        }
    }
}