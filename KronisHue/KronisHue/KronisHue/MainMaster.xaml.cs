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
    public partial class MainMaster : ContentPage
    {
        public ListView ListView;

        public MainMaster()
        {
            InitializeComponent();

            BindingContext = new MainMasterViewModel();
            ListView = MenuItemsListView;
        }

        class MainMasterViewModel : NotifyChangeBase
        {
            public ObservableCollection<MainMenuItem> MenuItems { get; set; }
            
            public MainMasterViewModel()
            {
                MenuItems = new ObservableCollection<MainMenuItem>(new[]
                {
                    new MainMenuItem { Id = 0, Title = "Lights", TargetType=typeof(LightsPage) },
                    new MainMenuItem { Id = 1, Title = "Settings", TargetType=typeof(SettingsPage) },
                });
            }
        }
    }
}