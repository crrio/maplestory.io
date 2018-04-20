using maplestory.io.Data;
using maplestory.io.Entities;
using maplestory.io.Entities.Models;
using maplestory.io.Models;
using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PKG1;
using SixLabors.ImageSharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace maplestory.io.Controllers
{
    [Route("api/wz")]
    public class WZController : Controller
    {
        private ApplicationDbContext _ctx;
        private readonly IWZFactory _wzFactory;
        private JsonSerializerSettings serializerSettings;

        public WZController(ApplicationDbContext dbCtx, IWZFactory wzFactory)
        {
            _ctx = dbCtx;
            _wzFactory = wzFactory;

            IgnorableSerializerContractResolver resolver = new IgnorableSerializerContractResolver();
            resolver.Ignore<MapleVersion>(a => a.Location);

            serializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = resolver,
                Formatting = Formatting.Indented
            };

        }

        [Route("")]
        [HttpGet]
        public IActionResult Index() => Json(_ctx.MapleVersions.ToArray(), serializerSettings);

        [Route("{region}/{version}/{*path}")]
        public IActionResult Query(Region region, string version, string path)
        {
            MSPackageCollection wz = _wzFactory.GetWZ(region, version);
            if (string.IsNullOrEmpty(path))
                return Json(new
                {
                    children = wz.Packages.Keys.ToArray(),
                    type = -1
                });

            WZProperty prop = wz.Resolve(path);
            if (prop == null) return NotFound();

            if (prop is IWZPropertyVal)
                return Json(new
                {
                    children = prop.Children.Select(c => c.Name),
                    type = prop.Type,
                    value = prop.Type == PropertyType.Canvas ? prop.ResolveForOrNull<Image<Rgba32>>() : ((IWZPropertyVal)prop).GetValue()
                });

            return Json(new
            {
                children = prop.Children.Select(c => c.Name),
                type = prop.Type
            });
        }

        [Route("export/{region}/{version}/{*path}")]
        public IActionResult Export(Region region, string version, string path, [FromQuery] bool rawImage = false)
        {
            MSPackageCollection wz = _wzFactory.GetWZ(region, version);
            WZProperty prop = wz.Resolve(path);

            if (prop == null) return NotFound();
            if (prop.Type == PropertyType.Directory) return Forbid();

            Queue<WZProperty> propQueue = new Queue<WZProperty>();
            propQueue.Enqueue(prop);

            if (rawImage && rawImage)
            {
                using (WZReader reader = prop.FileContainer.GetContentReader(null, prop.Resolve()))
                {
                    reader.BaseStream.Seek(prop.Offset, SeekOrigin.Begin);
                    return File(reader.ReadBytes((int)prop.Size), "wizet/img", $"{prop.NameWithoutExtension}.img");
                }
            }

            using (MemoryStream mem = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Create, true))
                {
                    while (propQueue.TryDequeue(out WZProperty entry))
                    {
                        byte[] data = null;
                        string extension = null;
                        if (entry.Type == PropertyType.Audio)
                        {
                            data = (byte[])((IWZPropertyVal<byte[]>)entry).Value;
                            extension = "mp3";
                        }
                        else if (entry.Type == PropertyType.Canvas)
                        {
                            data = entry.ResolveForOrNull<Image<Rgba32>>()?.ImageToByte(Request, false, null, false);
                            extension = "png";
                        }

                        if (data != null)
                        {
                            ZipArchiveEntry zipEntry = archive.CreateEntry(entry.Path + '.' + extension, CompressionLevel.Optimal);
                            using (Stream zipEntryData = zipEntry.Open())
                            {
                                zipEntryData.Write(data, 0, data.Length);
                                zipEntryData.Flush();
                            }
                        }

                        foreach (WZProperty child in entry.Children) propQueue.Enqueue(child);
                    }
                }

                return File(mem.ToArray(), "application/zip", "export.zip");
            }
        }
    }
}
