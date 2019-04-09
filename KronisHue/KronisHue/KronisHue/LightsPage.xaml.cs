using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

            LightsBindingContext = new LightsViewModel(this);
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

            if (BridgeApiClient.Current?.IP != null)
                await PopulateLightsAsync();

        }

        private async Task PopulateLightsAsync()
        {
            try
            {
                var gll = await BridgeApiClient.Current.GetGroupLightListAsync();
                LightsBindingContext.GroupLights = new ObservableCollection<GroupLightList>(gll);
                ResetError();

            }
            catch
            {
                ShowError("Unable to get lights");
            }
        }

        private void ResetError()
        {
            ErrorLabel.IsVisible = false;
        }

        private void ShowError(string error)
        {
            ErrorLabel.Text = error;
            ErrorLabel.IsVisible = true;
        }

        class LightsViewModel : NotifyChangeBase
        {
            private readonly LightsPage page;

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


            public LightsViewModel(LightsPage page)
            {
                this.page = page;
                GroupLights = new ObservableCollection<GroupLightList>();
            }

            private bool _isRefreshing = false;
            public bool IsRefreshing
            {
                get { return _isRefreshing; }
                set
                {
                    _isRefreshing = value;
                    OnPropertyChanged(nameof(IsRefreshing));
                }
            }

            public ICommand RefreshCommand
            {
                get
                {
                    return new Command(async () =>
                    {
                        IsRefreshing = true;

                        await page.PopulateLightsAsync();

                        IsRefreshing = false;
                    });
                }
            }
        }

        private async void GroupSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            GroupLightList gll = (GroupLightList)((Switch)sender).BindingContext;

            if (gll == null || gll.Group.Action.On == e.Value)
                return;

            GroupAction ga = new GroupAction
            {
                On = e.Value
            };

            try
            {
                await BridgeApiClient.Current.SetGroupActionAsync(gll.Group, ga);
                ResetError();
            }
            catch
            {
                ShowError("Unable to update group");
            }


        }

        private async void LightSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            Light l = (Light)((Switch)sender).BindingContext;
            if (l == null || l.State.On == e.Value)
                return;

            LightState ls = new LightState
            {
                On = e.Value
            };
            try
            {
                await BridgeApiClient.Current.SetLightStateAsync(l, ls);
                ShowError("Unable to update group");
            }
            catch
            {
                ResetError();
            }
        }

        private async void DetailsButton_Clicked(object sender, EventArgs e)
        {
            if(LightsListView.SelectedItem is Light light)
            {
                var detailpage = new LightDetailPage(light);
                var navpage = new NavigationPage(detailpage)
                {
                    Title = light.Name
                };

                await Navigation.PushAsync(navpage);
            }
        }
    }
}