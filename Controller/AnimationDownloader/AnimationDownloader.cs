using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace Controller.AnimationDownloader {
    class AnimationDownloader {
        private static readonly Regex GuidFinder = new Regex(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
        private static readonly Regex UrlPattern = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$");
        private static readonly HttpClient client = new HttpClient();

        public static void Download( string url ) {
            try {
                if(!UrlPattern.IsMatch(url)) { // if url is not a valid url, try to get data from the animation repository
                    url = $"https://raw.githubusercontent.com/yannikHoeflich/LedAnimations/master/InstallationManifests/{url}.json";
                }

                string id = Guid.NewGuid().ToString("D"); // creates guid to save the animation later
                string scriptPath = "data/animations/scripts/" + id + ".js"; // creates the js file path for the animation js
                DownloadManifestJson manifest;

                //download data obout the animation (where to download, name, delay, etc.)
                Program.Log("install", "[GET] manifest " + url, ConsoleColor.White);

                {
                    var response = client.GetAsync(url).GetAwaiter().GetResult();

                    if(!response.IsSuccessStatusCode) {
                        Program.Log("install", "[error] invalid response from manifest host", ConsoleColor.Red);
                        return;
                    }

                    string responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    try {
                        manifest = JsonConvert.DeserializeObject<DownloadManifestJson>(responseString);
                    } catch {
                        Program.Log("install", "[error] invalid response from manifest host", ConsoleColor.Red);
                        return;
                    }
                }

                // if the animations have dependencies download them
                if(manifest.Dependencies != null) {
                    foreach(string d in manifest.Dependencies) {
                        Download(d);
                    }
                }

                if(!string.IsNullOrEmpty(manifest.DownloadUrl)) {
                    var existingAnimation = Program.Animations.Find(x => x.DownloadUrl == manifest.DownloadUrl);
                    // download the script from the download url
                    if(existingAnimation == null || existingAnimation.Version != manifest.Version) {
                        Program.Log("install", "[GET] script " + manifest.DownloadUrl, ConsoleColor.White);
                        var response = client.GetAsync(manifest.DownloadUrl).GetAwaiter().GetResult();
                        if(!response.IsSuccessStatusCode) {
                            Program.Log("install", "[error] invalid response from script host", ConsoleColor.Red);
                            return;
                        }
                        File.WriteAllText(scriptPath, response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    } else {
                        // animation does exists!
                        // overwrite data in the downloaded data to fit the already downloaded script
                        Program.Log("install", "[info] script for " + manifest.Name + " does already exists", ConsoleColor.White);
                        id = GuidFinder.Match(existingAnimation.ScriptPath).Value;
                    }
                }

                var animation = manifest.ToAnimation("scripts/" + id + ".js"); // converts the downloaded data to a valid animation object
                //if animation contains an animation (not only animation collection)  add to animations and save new array

                if(animation != null && Program.Animations.Find(x => x.Equals(animation)) == null) {
                    Program.AddAnimation(animation);
                    Program.Log("install", "added " + animation.Name, ConsoleColor.Green);
                } else {
                    if(string.IsNullOrEmpty(manifest.DownloadUrl))
                        Program.Log("install", $"[info] from {url} is only a download collection", ConsoleColor.White);
                    else if(string.IsNullOrEmpty(manifest.Name))
                        Program.Log("install", $"[info] from {manifest.DownloadUrl} is download only", ConsoleColor.White);
                    else
                        Program.Log("install", $"[info] animation '{manifest.Name}' does already exists", ConsoleColor.White);
                }
                File.WriteAllText("data/animations/ScriptRegister.json", JsonConvert.SerializeObject(Program.Animations));
            } catch(Exception e) {
                Program.Log("install", "[error]" + e, ConsoleColor.Red);
            }
        }
    }
}
