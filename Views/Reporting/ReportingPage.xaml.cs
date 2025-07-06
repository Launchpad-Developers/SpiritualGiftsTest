using SpiritualGiftsTest.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsTest.Views.Reporting;

public partial class ReportingPage : BasePage
{
    public ReportingPage(ReportingViewModel vm) : base(vm)
    {
        InitializeComponent();
    }
}