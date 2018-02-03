using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/gms/latest/news")]
    public class GMSNews : Controller
    {
        [Route("article/{id}")]
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetArticle(int id)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage resp = await client.GetAsync($"https://gapi.nexon.net/cms/news/article/{id}"))
            {
                string APIResponse = await resp.Content.ReadAsStringAsync();
                int statusCode = (int)resp.StatusCode;
                if (statusCode > 500) throw new InvalidOperationException("Invalid response", new Exception(APIResponse));
                if (statusCode > 400) throw new InvalidOperationException("Invalid data presented", new Exception(APIResponse));

                return Json(JsonConvert.DeserializeObject(APIResponse));
            }
        }

        [Route("{type?}")]
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetNews(string type = "all")
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage resp = await client.GetAsync($"https://gapi.nexon.net/cms/news/1180/{type ?? "all"}"))
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
