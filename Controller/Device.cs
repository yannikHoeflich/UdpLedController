using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Controller {
    public class Device {
        public Color[] Leds { get; set; }
        public string Ip { get; }

        public Device( string iP ) {
            this.Ip = iP;
        }

        // send an array with colors to the device to set the colors their
        public void Set() {
            var udpClient = new UdpClient( );

            byte[] sendBytes = new byte[(this.Leds.Length * 3) + 1];
            sendBytes[0] = 255;
            for(int i = 0; i < this.Leds.Length; i++) {
                sendBytes[(i * 3) + 1] = this.Leds[i].R;
                sendBytes[(i * 3) + 2] = this.Leds[i].G;
                sendBytes[(i * 3) + 3] = this.Leds[i].B;
            }
            udpClient.Send(sendBytes, sendBytes.Length, this.Ip, 4210);
            udpClient.Close();
        }

        // init device (gets amount of leds from the device and create data that is needed)
        public void Init() {
            var udpClient = new UdpClient( );

            byte[] sendBytes = new byte[] { 0 };
            udpClient.Send(sendBytes, sendBytes.Length, this.Ip, 4210);

            var RemoteIpEndPoint = new IPEndPoint( IPAddress.Any , 0 );
            udpClient.Client.ReceiveTimeout = 2000;
            try {
                byte[] receiveBytes = udpClient.Receive( ref RemoteIpEndPoint );

                this.Leds = new Color[BitConverter.ToInt16(receiveBytes, 0)];

            } catch(Exception e) {
                if(e is ArgumentException || e is ArgumentNullException || e is ArgumentOutOfRangeException) {
                    throw new InvalidDataException("Device answered with a wrong byte array", e);
                } else {
                    throw e;
                }
            }
            udpClient.Close();
        }

        public override bool Equals( object obj ) => obj is Device device &&
                   this.Ip == device.Ip;

        public override int GetHashCode() => HashCode.Combine(this.Ip);
    }
}
