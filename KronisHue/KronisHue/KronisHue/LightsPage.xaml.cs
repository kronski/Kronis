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
                LightsBindingContext.GroupLights = new ObservableCollection<GroupLightList>(gll);

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

            private ObservableCollection<Light> lights;
            public ObservableCollection<Light> Lights
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

            private ObservableCollection<Group> groups;
            public ObservableCollection<Group> Groups
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

            private ObservableCollection<GroupLightList> grouplights;
            public ObservableCollection<GroupLightList> GroupLights
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
                Lights = new ObservableCollection<Light>();
                Groups = new ObservableCollection<Group>();
                GroupLights = new ObservableCollection<GroupLightList>();
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

        private async void GroupSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            GroupLightList gll = (GroupLightList)((Switch)sender).BindingContext;
            if (gll.Group.Action.On == e.Value)
                return;

            GroupAction ga = new GroupAction
            {
                On = e.Value
            };

            await BridgeApiClient.Current.SetGroupActionAsync(gll.Group, ga);
        }

        private async void LightSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            Light l = (Light)((Switch)sender).BindingContext;
            if (l.State.On == e.Value)
                return;

            LightState ls = new LightState
            {
                On = e.Value
            };

            await BridgeApiClient.Current.SetLightStateAsync(l, ls);
        }
    }
}