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
                var gll = await BridgeApiClient.Current.GetGroupLightListAsync();
                LightsBindingContext.GroupLights = new ObservableCollection<BridgeApiClient.GroupLightList>(gll);

            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
            }
        }

        class LightsViewModel : INotifyPropertyChanged
        {
            private bool updating;
            public bool Updating => updating;

            private ObservableCollection<BridgeApiClient.Light> lights;
            public ObservableCollection<BridgeApiClient.Light> Lights
            {
                get
                {
                    return lights;
                }
                set
                {
                    lights = value;
                    OnPropertyChanged();
                }
            }

            private ObservableCollection<BridgeApiClient.Group> groups;
            public ObservableCollection<BridgeApiClient.Group> Groups
            {
                get
                {
                    return groups;
                }
                set
                {
                    groups = value;
                    OnPropertyChanged();
                }
            }

            private ObservableCollection<BridgeApiClient.GroupLightList> grouplights;
            public ObservableCollection<BridgeApiClient.GroupLightList> GroupLights
            {
                get
                {
                    return grouplights;
                }
                set
                {
                    grouplights = value;
                    OnPropertyChanged();
                }
            }


            public LightsViewModel()
            {
                Lights = new ObservableCollection<BridgeApiClient.Light>();
                Groups = new ObservableCollection<BridgeApiClient.Group>();
                GroupLights = new ObservableCollection<BridgeApiClient.GroupLightList>();
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

        private void GroupSwitch_Toggled(object sender, ToggledEventArgs e)
        {

        }

        private async void LightSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (LightsBindingContext.Updating)
                return;

            BridgeApiClient.Light l = (BridgeApiClient.Light)((Switch)sender).BindingContext;

            BridgeApiClient.LightState ls = new BridgeApiClient.LightState
            {
                On = e.Value
            };

            await BridgeApiClient.Current.SetLightStateAsync(l.Id, ls);
        }

        private void GroupSwitch_BindingContextChanged(object sender, EventArgs e)
        {
            // we are not invoking Switch_Toggled directly from XAML for a reason!
            // that is in order to not fire it when Binding.
            if (!(sender is Switch s)) return;
                s.Toggled += GroupSwitch_Toggled;
        }

        private void LightSwitch_BindingContextChanged(object sender, EventArgs e)
        {
            // we are not invoking Switch_Toggled directly from XAML for a reason!
            // that is in order to not fire it when Binding.
            if (!(sender is Switch s)) return;
                s.Toggled += LightSwitch_Toggled;
        }
    }
}