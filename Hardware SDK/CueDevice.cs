using System.Collections.Generic;
using System.Drawing;

using CUE.NET;
using CUE.NET.Brushes;
using CUE.NET.Devices.Generic;
using CUE.NET.Devices.LightingNodePro;
using CUE.NET.Exceptions;

namespace Hardware_SDK {
    class CueDevice : IDevice {
        private readonly SingleLedBrush Brush = new SingleLedBrush();
        CorsairLightingNodePro Node;
        public int LedCount { get; private set; }

        public CueDevice() {
            CueSDK.Initialize();
            Node = CueSDK.LightingNodeProSDK;
            if(Node == null)
                throw new WrapperException("No Node found");
        }

        public void Init() {

            foreach(var _ in Node.Leds) {
                this.LedCount++;
            }

            for(int i = 0; i < this.LedCount; i++) {
                Brush.Colors.Add(Color.Black);
            }

            Node.Brush = Brush;

            Node.Update();
            Active = true;
        }

        public void SetLed( int index, Color color ) {
            if(Active)
                Brush.Colors[index] = color;
            else
                throw new System.Exception("Device is not active");
        }
        public void Update() {
            if(Active)
                Node.Update();
            else
                throw new System.Exception("Device is not active");
        }
        public bool Active { get; private set; } = false;
        public void Deactivate() {
            if(Active) {
                Node.SyncColors();
            }
            Active = false;
        }
    }

    public class SingleLedBrush : AbstractBrush {
        public List<Color> Colors { get; set; }

        public SingleLedBrush() : base() {
            this.Colors = new List<Color>();
        }

        protected override CorsairColor GetColorAtPoint( RectangleF rectangle, BrushRenderTarget renderTarget ) => this.Colors[(int) renderTarget.LedId - 200];
    }
}
