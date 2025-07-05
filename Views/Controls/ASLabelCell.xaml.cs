using System.Windows.Input;

namespace SpiritualGiftsTest.Views.Controls
{
    public partial class ASLabelCell : ContentView
    {
        public ASLabelCell()
        {
            InitializeComponent();
        }

        // ─── Bindable Properties ────────────────────────────────────────────────────

        public static readonly BindableProperty PayloadKeyProperty =
            BindableProperty.Create(nameof(PayloadKey),
                                    typeof(string),
                                    typeof(ASLabelCell),
                                    string.Empty);

        public static readonly BindableProperty LocalizedPickerTitleKeyProperty =
            BindableProperty.Create(nameof(LocalizedPickerTitleKey),
                                    typeof(string),
                                    typeof(ASLabelCell),
                                    string.Empty);

        public static readonly BindableProperty IsEmployeeProperty =
            BindableProperty.Create(nameof(IsEmployee),
                                    typeof(bool),
                                    typeof(ASLabelCell),
                                    false);

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title),
                                    typeof(string),
                                    typeof(ASLabelCell),
                                    string.Empty,
                                    BindingMode.TwoWay);

        public static readonly BindableProperty DetailProperty =
            BindableProperty.Create(nameof(Detail),
                                    typeof(string),
                                    typeof(ASLabelCell),
                                    string.Empty,
                                    BindingMode.TwoWay);

        public static readonly BindableProperty ErrorProperty =
            BindableProperty.Create(nameof(Error),
                                    typeof(string),
                                    typeof(ASLabelCell),
                                    string.Empty,
                                    BindingMode.TwoWay);

        public static readonly BindableProperty IsReadOnlyProperty =
            BindableProperty.Create(nameof(IsReadOnly),
                                    typeof(bool),
                                    typeof(ASLabelCell),
                                    false);

        public static readonly BindableProperty IsRequiredProperty =
            BindableProperty.Create(nameof(IsRequired),
                                    typeof(bool),
                                    typeof(ASLabelCell),
                                    false);

        public static readonly BindableProperty IsLastCellProperty =
            BindableProperty.Create(nameof(IsLastCell),
                                    typeof(bool),
                                    typeof(ASLabelCell),
                                    false);

        public static readonly BindableProperty IsImportantCellProperty =
            BindableProperty.Create(nameof(IsImportantCell),
                                    typeof(bool),
                                    typeof(ASLabelCell),
                                    false);

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command),
                                    typeof(ICommand),
                                    typeof(ASLabelCell),
                                    null);

        // ─── CLR Wrappers ────────────────────────────────────────────────────────────

        public string PayloadKey
        {
            get => (string)GetValue(PayloadKeyProperty);
            set => SetValue(PayloadKeyProperty, value);
        }

        public string LocalizedPickerTitleKey
        {
            get => (string)GetValue(LocalizedPickerTitleKeyProperty);
            set => SetValue(LocalizedPickerTitleKeyProperty, value);
        }

        public bool IsEmployee
        {
            get => (bool)GetValue(IsEmployeeProperty);
            set => SetValue(IsEmployeeProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Detail
        {
            get => (string)GetValue(DetailProperty);
            set => SetValue(DetailProperty, value);
        }

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public bool IsLastCell
        {
            get => (bool)GetValue(IsLastCellProperty);
            set => SetValue(IsLastCellProperty, value);
        }

        public bool IsImportantCell
        {
            get => (bool)GetValue(IsImportantCellProperty);
            set => SetValue(IsImportantCellProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }

    /// <summary>
    /// If you still need to package up parameters for your ViewModel command,
    /// you can define it here (or move it into its own file).
    /// </summary>
    public class PickerDefinition
    {
        /// <summary>
        /// The translation key for the picker’s title.
        /// </summary>
        public string LocalizedPickerTitleKey { get; set; } = string.Empty;

        /// <summary>
        /// A collection of items to show in the picker.
        /// (Assign this from your ViewModel when handling the command.)
        /// </summary>
        public IEnumerable<object>? SimpleItemsCollection { get; set; }

        /// <summary>
        /// For your special “employee” mode.
        /// </summary>
        public bool IsEmployee { get; set; }
    }
}
