using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace KronisHue
{
    public class PropertyView : ContentView
    {
        public static readonly BindableProperty ControlAttributeProperty =
            BindableProperty.Create("ControlAttribute", typeof(BindingControlAttribute), typeof(PropertyView), null);
        public static readonly BindableProperty PropertyObjectProperty =
            BindableProperty.Create("PropertyObject", typeof(object), typeof(PropertyView), null);

        public PropertyView()
        {
        }
                
        public BindingControlAttribute ControlAttribute
        {
            get { return (BindingControlAttribute)GetValue(ControlAttributeProperty); }
            set { SetValue(ControlAttributeProperty, value); }
        }


        public object PropertyObject
        {
            get { return GetValue(PropertyObjectProperty); }
            set { SetValue(PropertyObjectProperty, value); }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "ControlAttribute" || propertyName == "PropertyObject")
            {
                Content = null;
                if (ControlAttribute?.ControlType == typeof(Switch))
                {
                    bool b = false;
                    if (PropertyObject is bool?)
                        b = (bool?)PropertyObject == true;
                    else if (PropertyObject is bool)
                        b = (bool)PropertyObject;
                    Content = new Switch()
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        IsToggled = b
                    };
                }
                else if (ControlAttribute?.ControlType == typeof(Slider))
                {
                    Content = new Slider()
                    {
                        VerticalOptions = LayoutOptions.Center,
                        Minimum = 0,
                        Maximum = 1000,
                        Value = 0
                    };
                }
                else
                {
                    Content = new Label()
                    {
                        Text = PropertyObject?.ToString(),
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Color.Black,
                        FontAttributes = FontAttributes.None,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    };
                }
            }
        }
    }
}