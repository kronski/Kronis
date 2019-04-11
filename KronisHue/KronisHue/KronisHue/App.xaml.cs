using System;
using System.Collections.Generic;
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
    }
}
