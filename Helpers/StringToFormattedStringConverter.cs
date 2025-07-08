using System.Linq;
using System.Text.RegularExpressions;

namespace SpiritualGiftsSurvey.Helpers
{
    public static class StringToFormattedStringConverter
    {
		public static FormattedString FormatStringWithFontSize(string source, int attFontSize, int fontSize)
        {
            var formattedString = new FormattedString();

			var parts = Regex.Split(source, @"(<b>[\s\S]+?<\/b>|<i>[\s\S]+?<\/i>)").Where(l => l != string.Empty).ToArray();

            foreach (var s in parts)
            {
                bool bold = false;
                string cleanSpan = s;
                if (s.Contains("<b>"))
                {
                    bold = true;
                    cleanSpan = s.Replace("<b>", "").Replace("</b>", "");
				}

                bool italic = false;
                if (s.Contains("<i>"))
                {
                    italic = true;
                    cleanSpan = s.Replace("<i>", "").Replace("</i>", "");
                }

                var span = new Span();
                span.Text = cleanSpan;

                if (bold)
                {
                    span.FontAttributes = FontAttributes.Bold;
                    span.FontSize = attFontSize;
                }
				else if (italic)
				{
                    span.FontAttributes = FontAttributes.Italic;
					span.FontSize = attFontSize;
				}
				else
				{
					span.FontSize = fontSize;
				}

				span.FontFamily = DeviceInfo.Platform == DevicePlatform.iOS ? "Avenir-Roman" : "MavenPro-Regular.ttf#Maven Pro";

                formattedString.Spans.Add(span);
            }

            return formattedString;
        }
    }
}
