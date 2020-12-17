using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Hardware_SDK {
    class Program {
        const int udpPort = 4210;
        static CueDevice Cue;
        static RazerDevice Razer;
        static HueDevice Hue;

        static readonly List<IDevice> Devices = new List<IDevice>();
        private static void Main( string[] args ) {
#if !DEBUG
            Thread.Sleep(TimeSpan.FromMinutes(2));
#endif
            // Init
            Cue = new CueDevice();
            Cue.Init();
            Devices.Add(Cue);
            Console.WriteLine("Cue init finished");


            Razer = new RazerDevice();
            Razer.Init();
            Devices.Add(Razer);
            Console.WriteLine("Razer init finished");


            Hue = new HueDevice {
                UsedLightsArea = "4"
            };
            Hue.Init();
            Devices.Add(Hue);
            Console.WriteLine("Hue init finished");


            // Notify Icon

            var contextMenu = new ContextMenu();

            var CorsairActiveButton = new MenuItem {
                Index = 0,
                Text = "Corsair",
                Checked = true
            };
            CorsairActiveButton.Click += CorsairActiveButton_Click;

            var RazerActiveButton = new MenuItem {
                Index = 1,
                Text = "Razer",
                Checked = true
            };
            RazerActiveButton.Click += RazerActiveButton_Click;

            var HueActiveButton = new MenuItem {
                Index = 2,
                Text = "Philips Hue",
                Checked = true
            };
            HueActiveButton.Click += HueActiveButton_Click;

            var exitButton = new MenuItem {
                Index = 3,
                Text = "E&xit"
            };
            exitButton.Click += MenuItem_Click;


            contextMenu.MenuItems.AddRange(
                    new MenuItem[] { CorsairActiveButton, RazerActiveButton, HueActiveButton, exitButton });
            var notifIcon = new NotifyIcon {
                ContextMenu = contextMenu,

                Icon = new Icon("appicon.ico"),
                Text = "SDK Controller",

                Visible = true
            };

            //udp host
            new Thread(() => {
                byte[] data = new byte[1024];
                var ipep = new IPEndPoint(IPAddress.Any, udpPort);
                var newsock = new UdpClient(ipep);

                Console.WriteLine("Udp socket Started");

                var sender = new IPEndPoint(IPAddress.Any, 0);

                byte[] buffer = new byte[0];
                while(true) {
                    data = newsock.Receive(ref sender);

                    switch(data[0]) {
                        case 0:
                        buffer = BitConverter.GetBytes((short) Devices.Sum(x => x.LedCount));
                        break;
                        case 255:
                        int colorIndex = 1;
                        foreach(var d in Devices) {
                            int ledIndex = 0;
                            if(!d.Active) {
                                colorIndex += d.LedCount;
                                continue;
                            }
                            while(ledIndex < d.LedCount && colorIndex < data.Length) {
                                d.SetLed(ledIndex, Color.FromArgb(data[colorIndex + 0], data[colorIndex + 1], data[colorIndex + 2]));
                                colorIndex += 3;
                                ledIndex++;
                            }
                            d.Update();
                            if(colorIndex >= data.Length)
                                break;
                        }

                        break;
                    }



                    newsock.Send(buffer, buffer.Length, sender);
                }
            }).Start();

            Application.Run();
        }


        static bool CorsairActive = true;
        static bool RazerActive = true;
        static bool HueActive = true;
        private static void CorsairActiveButton_Click( object sender, EventArgs e ) {
            CorsairActive = !CorsairActive;
            ((MenuItem) sender).Checked = CorsairActive;
            if(CorsairActive)
                Cue.Init();
            else
                Cue.Deactivate();
        }
        private static void RazerActiveButton_Click( object sender, EventArgs e ) {
            RazerActive = !RazerActive;
            ((MenuItem) sender).Checked = RazerActive;
            if(RazerActive)
                Razer.Init();
            else
                Razer.Deactivate();
        }
        private static void HueActiveButton_Click( object sender, EventArgs e ) {
            HueActive = !HueActive;
            ((MenuItem) sender).Checked = HueActive;
            if(HueActive)
                Hue.Init();
            else
                Hue.Deactivate();
        }

        private static void MenuItem_Click( object sender, EventArgs e ) => Environment.Exit(0);
    }
}
