using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;

using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.Streaming.Models;

namespace Hardware_SDK {
    class HueDevice : IDevice {
        public int LedCount { get; private set; }

        public bool Active { get; private set; }


        private Color[] colors;

        private StreamingHueClient mainClient;
        private EntertainmentLayer layer;
        private DateTime LastUpdate = DateTime.MinValue;

        public void Init() {
            if(File.Exists("bridge.json")) {
                var bridgeSaver = JsonConvert.DeserializeObject<BridgeSaver>(File.ReadAllText("bridge.json"));
                mainClient = new StreamingHueClient(bridgeSaver.Ip, bridgeSaver.AppKey, bridgeSaver.ClientKey);
            } else {
                string ip = FindIP().GetAwaiter().GetResult();

                var client = new LocalHueClient(ip);
                MessageBox.Show("Please press the button on the HueHub for the Hub: " + ip);
                var Keys = client.RegisterAsync("HardwareSDK", Environment.MachineName, true).GetAwaiter().GetResult();


                var bridgeSaver = new BridgeSaver(){Ip = ip, AppKey = Keys.Username, ClientKey = Keys.StreamingClientKey};
                File.WriteAllText("bridge.json", JsonConvert.SerializeObject(bridgeSaver));
                mainClient = new StreamingHueClient(bridgeSaver.Ip, bridgeSaver.AppKey, bridgeSaver.ClientKey);
            }

            var all = mainClient.LocalHueClient.GetEntertainmentGroups().GetAwaiter().GetResult();
            var group = all.First(x => x.Id == this.UsedLightsArea);

            var entGroup = new StreamingGroup(group.Locations);

            mainClient.Connect(group.Id).Wait();

            mainClient.AutoUpdate(entGroup, CancellationToken.None, 50);

            layer = entGroup.GetNewLayer(isBaseLayer: true);

            layer.SetState(CancellationToken.None, new RGBColor("000000"), 1, TimeSpan.FromSeconds(0));
            this.LedCount = layer.Count;
            colors = new Color[this.LedCount];
            this.Active = true;

            //new Thread(KeepAlive).Start();
        }

        public void KeepAlive() {
            while(this.Active) {
                Update();
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }

        public string UsedLightsArea { get; set; }

        public void SetLed( int index, Color color ) {
            if(!this.Active)
                throw new System.Exception("Device is not active");
            colors[index] = color;
        }

        public void Update() {
            if(!this.Active)
                throw new Exception("Device is not active");
            if((DateTime.Now - LastUpdate).TotalMilliseconds < 100)
                return;
            int colorIndex = 0;
            foreach(var light in layer) {
                light.SetColor(CancellationToken.None, new RGBColor(colors[colorIndex].R, colors[colorIndex].G, colors[colorIndex].B));
            }
            LastUpdate = DateTime.Now;
        }

        public void Deactivate() {
            if(this.Active) {
                mainClient.Close();
                mainClient = null;
            }
            this.Active = false;
        }
        async Task<string> FindIP() {
            string ip = "";

            IBridgeLocator locator = new HttpBridgeLocator();
            IEnumerable<Q42.HueApi.Models.Bridge.LocatedBridge> bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(10));

            if(bridgeIPs.Any()) {
                ip = bridgeIPs.First().IpAddress;
                Console.WriteLine("Bridge found using IP address: " + ip);
            } else {
                Console.WriteLine("Scan did not find a Hue Bridge. Try suppling a IP address for the bridge");
                return null;
            }

            return ip;
        }
    }
}
