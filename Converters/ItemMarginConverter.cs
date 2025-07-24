using Newtonsoft.Json.Linq;
using System.Globalization;

namespace SpiritualGiftsSurvey.Converters;

public class ItemMarginConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isFirst = values.Length > 0 && values[0] is true;
        bool isLast = values.Length > 1 && values[1] is true;

        double top = isFirst ? (DeviceInfo.Idiom == DeviceIdiom.Tablet ? 80 : 40) : 0;
        double bottom = isLast ? (DeviceInfo.Idiom == DeviceIdiom.Tablet ? 150 : 100) : 0;

        return new Thickness(0, top, 0, bottom);
    }


    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
