using SpiritualGiftsSurvey.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Results;

public partial class ResultsPage : BasePage
{
    public ResultsPage(ResultsViewModel vm) : base(vm)
    {
        InitializeComponent();
    }
}