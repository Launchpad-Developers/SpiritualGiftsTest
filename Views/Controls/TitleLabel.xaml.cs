
namespace SpiritualGiftsSurvey.Views.Controls
{
    public partial class TitleLabel : ContentView
    {
        public TitleLabel()
        {
            InitializeComponent();
		}
        
        public Style NewStyle
        {
			get { return (Style)GetValue(NewStyleProperty); }
			set { SetValue(NewStyleProperty, value); }
        }

		public static readonly BindableProperty NewStyleProperty = BindableProperty.Create(
            nameof(NewStyle),
			typeof(Style),
            typeof(TitleLabel),
            null,
			propertyChanged: OnNewStylePropertyChanged);

		private static void OnNewStylePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var context = (TitleLabel)bindable;

            if (context != null && newValue != null)
            {
				context.titleLabel.Style = (Style)newValue;              
            }
		}

		public double NewFontSize
        {
			get { return (double)GetValue(NewFontSizeProperty); }
			set { SetValue(NewFontSizeProperty, value); }
        }

		public static readonly BindableProperty NewFontSizeProperty = BindableProperty.Create(
			nameof(NewFontSize),
            typeof(double),
            typeof(TitleLabel),
            12.0,
			propertyChanged: OnNewFontSizePropertyChanged);

		private static void OnNewFontSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var context = (TitleLabel)bindable;

            if (context != null && newValue != null)
            {
				context.titleLabel.FontSize = (double)newValue;
            }
        }
        
		public string LabelText
        {
			get { return (string)GetValue(LabelTextProperty); }
			set { SetValue(LabelTextProperty, value); }
        }

		public static readonly BindableProperty LabelTextProperty = BindableProperty.Create(
            nameof(LabelText),
            typeof(string),
			typeof(TitleLabel),
            string.Empty,
			propertyChanged: OnLabelTextPropertyChanged);

		private static void OnLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
			var context = (TitleLabel)bindable;

            if (context != null && newValue != null)
            {
				context.titleLabel.Text = (string)newValue;
            }
        }
        
        public FormattedString FormattedLabelText
        {
			get { return (FormattedString)GetValue(FormattedLabelTextProperty); }
			set { SetValue(FormattedLabelTextProperty, value); }
        }

		public static readonly BindableProperty FormattedLabelTextProperty = BindableProperty.Create(
			nameof(FormattedLabelText),
			typeof(FormattedString),
            typeof(TitleLabel),
            null,
			propertyChanged: OnFormattedLabelTextPropertyChanged);

		private static void OnFormattedLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var context = (TitleLabel)bindable;

            if (context != null && newValue != null)
            {
				context.titleLabel.FormattedText = (FormattedString)newValue;
            }
		}
    }
}
