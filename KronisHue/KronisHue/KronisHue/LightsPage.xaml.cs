using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KronisHue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LightsPage : ContentPage
    {
        private LightsViewModel LightsBindingContext;
        public LightsPage()
        {
            InitializeComponent();

            LightsBindingContext = new LightsViewModel();
            BindingContext = LightsBindingContext;

            App.Curr.OnBridgeFound += Curr_OnBridgeFound;
        }

        private async void Curr_OnBridgeFound(object sender, string e)
        {
            await PopulateLightsAsync();
        }

        

        async protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BridgeApiClient.Current.IP != null)
                await PopulateLightsAsync();

        }

        private async Task PopulateLightsAsync()
        {
            try
            {
                LightsBindingContext.Lights = new ObservableCollection<BridgeApiClient.Light>(await BridgeApiClient.Current.GetLightsWithGroupAsync());
                
            }
            catch(Exception ex)
            {
                ErrorLabel.Text = ex.Message;
            }
        }

        class LightsViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<BridgeApiClient.Light> Lights { get; set; }

            public LightsViewModel()
            {
                Lights = new ObservableCollection<BridgeApiClient.Light>();
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}