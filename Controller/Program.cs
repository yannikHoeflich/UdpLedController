using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Controller {
    class Program {
        static void Main( string[] args ) {
            try {
                StartLogger(); //starts thread to log everthing with no multythreading isues 

                //create directories to save data
                if(!Directory.Exists("data"))
                    Directory.CreateDirectory("data");
                if(!Directory.Exists("data/animations"))
                    Directory.CreateDirectory("data/animations");
                if(!Directory.Exists("data/animations/scripts"))
                    Directory.CreateDirectory("data/animations/scripts");
                if(!Directory.Exists("data/website")) {
                    Directory.CreateDirectory("data/website");
                    File.WriteAllText("data/website/index.html", WebsiteConstants.Html); // create html file for the website
                }

            } catch {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[critical error] error with writing files and creating directories, maybe try to run the program with higher privileges");
                Environment.Exit(0);
            }

            // start aguments
            if(args.Length > 0) {
                switch(args[0].ToLower()) {
                    //install animations
                    case "install":
                    case "i":
                    if(args.Length >= 2) {
                        AnimationDownloader.AnimationDownloader.Download(args[1]);
                        switch(args[2].ToLower()) {
                            case "/s":
                            new Program().Start(); //starts an the program (only of parameter is given)
                            break;
                        }
                    } else {
                        Console.WriteLine("invalid syntax: Controller install[url or name]");
                    }
                    break;

                }


                return;
            }

            new Program().Start();
        }

        public Program() {
        }

        readonly Page ErrorPage = new Page( ) {
            CanCache = true ,
            Cache = new Cache( ) {
                CacheUpdater = ( ) => Encoding.UTF8.GetBytes( "%code <br> %message% <br> %details%")
            }
        };

        public static List<Animation> Animations = new List<Animation>();
        public static List<Animation> VisibleAnimations => Animations.FindAll(x => !x.Hidden);

        //main method
        void Start() {
            new Thread(ConsoleInputThread).Start();

            //init webpages for Api
            var pages = new List<Page>( ) {
                new Page( ) {
                    Cache = new Cache(){
                        CacheUpdater = () => File.ReadAllBytes("data/website/index.html"), UpdateTimeSpan = TimeSpan.FromHours(1)
                    },
                    Method = HomePageRequest,
                    Regex = new Regex(@"\/"),
                    CanCache = true
                },
                new Page( ) {
                    Method = API_CALL,
                    Regex = new Regex(@"\/api\?([a-z]+=[a-zA-Z0-9%\.]*&?)+"),
                    CanCache = false
                },
            };

            //create weblistenet
            var listener = new HttpListener( );
            listener.Prefixes.Add("http://*:80/");
            listener.Start();
            Program.Log("API", "Listening...", ConsoleColor.Blue);

            // Api listener
            try {
                while(true) {
                    HttpListenerContext context = listener.GetContext( ); //waiting for request
                    new Thread(() => {
                        try {
                            HttpListenerRequest request = context.Request;
                            HttpListenerResponse response = context.Response;

                            byte[] buffer = new byte[0];

                            Program.Log("API", "[request] " + request.RawUrl, ConsoleColor.Blue);
                            var requestPages = pages.Find( x => x.Regex.Match( request.RawUrl ).Length == request.RawUrl.Length );
                            if(requestPages == null) { // can't find a webpage where the regex is mathing to the requested url
                                buffer = Err(404, request.RawUrl);
                                goto send;
                            }

                            try {
                                var inputData = new InputData( ) {
                                    Page = requestPages ,
                                    request = request
                                };
                                OutputData outputData = requestPages.Method.Invoke( inputData ); // run method to get the content for response
                                buffer = outputData.buffer;
                                response.ContentType = outputData.ContentTypeString;
                                foreach(Cookie c in outputData.NewCookies) { // create cookies, if needed
                                    response.AppendCookie(c);
                                }
                            } catch(NotImplementedException) {
                                buffer = Err(403, "comming soon");
                            } catch(Exception e) {
                                buffer = Err(502, e.Message);
                            }

                            send:
                            //send response to client
                            response.ContentLength64 = buffer.Length;
                            Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        } catch { }
                    }).Start();
                }
            } catch(Exception e) {
                Console.ForegroundColor = ConsoleColor.Red;
                Program.Log("API", "[error] " + e.Message, ConsoleColor.Red);
            } finally {
                listener.Stop();
            }
        }

        private OutputData HomePageRequest( InputData arg ) => new OutputData() { buffer = arg.Page.Cache.Value }; // Main page, just return html

        Animation CurrentAnimation = null;
        private OutputData API_CALL( InputData arg ) {
            Dictionary<string , string> data = GetGetData(arg.request);
            object responseObject = "Invalid request"; // reponse, later turtend into json
            try {
                switch(data["method"]) {
                    case "getAnimations":
                    responseObject = VisibleAnimations;
                    var animations = Animations.Select(x => new ApiAnimation(x)); // convert animations into the right format and send back
                    break;

                    case "getDevices":
                    responseObject = StaticAnimationThreadClass.Devices;
                    break;

                    case "setAnimation":
                    //find animation with id
                    ulong id = ulong.Parse( data["animation"] );
                    CurrentAnimation = VisibleAnimations[(int) id];
                    //run animation in new Thread
                    StaticAnimationThreadClass.StartAnimation(CurrentAnimation);
                    responseObject = true;
                    break;

                    case "getCurrentAnimation":
                    responseObject = new ApiAnimation(CurrentAnimation);
                    break;

                    case "scanDevices":
                    string ip;
                    //scan for ip if it is not given as GET parameter in request
                    if(!data.TryGetValue("ip", out ip)) {
                        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                        socket.Connect("8.8.8.8", 65534);
                        var endPoint = socket.LocalEndPoint as IPEndPoint;
                        ip = endPoint.Address.ToString();
                    }
                    //scan for devices is local network
                    StaticAnimationThreadClass.ScanDevices(ip);
                    //return all devices that were found
                    responseObject = StaticAnimationThreadClass.Devices;
                    break;

                    case "scanAnimations":
                    try {
                        ScanAnimations(); //reload animations
                    } catch(Exception e) { Program.Log("API", "[error]" + e.Message, ConsoleColor.Red); }
                    responseObject = true;
                    break;

                    case "getDimmng":
                    responseObject = StaticAnimationThreadClass.Dimming;
                    break;

                    case "setDimmng":
                    Program.Log("debug", data["factor"], ConsoleColor.White);
                    StaticAnimationThreadClass.Dimming = double.Parse(data["factor"], CultureInfo.GetCultureInfo("en-US"));
                    break;
                }
            } catch(KeyNotFoundException) { }
            return new OutputData() { buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObject)), ContentType = ContentType.application_json };
        }

        //reload animations
        public void ScanAnimations() {
            if(!File.Exists("data/animations/ScriptRegister.json")) {
                Animations = new List<Animation>();
                File.WriteAllText("data/animations/ScriptRegister.json", JsonConvert.SerializeObject(Animations));
                return;
            }
            var animations = JsonConvert.DeserializeObject<List<Animation>>(File.ReadAllText("data/animations/ScriptRegister.json"));
            foreach(var ani in animations) {
                AddAnimation(ani);
            }
        }


        static readonly Regex VariableNameFinder = new Regex(@"%.+%");
        //add new animation to array (format json / convert object to right types)
        public static void AddAnimation( Animation animation ) {
            var newData = new Dictionary<string, dynamic>();


            // object name has to begin with "_type_" and then the object name like "_type_color" to give the "color" object the right type
            if(animation.Data != null) {
                // get input for all values that are "%_input%"
                foreach(var d in animation.Data) {
                    if(d.Value is string && d.Value == "%_input%") {
                        Console.Write($"value for {d.Key}: ");
                        dynamic value = Console.ReadLine();
                        if(float.TryParse(value, out float newValue)) {
                            value = newValue;
                        }
                        animation.Data[d.Key] = value;
                    }
                }

                foreach(var d in animation.Data) {
                    if(d.Key.StartsWith("_type_")) {
                        string argName = d.Key.TrimStart("_type_".ToCharArray());
                        Type type = ByName(d.Value);
                        var data = animation.Data.ToList().Find(x => x.Key == argName);
                        dynamic value = data.Value;
                        if(!(value is JToken)) { // checks if object is already converted
                            value = value.GetType().IsValueType || value is string ? (dynamic) new JValue(value) : (dynamic) new JObject(value); // converts false converted object back to Json-values or -objects
                        }
                        newData.Add(d.Key, type.ToString());
                        newData.Add(data.Key, RunMethodWithType(type, value, "ToObject"));
                    }
                }


                foreach(var d in animation.Data) {
                    if(!d.Key.StartsWith("_") && !newData.ContainsKey(d.Key)) {
                        newData.Add(d.Key, d.Value);
                    }
                }

                animation.Data = newData;

                //replace all %key% with keyname in data values
                foreach(var d in animation.Data) {
                    if(!(d.Value is string))
                        continue;
                    while(true) {
                        var match = VariableNameFinder.Match(d.Value);
                        if(!match.Success)
                            break;
                        string valueName = match.Value.Trim('%');
                        if(animation.Data.ContainsKey(valueName))
                            break;
                        animation.Data[d.Key] = animation.Name.Remove(match.Index, match.Length);
                        animation.Data[d.Key].Insert(match.Index, animation.Data[valueName]);
                    }
                }

                //replace all %key% with keyname in animation name
                while(true) {
                    var match = VariableNameFinder.Match(animation.Name);
                    if(!match.Success)
                        break;
                    string valueName = match.Value.Trim('%');
                    if(animation.Data.ContainsKey(valueName))
                        break;
                    animation.Name = animation.Name.Remove(match.Index, match.Length);
                    animation.Name.Insert(match.Index, animation.Data[valueName]);
                }
            }
            Animations.Add(animation);
        }

        // gets an type by the name as string (Type.GetType() often doesn't worked)
        public static Type ByName( string name ) {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
                var tt = assembly.GetType(name);
                if(tt != null) {
                    return tt;
                }
            }

            return null;
        }

        //runs a generic method with a type as variable (here mostly used for JObject.ToObject<T>())
        public static dynamic RunMethodWithType( Type genericType, object o, string method ) {
            var t = o.GetType();
            var m = t.GetMethod(method, new Type[0]);
            var hurr = m.MakeGenericMethod(genericType);
            return hurr.Invoke(o, new object[0]);
        }

        readonly Dictionary<int , string> err_messages = new Dictionary<int , string>( ) {
            {400, "Bad Request" },
            {401, "Unauthorized" },
            {403, "Forbidden" },
            {404, "path not Found" },

            {502, "Internal Server Error" },
        };

        //creates an error page (gets the error message from error code) and convert to byte array
        private byte[] Err( int code, string details ) {
            string responseString = Encoding.UTF8.GetString( ErrorPage.Cache.Value );
            responseString = responseString.Replace("%code%", code.ToString());
            responseString = responseString.Replace("%message%", err_messages[code]);
            responseString = responseString.Replace("%details%", details);
            return Encoding.UTF8.GetBytes(responseString);
        }

        //returns a page to redirect the user
        private byte[] Redirect( string url ) => Encoding.UTF8.GetBytes("<html><head><meta http-equiv=\"refresh\" content=\"0; URL=" + url + "\"></head></html>");

        //extracts the GET data from an request
        private Dictionary<string, string> GetGetData( HttpListenerRequest request ) {
            string query= request.Url.Query;
            query = query[1..]; // skip the first because its always "method" in this case
            string[] postDataStringSplitted = query.Split( '&' );
            var PostData = new Dictionary<string , string>( );
            foreach(string s in postDataStringSplitted) {
                string[] splitted = s.Split( '=' );
                PostData.Add(HttpUtility.UrlDecode(splitted[0]), HttpUtility.UrlDecode(splitted[1]));
            }

            return PostData;
        }


        // class to save a command, which can used while the program is running
        internal class ConsoleCommand {
            public Regex Regex { get; set; }
            public Action<string[]> Action { get; set; }

            public ConsoleCommand( Regex regex, Action<string[]> action ) {
                this.Regex = regex;
                this.Action = action;
            }
        }

        //takes any input in console and runs commands from it
        private void ConsoleInputThread() {
            var commands = new List<ConsoleCommand>(){
                new ConsoleCommand(new Regex(@"install [a-zA-Z0-9\-\/\:\#\.\@]+"), Install)
            };

            while(true) {
                string line = Console.ReadLine();
                var command = commands.Find(x => x.Regex.Match(line).Length == line.Length);
                var args = line.Split(' ').ToList();
                args.RemoveAt(0);
                command.Action.Invoke(args.ToArray());
            }
        }

        // method for the "install" command
        private void Install( string[] obj ) {
            AnimationDownloader.AnimationDownloader.Download(obj[0]);
            Program.Log("install", "finished", ConsoleColor.Green);
        }

        // class to save something that need to get logged (text and color of the text)
        internal class LogItem {
            public string Line { get; set; }
            public ConsoleColor Color { get; set; }
        }

        static readonly List<LogItem> Logs = new List<LogItem>();

        // converts a message to a Log message type to log it when nothing else is their to log 
        // (to prevent multithreading problems with logging from diffrent threads)
        // also format message
        public static void Log( string type, string message, ConsoleColor color ) {
            string line = $"{DateTime.Now:G} [{type}] {message}\n";
            Logs.Add(new LogItem() { Color = color, Line = line });
        }

        //loop to log messages in an diffrent thread if something need to get logged
        public static void StartLogger() => new Thread(() => {
            while(true) {
                try {
                    while(Logs.Count == 0)
                        Thread.Sleep(30);
                    var item = Logs[0];
                    Logs.RemoveAt(0);

                    var cc = Console.ForegroundColor;
                    Console.ForegroundColor = item.Color;
                    Console.Write(item.Line);
                    Console.ForegroundColor = cc;
                } catch(Exception e) {
                    var cc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[logger][error] " + e);
                    Console.ForegroundColor = cc;
                }
            }
        }).Start();
    }

}
