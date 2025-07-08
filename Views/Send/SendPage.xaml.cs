using SpiritualGiftsSurvey.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Send;

public partial class SendPage : BasePage
{
    public SendPage(SendViewModel vm) : base(vm)
    {
        InitializeComponent();
    }
}