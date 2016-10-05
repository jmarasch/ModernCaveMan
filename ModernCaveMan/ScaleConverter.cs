using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ModernCaveMan {
    class ScaleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var resolutionScale = (int)DisplayInformation.GetForCurrentView().ResolutionScale / 100.0;
            var baseValue = int.Parse(parameter as string);
            var scaledValue = baseValue * resolutionScale;
            if (targetType == typeof(GridLength))
                return new GridLength(scaledValue);
            return scaledValue;
            }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var resolutionScale = (int)DisplayInformation.GetForCurrentView().ResolutionScale / 100.0;
            var baseValue = int.Parse(parameter as string);
            var scaledValue = baseValue * resolutionScale;
            if (targetType == typeof(GridLength))
                return new GridLength(scaledValue);
            return scaledValue;
            }
        }
    }
