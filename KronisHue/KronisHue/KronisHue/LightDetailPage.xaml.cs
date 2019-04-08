using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        class DetailsViewModel : NotifyChangeBase
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

                Newtonsoft.Json.JsonSerializer ser = new Newtonsoft.Json.JsonSerializer();

                void BuildProperties(object baseobj, string basename)
                {
                    PropertyInfo[] pia = baseobj.GetType().GetProperties();
                    foreach (PropertyInfo pi in pia)
                    {
                        if (!pi.PropertyType.IsPublic)
                            continue;
                        Debug.WriteLine(basename + pi.Name);
                        var obj = pi.GetValue(baseobj);
                        if (obj == null)
                        {
                            p.Add(new LightDetailProperty()
                            {
                                Property = basename + pi.Name,
                                Value = null
                            });
                        }
                        else if (pi.PropertyType == typeof(Light))
                        {
                        }
                        else if (pi.PropertyType.IsArray || pi.PropertyType.Name == "List`1")
                        {
                            string json = JsonConvert.SerializeObject(obj);

                            p.Add(new LightDetailProperty()
                            {
                                Property = basename + pi.Name,
                                Value = json
                            });
                        }
                        else if (pi.PropertyType.IsClass && pi.PropertyType != typeof(string))
                        {
                            BuildProperties(obj, basename + pi.Name+".");
                        }
                        else
                        {
                            p.Add(new LightDetailProperty()
                            {
                                Property = basename + pi.Name,
                                Value = obj?.ToString()
                            });
                        }
                    }
                }

                BuildProperties(light,"");

                

                Properties = p;
            }
        }

    }
}