using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace maplestory.io.Controllers.API
{
    [Route("api")]
    public class NXFSController : Controller
    {
        [Route("about")]
        [HttpGet]
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
        [HttpGet]
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