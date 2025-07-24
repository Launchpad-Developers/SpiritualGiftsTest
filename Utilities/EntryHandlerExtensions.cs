using SpiritualGiftsSurvey.Views.Controls;

#if IOS
using UIKit;
using CoreAnimation;
using CoreGraphics;
#endif

namespace SpiritualGiftsSurvey.Utilities.Handlers;

public static class EntryHandlerExtensions
{
    public static void MapUnderlineEntryHandler()
    {
#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("UnderlineEntry", (handler, view) =>
        {
            if (view is UnderlineEntry)
            {
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Gray);
            }
        });
#elif IOS
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BottomBorder", (handler, view) =>
        {
            var nativeEntry = handler.PlatformView;
            nativeEntry.BorderStyle = UITextBorderStyle.None;

            var bottomLine = new CALayer
            {
                Frame = new CGRect(0, nativeEntry.Frame.Height - 1, nativeEntry.Frame.Width, 1),
                BackgroundColor = UIColor.Gray.CGColor
            };

            nativeEntry.Layer.AddSublayer(bottomLine);
            nativeEntry.ClipsToBounds = false;
        });
#endif
    }
}
