using System.Drawing;

namespace Hardware_SDK {
    interface IDevice {
        int LedCount { get; }

        void SetLed( int index, Color color );
        void Update();
        void Init();
        void Deactivate();
        bool Active { get; }
    }
}
