using System;

namespace SpiritualGiftsTest.Views.Controls
{
    public class ASImageButton : ImageButton
    {
        public ContentView CurrentContentView { get; set; } = default!;
        public ContentView TargetContentView { get; set; } = default!;
        public ContentView CurrentParallelContentView { get; set; } = default!;
        public ContentView TargetParallelContentView { get; set; } = default!;
        public int TargetPageNumber { get; set; }
    }
}
