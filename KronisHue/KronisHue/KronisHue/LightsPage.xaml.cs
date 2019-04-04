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
    public partial class LightsPage : ContentPage
    {
        public LightsPage()
        {
            InitializeComponent();
        }

        public BridgeApiClient.Light[] Lights { get; set; } = new BridgeApiClient.Light[] { };

        async protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var l = await BridgeApiClient.Current.GetLightsWithGroupAsync();
                Lights = l;
            }
            catch
            {
            }

            
        }

    }
}