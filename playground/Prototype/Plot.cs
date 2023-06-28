using System;
using Avalonia.Utilities;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;
using Avalonia.Visuals.Platform;
using Avalonia.Platform;
using System.Runtime.InteropServices;
using System.Linq;
using Avalonia.Input;

namespace Prototype;

public class Plot : Control
{
    private RenderTargetBitmap _bitmap = null!;

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _bitmap = new RenderTargetBitmap(new PixelSize(100, 100), new Vector(96, 96));
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _bitmap.Dispose();
        _bitmap = null!;
        base.OnDetachedFromLogicalTree(e);
    }

    private int box_size = 100;
    private int x_start = 0;
    private int y_start = 0;

    // protected override void OnPointerMoved(PointerEventArgs e)

    public override void Render(DrawingContext context)
    {
        if ((_bitmap.Size.Width, _bitmap.Size.Height) != (Bounds.Width, Bounds.Height))
        {
            _bitmap = new RenderTargetBitmap(new PixelSize((int)Bounds.Width, (int)Bounds.Height), new Vector(96, 96));

            box_size = Math.Max(Math.Min((int)Bounds.Width, (int)Bounds.Height) - 200, 100);

            x_start = Math.Max(((int)Bounds.Width - box_size) / 2, 50);
            y_start = Math.Max(((int)Bounds.Height - box_size) / 2, 50);
        }

        FormattedText text = new("yeey", Typeface.Default, 20.0, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);

        using (var ctx = _bitmap.CreateDrawingContext(null))
        {
            ctx.Clear(Colors.Aqua);
            ctx.DrawRectangle(Brushes.Red, null, new Rect(x_start, y_start, box_size, box_size));
            ctx.DrawText(Brushes.Black, new Point(x_start, y_start + box_size), text.PlatformImpl);
        }

        context.DrawImage(_bitmap, new Rect(0, 0, Bounds.Width, Bounds.Height));
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        base.Render(context);
    }
}

