using System;
using System.Threading;

using Corale.Colore.Core;

namespace Hardware_SDK {
    class RazerDevice : IDevice {
        public int LedCount { get; private set; }

        public bool Active { get; private set; } = false;

        private System.Drawing.Color[] colors;

        private int Width;
        private int Height;
        //height: 5
        //width: 21
        public void Init() {
            int width = 0;
            while(true) {
                try {
                    Chroma.Instance.Keyboard.SetPosition(0, width, Color.Blue);
                } catch { break; }
                Thread.Sleep(100);
                width++;
            }

            int height = 0;
            while(true) {
                try {
                    Chroma.Instance.Keyboard.SetPosition(height, 0, Color.Red);
                } catch { break; }
                Thread.Sleep(100);
                height++;
            }
            Chroma.Instance.Keyboard.Clear();

            this.Height = height;
            this.Width = width;

            this.LedCount = width * height;

            colors = new System.Drawing.Color[this.LedCount];
            for(int i = 0; i < colors.Length; i++)
                colors[i] = System.Drawing.Color.Black;

            Console.WriteLine("razer: " + width + " / " + height);

            this.Active = true;
        }
        public void SetLed( int index, System.Drawing.Color color ) {
            if(!this.Active)
                throw new System.Exception("Device is not active");

            colors[index] = color;

        }
        public void Update() {
            if(!this.Active)
                throw new System.Exception("Device is not active");

            int ledIndex = 0;
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    Chroma.Instance.Keyboard.SetPosition(y, x, GetColoreColerFromColor(colors[ledIndex]));
                    ledIndex++;
                }
                x++;
                if(x >= Width)
                    break;
                for(int y = Height - 1; y >= 0; y--) {
                    Chroma.Instance.Keyboard.SetPosition(y, x, GetColoreColerFromColor(colors[ledIndex]));
                    ledIndex++;
                }
            }
        }
        public void Deactivate() {
            if(!this.Active)
                return;
            try {
                Chroma.Instance.Uninitialize();
            } catch { }
            this.Active = false;
        }

        private Color GetColoreColerFromColor( System.Drawing.Color color ) {
            byte[] buffer = new byte[4];
            buffer[2] = color.R;
            buffer[1] = color.G;
            buffer[0] = color.B;
            return Color.FromRgb(BitConverter.ToUInt32(buffer, 0));
        }
    }
}
