using SpiritualGiftsTest.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsTest.Views.Send;

public partial class SendPage : BasePage
{
    public SendPage(SendViewModel vm) : base(vm)
    {
        InitializeComponent();
    }
}