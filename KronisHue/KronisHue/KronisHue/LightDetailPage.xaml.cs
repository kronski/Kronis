using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KronisHue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LightDetailPage : ContentPage
    {
        public LightDetailPage(Light light)
        {
            InitializeComponent();

            
            BindingContext = new DetailsViewModel(light);
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public class LightDetailProperty
        {
            public string Property { get; set; }
            public object Value { get; set; }
        }

        class DetailsViewModel : INotifyPropertyChanged
        {

            private ObservableCollection<LightDetailProperty> properties;
            public ObservableCollection<LightDetailProperty> Properties
            {
                get
                {
                    return properties;
                }
                set
                {
                    properties = value;
                    OnPropertyChanged();
                }
            }
            public DetailsViewModel(Light light)
            {
                ObservableCollection<LightDetailProperty>  p = new ObservableCollection<LightDetailProperty>();

                PropertyInfo[] pia = typeof(Light).GetProperties();
                foreach (PropertyInfo pi in pia)
                {
                    var obj = pi.GetValue(light);

                    p.Add(new LightDetailProperty()
                    {
                        Property = pi.Name,
                        Value = obj
                    });
                }

                pia = typeof(LightState).GetProperties();
                foreach (PropertyInfo pi in pia)
                {
                    var obj = pi.GetValue(light.State);

                    p.Add(new LightDetailProperty()
                    {
                        Property = pi.Name,
                        Value = obj
                    });
                }

                Properties = p;
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