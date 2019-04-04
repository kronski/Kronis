using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        protected override void OnStart()
        {
            InitBridge();
        }

        private void InitBridge()
        {
            BridgeApiClient.Init();
            if (Properties.TryGetValue("Username", out object value) && value is string v)
                BridgeApiClient.Current.Username = v;
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += FindHueBridgeViaUpnpInBackground;
            backgroundWorker.RunWorkerAsync();

            BackgroundWorker backgroundWorker2 = new BackgroundWorker();
            backgroundWorker2.DoWork += FindHueBridgeViaMeetHueInBackground;
            backgroundWorker2.RunWorkerAsync();

        }

        private async void FindHueBridgeViaUpnpInBackground(object sender, DoWorkEventArgs e)
        {
            await BridgeLocator.FindHueBridgeViaUpnp(
                bridge =>
                {
                    AddBridge(bridge);
                }, null
            );
        }

        private async void FindHueBridgeViaMeetHueInBackground(object sender, DoWorkEventArgs e)
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
        }
        
        private void AddBridge(string bridge)
        {
            if (!BridgeApiClient.Current.Bridges.Contains(bridge))
            {
                BridgeApiClient.Current.Bridges.Add(bridge);

                if(BridgeApiClient.Current.IP ==null)
                    BridgeApiClient.Current.IP = bridge;
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
