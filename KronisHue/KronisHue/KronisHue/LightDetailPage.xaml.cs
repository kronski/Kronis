using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KronisHue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LightDetailPage : ContentPage
    {
        readonly Light currentlight;
        Dictionary<View, string> controls = new Dictionary<View, string>();

        public LightDetailPage(Light light)
        {
            currentlight = light;
            InitializeComponent();
            BuildGridContent();
            FillTable();
        }

        private void FillTable()
        {
            var leftm = new Thickness(10, 0, 0, 0);
            Grid.RowDefinitions.Clear();
            Grid.Children.Clear();
            Grid.RowSpacing = 0;
            Grid.ColumnSpacing = 0;
            Thickness lm = new Thickness(3, 3, 0, 3);
            Thickness rm = new Thickness(0, 3, 3, 3);
            foreach (var c in GridContent)
            {
                Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var label = new Label()
                {
                    Margin = leftm,
                    HorizontalOptions = LayoutOptions.Fill,
                    Text = c.Property,
                    //BackgroundColor = c.BackgroundColor,
                };
                label.SizeChanged += Label_SizeChanged;


                Grid.Children.Add(
                    new Frame()
                    {
                        Padding = 2,
                        Margin =rm,
                        BorderColor = Color.Silver,
                        Content = label
                    }, 0, c.Row);

                View control = null;
                if (c?.ControlAttribute?.ControlType == typeof(Switch))
                {
                    control = new Switch()
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        IsToggled = Convert.ToBoolean(c.Value),
                    };
                    ((Switch)control).Toggled += Control_Toggled;

                    controls.Add(control, c.Property);
                }
                else if (c?.ControlAttribute?.ControlType == typeof(Slider))
                {
                    control = new Slider()
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        Minimum = c.ControlAttribute.Min,
                        Maximum = c.ControlAttribute.Max,
                        Value = Convert.ToDouble(c.Value),
                        MinimumTrackColor = Color.Green,
                        MaximumTrackColor = Color.Red,
                        //BackgroundColor = c.BackgroundColor
                    };

                    ((Slider)control).ValueChanged += Control_ValueChanged;
                    controls.Add(control, c.Property);
                }
                else
                {
                    control = new Label()
                    {
                        Margin = leftm,
                        HorizontalOptions = LayoutOptions.Fill,
                        Text = c.Value?.ToString(),
                        //BackgroundColor = c.BackgroundColor
                    };
                }



                if (control != null)
                {
                    Grid.Children.Add(
                        new Frame()
                        {
                            Padding = 2,
                            BorderColor = Color.Silver,
                            Margin = rm,
                            Content = control
                        }, 1, c.Row
                    );
                }
            }
        }

        private async void Control_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (controls.TryGetValue(sender as View, out var controlname))
            {
                try
                {
                    switch (controlname)
                    {
                        case "State.Bri":
                        case "State.Hue":
                            await BridgeApiClient.Current.SetLightStateAsync(currentlight, new LightState()
                            {
                                Bri = (controlname== "State.Bri") ? (long?)Convert.ToInt64(e.NewValue) : null,
                                Hue = (controlname == "State.Hue") ? (long?)Convert.ToInt64(e.NewValue) : null,
                            });
                            break;
                    }
                }
                catch
                {

                }
            }
        }

        private async void Control_Toggled(object sender, ToggledEventArgs e)
        {
            if (controls.TryGetValue(sender as View, out var controlname))
            {
                try
                {
                    switch (controlname)
                    {
                        case "State.On":
                            await BridgeApiClient.Current.SetLightStateAsync(currentlight, new LightState()
                            {
                                On = e.Value
                            });
                            break;
                    }
                }
                catch
                {

                }
            }
        }

        private void Label_SizeChanged(object sender, EventArgs e)
        {
            /*Label label = (Label)sender;
            int row = RowDict[label];
            Grid.RowDefinitions[row].Height = label.Height;*/
        }


        public class LightDetailProperty
        {
            public int Row { get; set; }
            public string Property { get; set; }
            public object Value { get; set; }
            public BindingControlAttribute ControlAttribute { get; set; }

            public Color BackgroundColor { get => Row % 2 == 0 ? Color.LightGray : Color.White; }
        }

        private ObservableCollection<LightDetailProperty> gridContent;
        public ObservableCollection<LightDetailProperty> GridContent
        {
            get
            {
                return gridContent;
            }
            set
            {
                gridContent = value;
                OnPropertyChanged();
            }
        }

        public void BuildGridContent()
        {
            ObservableCollection<LightDetailProperty>  p = new ObservableCollection<LightDetailProperty>();

            Newtonsoft.Json.JsonSerializer ser = new Newtonsoft.Json.JsonSerializer();

            void BuildProperties(object baseobj, string basename)
            {
                List<(object obj, string basepath)> laterobjects = null;
                PropertyInfo[] pia = baseobj.GetType().GetProperties();
                foreach (PropertyInfo pi in pia)
                {
                    if (!pi.PropertyType.IsPublic)
                        continue;
                    BindingControlAttribute bca = pi.GetCustomAttribute<BindingControlAttribute>();

                    var obj = pi.GetValue(baseobj);
                    if (obj == null)
                    {
                        p.Add(new LightDetailProperty()
                        {
                            Property = basename + pi.Name,
                            Value = null,
                            ControlAttribute = bca,
                            Row = p.Count
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
                            Value = json,
                            ControlAttribute = bca,
                            Row = p.Count
                        });
                    }
                    else if (pi.PropertyType.IsClass && pi.PropertyType != typeof(string))
                    {
                        if (laterobjects == null)
                            laterobjects = new List<(object, string)>();
                        laterobjects.Add((obj, basename + pi.Name + "."));
                    }
                    else
                    {
                        p.Add(new LightDetailProperty()
                        {
                            Property = basename + pi.Name,
                            Value = obj,
                            ControlAttribute = bca,
                            Row = p.Count
                        });
                    }
                }

                laterobjects?.ForEach(obj => {
                    BuildProperties(obj.obj, obj.basepath);
                });
            }

            BuildProperties(currentlight,"");

            GridContent = p;
        }

        
    }
}