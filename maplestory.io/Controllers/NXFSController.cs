using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;

namespace maplestory.io.Controllers
{
    [Route("api")]
    public class NXFSController : Controller
    {
        [Route("about")]
        public async Task<IActionResult> About()
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage resp = await client.GetAsync($"https://nxl.nxfs.nexon.com/games/10100/info.json"))
            {
                string APIResponse = await resp.Content.ReadAsStringAsync();
                int statusCode = (int)resp.StatusCode;
                if (statusCode > 500) throw new InvalidOperationException("Invalid response", new Exception(APIResponse));
                if (statusCode > 400) throw new InvalidOperationException("Invalid data presented", new Exception(APIResponse));

                return Json(JsonConvert.DeserializeObject(APIResponse));
            }
        }

        [Route("banners")]
        public async Task<IActionResult> Banners()
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage resp = await client.GetAsync($"https://nxl.nxfs.nexon.com/banners/10100/list.json"))
            {
                string APIResponse = await resp.Content.ReadAsStringAsync();
                int statusCode = (int)resp.StatusCode;
                if (statusCode > 500) throw new InvalidOperationException("Invalid response", new Exception(APIResponse));
                if (statusCode > 400) throw new InvalidOperationException("Invalid data presented", new Exception(APIResponse));

                return Json(JsonConvert.DeserializeObject(APIResponse));
            }
        }
    }
}