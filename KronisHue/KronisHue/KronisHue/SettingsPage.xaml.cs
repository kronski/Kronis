using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KronisHue
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage
	{
		public SettingsPage ()
		{
			InitializeComponent ();
        }

        protected override void OnAppearing()
        {
            var list = BridgeApiClient.Current.Bridges.OrderBy(x => x).ToList();
            AvailableBridges.ItemsSource = list;
            AvailableBridges.SelectedIndex = list.IndexOf(BridgeApiClient.Current.IP);

        }

        private async void Register_Click(object sender, EventArgs e)
        {
            if (BridgeApiClient.Current.Username == null)
            {
                BridgeApiClient.Current.Username = await BridgeApiClient.Current.NewDeveloperAsync();
                if (BridgeApiClient.Current.Username != null)
                {
                    App.Current.Properties["Username"] = BridgeApiClient.Current.Username;
                }
            }
        }

        private void AvailableBridges_SelectedIndexChanged(object sender, EventArgs e)
        {
            BridgeApiClient.Current.IP = AvailableBridges.SelectedItem as string;
        }
    }
}