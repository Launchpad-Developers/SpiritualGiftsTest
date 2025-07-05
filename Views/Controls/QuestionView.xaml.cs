using Microsoft.Maui.Controls;
using SpiritualGiftsTest.ViewModels;

namespace SpiritualGiftsTest.Views.Controls
{
    public partial class QuestionView : ContentView
    {
        public QuestionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The view‐model backing this QuestionView.
        /// When set, we update the BindingContext so that all bindings in XAML (e.g. to Question.QuestionText, Question.UserValue) resolve correctly.
        /// </summary>
        public QuestionViewModel Question
        {
            get => (QuestionViewModel)GetValue(QuestionProperty);
            set => SetValue(QuestionProperty, value);
        }

        public static readonly BindableProperty QuestionProperty =
            BindableProperty.Create(
                nameof(Question),
                typeof(QuestionViewModel),
                typeof(QuestionView),
                default(QuestionViewModel),
                propertyChanged: OnQuestionChanged);

        private static void OnQuestionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is QuestionView view && newValue is QuestionViewModel vm)
            {
                view.BindingContext = vm;
            }
        }
    }
}
