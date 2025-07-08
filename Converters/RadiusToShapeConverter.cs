using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace SpiritualGiftsSurvey.Converters
{
    public class RadiusToShapeConverter : IValueConverter
    {
        // value: your OuterRadius or InnerRadius int
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int radius)
            {
                return new RoundRectangle
                {
                    CornerRadius = new CornerRadius(radius)
                };
            }

            // fallback to no rounding
            return new RoundRectangle
            {
                CornerRadius = new CornerRadius(0)
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
