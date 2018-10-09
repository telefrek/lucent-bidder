using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Lucent.Portal.Data;
using Lucent.Common.Entities;
using System;
using Microsoft.Extensions.Logging;
using Lucent.Common.Storage;
using System.Linq;
using Lucent.Common.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using Lucent.Common;
using Lucent.Common.Media;

namespace Lucent.Portal.Models
{
    public class CreateCreativeModel : PageModel
    {
        private readonly ILucentRepository<Creative> _db;
        private readonly ILogger<CreateCreativeModel> _log;
        readonly IMessageFactory _factory;
        readonly IMediaScanner _scanner;
        readonly string _contentRoot;
        readonly string _contentHost;
        readonly string _contentCache;

        public CreateCreativeModel(IStorageManager db, IConfiguration configuration, ILogger<CreateCreativeModel> logger, IMessageFactory factory, IMediaScanner scanner)
        {
            _contentRoot = configuration.GetValue("ContentPath", "/tmp/content");
            _contentHost = configuration.GetValue("ContentHost", "http://localhost");
            _contentCache = configuration.GetValue("ContentCache", "http://localhost");
            _db = db.GetRepository<Creative>();
            _log = logger;
            _factory = factory;
            _scanner = scanner;
        }

        public IList<Creative> Creatives { get; private set; }

        public async Task OnGetAsync()
        {
            _log.LogInformation("Getting creatives");
            Creatives = (await _db.Get()).ToList();
        }

        [DisableRequestSizeLimit] // questionable on if this is advised, but auth should keep bad actors out in theory...
        public async Task<IActionResult> OnPostAsync(List<IFormFile> files)
        {
            _log.LogInformation("Uploading {0} files", files.Count);

            if (files != null && files.Count > 0)
            {
                if (!Directory.Exists(_contentRoot))
                    Directory.CreateDirectory(_contentRoot);

                var creative = new Creative { Id = SequentialGuid.NextGuid().ToString() };
                var creativeRoot = Path.Combine(_contentRoot, creative.Id);

                if (!Directory.Exists(creativeRoot))
                    Directory.CreateDirectory(creativeRoot);

                foreach (IFormFile item in files)
                {
                    if (item.Length > 0)
                    {
                        string fileName = ContentDispositionHeaderValue.Parse(item.ContentDisposition).FileName.Trim('"');
                        _log.LogInformation("Creating {0}", fileName);
                        _log.LogInformation("ContentType: {0}", item.ContentType);
                        string fullPath = Path.Combine(creativeRoot, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await item.CopyToAsync(stream);
                        }

                        // Scan the contents
                        var content = _scanner.Scan(fullPath, item.ContentType);

                        if (content != null)
                        {
                            content.RawUri = _contentHost + "/creatives/" + creative.Id + "/" + fileName;
                            content.CreativeUri = _contentCache + "/creatives/" + creative.Id + "/" + fileName;

                            creative.Contents.Add(content);
                        }
                        else
                        {
                            new FileInfo(fullPath).Delete();
                        }
                    }
                }

                _log.LogInformation("Creating creative");
                if (await _db.TryInsert(creative))
                    return this.Content("Success");
                else
                {
                    // try, try our best to cleanup
                    foreach (var content in creative.Contents)
                        new FileInfo(content.ContentLocation).Delete();
                }
            }
            return this.Content("Fail");
        }
    }
}