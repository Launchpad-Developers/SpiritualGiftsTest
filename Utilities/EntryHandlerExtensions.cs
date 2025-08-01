using SpiritualGiftsSurvey.Views.Controls;

#if IOS
using UIKit;
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
            if (view is UnderlineEntry)
            {
                var nativeEntry = handler.PlatformView;
                nativeEntry.BorderStyle = UITextBorderStyle.RoundedRect;
                nativeEntry.BackgroundColor = UIColor.White;

                nativeEntry.Layer.CornerRadius = 8;
                nativeEntry.Layer.MasksToBounds = true;
                nativeEntry.Layer.BorderWidth = 1;
                nativeEntry.Layer.BorderColor = UIColor.LightGray.CGColor;
                
                nativeEntry.LeftView = new UIView(new CGRect(0, 0, 10, 0));
                nativeEntry.LeftViewMode = UITextFieldViewMode.Always;
            }

            if (view is BorderlessEntry)
            {
                var nativeEntry = handler.PlatformView;
                nativeEntry.BorderStyle = UITextBorderStyle.None; // no border
                nativeEntry.BackgroundColor = UIColor.Clear;      // transparent so parent Border shows
            }
        });
#endif
    }
}
