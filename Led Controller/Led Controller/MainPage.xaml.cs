using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Led_Controller {
    public partial class MainPage : ContentPage {
        private readonly HttpClient client;
        const string host = "LED Controller url";
        List<Animation>  Animations;
        ulong  CurrentAnimation;

        public MainPage() {
            InitializeComponent();
            client = new HttpClient();
            Load();
        }

        async void Load() {
            Animations = await Request<List<Animation>>("getAnimations");
            Console.WriteLine("got animations");
            var tempCurrentAnimation = await Request<Animation>("getCurrentAnimation");
            CurrentAnimation = (ulong) Animations.FindIndex(x => x.Equals(tempCurrentAnimation));
            Console.WriteLine("got current animations");


            if(Animations == null)
                return;

            ulong i = 0;
            foreach(Animation animation in Animations) {
                this.MainContent.Children.Add(CreateFrame(animation, CurrentAnimation == i, i));
                i++;
            }
        }

        async void SetAnimation( object sender, EventArgs e ) {
            ulong AnimationId = ((AnimationFrame)sender).AnimationId;
            if(!await Request<bool>("setAnimation", "animation=" + AnimationId))
                throw new AccessViolationException();

            CurrentAnimation = AnimationId;

            this.MainContent.Children.ForEach(x => {
                if(x is AnimationFrame frame) {
                    frame.SetBackColor(frame.AnimationId == CurrentAnimation);
                }
            });
        }
        async Task<T> Request<T>( string method ) {
            HttpResponseMessage response = await client.GetAsync( "http://" + host + "/api?method=" + method);

            response.EnsureSuccessStatusCode();

            string s = await response.Content.ReadAsStringAsync( );
            return JsonConvert.DeserializeObject<T>(s);
        }

        async Task<T> Request<T>( string method, string values ) {
            HttpResponseMessage response = await client.GetAsync( "http://" + host + "/api?method=" + method +"&" +values);

            response.EnsureSuccessStatusCode();

            string s = await response.Content.ReadAsStringAsync( );
            return JsonConvert.DeserializeObject<T>(s);
        }

        AnimationFrame CreateFrame( Animation animation, bool selected, ulong id ) {
            var animationFrame = new AnimationFrame(selected) {
                CornerRadius = 5 ,
                Padding = 8 ,
                AnimationId = id,
                Content = new StackLayout {
                    Children = {
                        new Label{
                            Text = animation.Name,
                            FontSize = 35,
                            FontAttributes = FontAttributes.Bold
                        },
                        new Label{
                            Text = animation.IsAnimated ? "animated" : "not animated",
                            FontSize = 14
                        }
                    }
                }
            };

            animationFrame.Clicked += SetAnimation;
            return animationFrame;
        }
    }
}
