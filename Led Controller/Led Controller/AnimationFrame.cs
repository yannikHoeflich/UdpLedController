using System;

using Xamarin.Forms;

namespace Led_Controller {
    public class AnimationFrame : Frame {
        private event EventHandler Click;
        public ulong AnimationId { get; set; }

        public AnimationFrame() : base() {
            SetBackColor(false);
        }

        public AnimationFrame( bool selected ) : base() {
            SetBackColor(selected);
        }

        public void SetBackColor( bool Selected ) {
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            if(currentTheme == OSAppTheme.Dark) {
                if(Selected) {
                    this.BackgroundColor = Color.DarkGreen;
                    this.BorderColor = Color.Green;
                } else {
                    this.BackgroundColor = Color.DimGray;
                    this.BorderColor = Color.Gray;
                }
            } else {
                if(Selected) {
                    this.BackgroundColor = Color.Green;
                    this.BorderColor = Color.DarkGreen;
                } else {
                    this.BackgroundColor = Color.LightGray;
                    this.BorderColor = Color.Gray;
                }
            }
        }

        public string Name {
            get; set;
        }

        public void DoClick() => Click?.Invoke(this, null);

        public event EventHandler Clicked{
            add{
                lock (this){
                    Click += value;

                    var g = new TapGestureRecognizer();

                    g.Tapped += (s, e) => Click?.Invoke(s, e);

                    this.GestureRecognizers.Add(g);
                }
            }
            remove{
                lock (this){
                    Click -= value;

                    this.GestureRecognizers.Clear();
                }
            }
        }
    }
}
