using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace KronisHue
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new Main();
        }

        public static App Curr => (App)Current;

        public event EventHandler<string> OnBridgeFound = delegate { };

        protected override void OnStart()
        {
            InitBridge();
            InitAppLink();
        }

        private void InitAppLink()
        {
            try
            {
                var url = $"https://kronishue.azurewebsites.net/app";

                var entry = new AppLinkEntry
                {
                    Title = "KronisHue",
                    Description = "KronisHue",
                    AppLinkUri = new Uri(url, UriKind.RelativeOrAbsolute),
                    IsLinkActive = true,
                    Thumbnail = ImageSource.FromFile("Icon.png")
                };

                entry.KeyValues.Add("contentType", "Session");
                entry.KeyValues.Add("appName", "Kronis Hue");
                entry.KeyValues.Add("companyName", "DataPolarna AB");

                Application.Current.AppLinks.RegisterLink(entry);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void InitBridge()
        {
            BridgeApiClient.Init();
            if (Properties.TryGetValue("Username", out object value) && value is string v)
                BridgeApiClient.Current.Username = v;

            Task.Run(async () =>
            {
                await BridgeLocator.FindHueBridgeViaUpnp(
                    bridge =>
                    {
                        AddBridge(bridge);
                    }, null
                );
            });

            Task.Run(async () =>
            {
                try
                {
                    var bridges = await BridgeLocator.FindHueBridgeViaMeetHue();
                    foreach (string bridge in bridges)
                        AddBridge(bridge);
                }
                catch
                {
                }
            });
        }

        private void AddBridge(string bridge)
        {
            if (BridgeApiClient.Current?.Bridges.Contains(bridge) == false)
            {
                BridgeApiClient.Current.Bridges.Add(bridge);

                if(BridgeApiClient.Current.IP == null)
                {
                    Properties["LastIP"] = bridge;

                    BridgeApiClient.Current.IP = bridge;
                    OnBridgeFound(this,bridge);
                }
            }
        }

        protected override void OnSleep()
        {
            BridgeApiClient.Current = null;
        }

        protected override void OnResume()
        {
            InitBridge();
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);
        }
    }
}
