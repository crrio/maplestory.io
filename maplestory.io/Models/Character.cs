using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace maplestory.io.Models
{
    public class Character
    {
        static ConcurrentDictionary<string, Tuple<Character, DateTime>> cache = new ConcurrentDictionary<string, Tuple<Character, DateTime>>();

        public string Name, Job, World, RankDirection, Avatar;
        public DateTime Got;
        public long Ranking, Exp, RankMovement;
        public int Level;
        // JobIcon is going to be private until someone wants it public. We'll probably want to cache it on our side to not abuse Nexon's CDN.
        private string realAvatarUrl, JobIcon;
        private byte[] avatarData;

        public static bool IsCached(string characterName, string rankingMode = "overall", string rankAttribute = "legendary")
        {
            Tuple<Character, DateTime> cachedEntry;
            return cache.TryGetValue(string.Join("-", characterName.ToLower(), rankingMode.ToLower(), rankAttribute.ToLower()), out cachedEntry) && cachedEntry.Item2 > DateTime.Now;
        }

        public static async Task<Character> GetCharacter(string characterName, string rankingMode = "overall", string rankAttribute = "legendary")
        {
            string cacheName = string.Join("-", characterName.ToLower(), rankingMode.ToLower(), rankAttribute.ToLower());

            Tuple<Character, DateTime> cachedEntry = null;
            if (cache.TryGetValue(cacheName, out cachedEntry) && cachedEntry.Item2 > DateTime.Now) return cachedEntry.Item1;

            string rankingResponse = null;

            int retryCount = 0;
            // Keep trying up to 5 times then throw the exception and let it bubble up
            do
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                        rankingResponse = await client.GetStringAsync($"http://maplestory.nexon.net/rankings/{rankingMode}-ranking/{rankAttribute}?pageIndex=1&character_name={characterName}&search=true");
                    break;
                }
                catch (HttpRequestException requestException) 
                {
                    if (requestException.InnerException != null && requestException.InnerException.Message.Contains("terminated abnormally") && retryCount++ < 5)
                        continue;
                    else throw;
                }
            } while (true);

            DateTime got = DateTime.Now;
            rankingResponse = rankingResponse.Replace("\t", "").Replace("\n", "").Replace("\r", "");

            string pattern;

            if (rankingMode != "fame")
                pattern = "<td>[ \r\n\t]*([0-9]*)[ \r\n\t]*<\/td>[ \r\n\t]*<td> <img class=\"avatar\"[ ]* src=\"([^\"]*)\"></td>[ ]*<td>(<img src=\"http://nxcache.nexon.net/maplestory/img/bg/bg-immigrant.png\"/><br />)*([^<]*)</td>[ ]*<td><a class=\"([^\"]*)\" href=\"([^\"]*)\" title=\"([^\"]*)\">&nbsp;</a></td>[ ]*<td><img class=\"job\" src=\"([^\"]*)\" alt=\"([^\"]*)\" title=\"[^\"]*\"></td>[ ]*<td class=\"level-move\">[ ]*([0-9]*)<br />[ ]*\\(([0-9]*)\\)[ ]*<br />[ ]*<div class=\"rank-([^\"]*)\">([^<]*)</div>";
            else
                pattern = "<td>[ \r\n\t]*([0-9]*)[ \r\n\t]*<\/td>[ \r\n\t]*<td> <img class=\"avatar\"[ ]* src=\"([^\"]*)\"></td>[ ]*<td>(<img src=\"http://nxcache.nexon.net/maplestory/img/bg/bg-immigrant.png\"/><br />)*([^<]*)</td>[ ]*<td><a class=\"([^\"]*)\" href=\"([^\"]*)\" title=\"([^\"]*)\">&nbsp;</a></td>[ ]*<td><img class=\"job\" src=\"([^\"]*)\" alt=\"([^\"]*)\" title=\"[^\"]*\"></td>[ ]*<td class=\"level-move\">[ ]*([0-9]*)";

            Regex search = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            Match matches = search.Match(rankingResponse);

            List<Character> characters = new List<Character>();
            do
            {
                GroupCollection captures = matches.Groups;
                if (captures.Count != 14) throw new InvalidOperationException($"Expected 14 captures, got {captures.Count}");

                var parsed = new Character()
                {
                    Ranking = long.Parse(captures[1].Value),
                    realAvatarUrl = captures[2].Value,
                    // We don't want people spamming Nexon, so pls go through us kty <3
                    Avatar = $"/api/character/{captures[4].Value}/avatar",
                    Name = captures[4].Value,
                    World = captures[7].Value,
                    JobIcon = captures[8].Value,
                    Job = captures[9].Value,
                    Level = int.Parse(captures[10].Value),
                    Exp = long.Parse(captures[11].Value),
                    RankDirection = captures[12].Value,
                    RankMovement = long.Parse(captures[13].Value),
                    Got = got
                };

                characters.Add(parsed);

                Tuple<Character, DateTime> cacheEntry = new Tuple<Character, DateTime>(parsed, got.AddDays(1));

                cache.AddOrUpdate(string.Join("-", parsed.Name.ToLower(), rankingMode.ToLower(), rankAttribute.ToLower()), s => { return cacheEntry; }, (s, old) => { return cacheEntry; });
            } while ((matches = matches.NextMatch()) != null && matches.Success);

            return characters.First(c => c.Name.Equals(characterName, StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task<byte[]> GetAvatar(int retryCount = 0)
        {
            // It is assumed it was cached recently, otherwise this Character wouldn't even be loaded into memory.
            if (avatarData != null) return avatarData;

            using (HttpClient client = new HttpClient())
            {
                byte[] avatarDataResponse = null;
                try
                {
                    avatarDataResponse = await client.GetByteArrayAsync(realAvatarUrl);
                }
                catch (HttpRequestException requestException)
                {
                    if (requestException.InnerException != null && requestException.InnerException.Message.Contains("terminated abnormally") && retryCount < 5)
                    {
                        byte[] tryAgain = await GetAvatar(retryCount + 1);
                        if (tryAgain == null) throw requestException;
                    }
                    else throw;
                }
                avatarData = avatarDataResponse;
                return avatarDataResponse;
            }
        }

        public override string ToString() => $"{Name} the level {Level} {Job}";
    }
}
