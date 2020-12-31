using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

using Jint;
using Jint.Runtime.Interop;

namespace Controller {
    static class StaticAnimationThreadClass {
        public static List<Thread> AnimationThreads = new List<Thread> ();
        public static Device[] Devices;
        static Color[] LastFrame = null;

        private static double dimming = 1;
        public static double Dimming {
            get => dimming; set {
                if(value > 1)
                    dimming = 1;
                else if(value < 0)
                    dimming = 0;
                else
                    dimming = value;

                if(AnimationThreads.Count == 0)
                    Update(true);
            }
        }
        public static int LedCount => Devices.Sum(x => x.Leds.Length);

        // method to start new animation
        public static void StartAnimation( Animation animation ) {
            if(Devices == null) {
                Program.Log("animation", "[error] never scanned for devices", ConsoleColor.Red);
                return;
            }

            var t = new Thread(() => {
                try {

                    //init variables to insert them into the JavaScript Engine
                    var r = new Random ();
                    var time = new Time();
                    int ledLength = LedCount;

                    //Creates JavaScript Engine and insert basic data and methods
                    Engine engine = new Engine ()
                    .SetValue ("LedLength", ledLength)
                    .SetValue ("Time", time)
                    .SetValue ("sleep", new Action<int>(ms => Sleep(ms, () => AnimationThreads.Count > 1)))
                    .SetValue ("SetColor", new Action<int, Color>(SetColor))
                    .SetValue ("Update", new Action(() => Update()))
                    .SetValue ("Log", new Action<object>(o => Program.Log("Javascript engine", o.ToString(), ConsoleColor.DarkYellow)))
                    .SetValue ("Random", new Func<int, int, int>(r.Next))
                    .SetValue ("Round", new Func<double, int>(Round))
                    .SetValue ("GetArg", new Func<string, dynamic>((key) => GetArg(animation, key)));

                    engine.SetValue("Color", TypeReference.CreateTypeReference(engine, typeof(Color)));
                    engine.SetValue("RunExternal", new Action<string>((url) => RunExternal( engine, url)));

                    // wait until other animations are exited, if more that 1 is waiting quit current thread to prevent too many threads waiting
                    while(AnimationThreads.Count > 1) {
                        Thread.Sleep(100);
                        if(AnimationThreads.Count > 2) {
                            AnimationThreads.RemoveAt(0);
                            return;
                        }
                    }

                    // reads and formats script
                    string script = File.ReadAllText("data/animations/" + animation.ScriptPath);

                    // converts constant variables that they only get declared ones
                    var regex = new Regex("const .*\\n");
                    Match match;
                    try {
                        while((match = regex.Match(script)) != null) {
                            string[] splitted = match.Value.Replace(";", "").Split(' ');
                            if(splitted.Length <= 1)
                                break;
                            string name = splitted[1];
                            string initPart = "";
                            if(splitted.Length > 2) {
                                for(int i = 2; i < splitted.Length; i++) {
                                    initPart += " " + splitted[i];
                                }
                            }

                            string newPart = "if(typeof " + name + " === 'undefined') " + "var " + name + initPart + ";";

                            script = script.Remove(match.Index, match.Length);
                            script = script.Insert(match.Index, newPart);
                        }
                    } catch(Exception) { }

                    //start animating
                    Program.Log("Javascript engine", "start animation " + animation.Name, ConsoleColor.DarkYellow);
                    do {
                        time.Update();
                        engine.Execute(script);
                        // update and wait the delay
                        Update();
                        Sleep(animation.Delay, () => AnimationThreads.Count > 1); // stop sleep if other thread is waiting
                    } while(AnimationThreads.Count <= 1 && animation.IsAnimated);
                } catch(Exception e) {
                    Program.Log("Javascript engine", "[error]" + e, ConsoleColor.DarkYellow);
                }
                AnimationThreads.RemoveAt(0); // remove ot self from thread list
            });
            t.Start();
            AnimationThreads.Add(t);
        }

        // only send data to devices if the leds have changed since exucution
        private static void Update( bool alwaysUpdate = false ) {
            if(alwaysUpdate || LastFrame == null || !LastFrame.SequenceEqual(GetLeds())) {
                foreach(Device d in Devices)
                    d.Set(Dimming);
                LastFrame = GetLeds().ToArray();
            }
        }

        //runs an external JavaScript file from other file
        private static void RunExternal( Engine engine, string AnimationName ) {

            var animation = Program.Animations.Find(x => x.Name == AnimationName);
            if(animation == null) {
                Program.Log("Javascript engine", "can't find external" + AnimationName, ConsoleColor.DarkYellow);
                return;
            }

            //loads script and formats it like on top
            string script = File.ReadAllText("data/animations/" + animation.ScriptPath);
            var regex = new Regex("const .*\\n");
            Match match;
            try {
                while((match = regex.Match(script)) != null) {
                    string[] splitted = match.Value.Replace(";", "").Split(' ');
                    if(splitted.Length <= 1)
                        break;
                    string name = splitted[1];
                    string initPart = "";
                    if(splitted.Length > 2) {
                        for(int i = 2; i < splitted.Length; i++) {
                            initPart += " " + splitted[i];
                        }
                    }

                    string newPart = "if(typeof " + name + " === 'undefined') " + "var " + name + initPart + ";";


                    script = script.Remove(match.Index, match.Length);
                    script = script.Insert(match.Index, newPart);
                }
            } catch(Exception) { }

            try {
                engine.Execute(script);
            } catch(Exception e) {
                Program.Log("Javascript engine", "[external]" + e, ConsoleColor.DarkYellow);
            }
        }

        //converts leds of all devices to one IEnumerable
        public static IEnumerable<Color> GetLeds() {
            var leds = new List<Color>();
            foreach(var d in Devices) {
                foreach(var c in d.Leds)
                    yield return c;
            }
        }

        //Scan network for devices
        public static void ScanDevices( string ip ) {
            // split example ip to get the start
            var result = new List<Device>();
            string IpStart = "";
            string[] ipParts = ip.Split( '.' );
            for(int i = 0; i < 3; i++) {
                IpStart += ipParts[i] + ".";
            }

            //loop through all devices in range of the ip and send udp ping
            byte[] sendBytes = new byte[] { 0 };
            for(int i = 2; i < 255; i++) {
                int index = i;
                var t = new Thread( ( ) => {

                    using var udpClient = new UdpClient();
                    udpClient.Client.ReceiveTimeout = 500;
                    try {
                        udpClient.Send( sendBytes , sendBytes.Length , IpStart + index , 4210 );

                        var RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IpStart + index ) , 4210);
                        byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);

                        int ledCount = BitConverter.ToInt16( receiveBytes , 0 );
                        result.Add( new Device( IpStart + index ) );
                        Program.Log("devices", $"found: { IpStart + index } with { ledCount} leds", ConsoleColor.Cyan);
                    } catch {} finally { // if anything failes (ping or convert to int16) skip device
                        udpClient.Close( );
                    }
                } );
                t.Start();
            }

            Thread.Sleep(1500); // wait for all threads to finish
            Devices = result.ToArray();

            //init all found devices
            foreach(Device d in Devices) {
                d.Init();
            }
        }

        //sets color of led from specific index
        private static void SetColor( int led, Color color ) {
            try {
                if(led >= LedCount)
                    return;
                int device = 0;
                while(Devices[device].Leds.Length < led) {
                    led -= Devices[device].Leds.Length;
                    device++;
                }
                if(Devices[device].Leds[led] != color) {
                    Devices[device].Leds[led] = color;
                }
            } catch { }
        }

        //round to int (for JavaScript engine)
        public static int Round( double d ) => (int) Math.Round(d);

        // get argument from data of animation that is running
        public static dynamic GetArg( Animation source, string key ) {
            try {
                return source.Data[key];
            } catch {
                return null;
            }
        }

        // converts timespan to milliseconds and wait that long with exit condition
        public static bool Sleep( TimeSpan timeSpan, Func<bool> exitMatch ) => Sleep(timeSpan.TotalMilliseconds, exitMatch);

        // sleeps as long a given condition is not true or for a given time in milliseconds
        public static bool Sleep( double miliseconds, Func<bool> exitMatch ) {
            DateTime endTime = DateTime.Now + TimeSpan.FromMilliseconds(miliseconds);
            bool funcValue = false;
            while(!funcValue && (endTime - DateTime.Now).Ticks > 0) {
                Thread.Sleep(1);
                funcValue = exitMatch.Invoke();
            }
            return funcValue;
        }
    }
}
