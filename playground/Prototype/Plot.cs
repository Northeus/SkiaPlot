using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;

namespace Prototype;

public class Plot : Control
{
    private RenderTargetBitmap _bitmap = null!;

    private Bitmap _datamap = null!;
    private int data_size = 0;

    private int view_x = 0;
    private int view_y = 0;
    private int view_size = 0;

    private int box_size = 100;

    private int x_start = 0;
    private int y_start = 0;

    private int min_tick_x_size = 50;
    private int min_tick_y_size = 40;

    private int tick_x_count = 2;
    private int tick_y_count = 5;

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _bitmap = new RenderTargetBitmap(new PixelSize(box_size, box_size));

        _datamap = new Bitmap("data.bmp");
        data_size = _datamap.PixelSize.Width == _datamap.PixelSize.Height ? _datamap.PixelSize.Width : -1;
        view_size = data_size;
        Console.WriteLine($"Loaded data.bmp with size {data_size}");

        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _bitmap.Dispose();
        _bitmap = null!;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        Console.WriteLine($"X={e.Delta.X} Y={e.Delta.Y}");

        Point pos = e.GetPosition(this) - new Point(x_start, y_start);
        double x_norm = pos.X / box_size;
        double y_norm = pos.Y / box_size;

        if (0 <= x_norm && x_norm <= 1 && 0 <= y_norm && y_norm <= 1)
        {
            double x_pixel = view_x + x_norm * view_size;
            double y_pixel = view_y + y_norm * view_size;

            // New view size
            view_size -= (int) e.Delta.Y * 200;
            view_size = Math.Max(view_size, 1);
            view_size = Math.Min(view_size, data_size);

            // Adjust position
            view_x = (int) (x_pixel - x_norm * view_size);
            view_y = (int) (y_pixel - y_norm * view_size);

            view_x = Math.Clamp(view_x, 0, data_size - view_size);
            view_y = Math.Clamp(view_y, 0, data_size - view_size);
        }

        base.OnPointerWheelChanged(e);
    }

    public override void Render(DrawingContext context)
    {
        if ((_bitmap.Size.Width, _bitmap.Size.Height) != (Bounds.Width, Bounds.Height))
        {
            // Set up plot size and alignment
            _bitmap = new RenderTargetBitmap(new PixelSize((int)Bounds.Width, (int)Bounds.Height), new Vector(96, 96));

            box_size = Math.Max(Math.Min((int)Bounds.Width, (int)Bounds.Height) - 200, 100);

            x_start = Math.Max(((int)Bounds.Width - box_size) / 2, 50);
            y_start = Math.Max(((int)Bounds.Height - box_size) / 2, 50);

            // Set up scales
            tick_x_count = box_size / min_tick_x_size + 1;
            tick_y_count = box_size / min_tick_y_size + 1;
        }

        using (var ctx = _bitmap.CreateDrawingContext(null))
        {
            ctx.Clear(default);
            ctx.DrawBitmap(_datamap.PlatformImpl, 1.0, new Rect(view_x, view_y, view_size, view_size), new Rect(x_start, y_start, box_size, box_size), BitmapInterpolationMode.Default);
            ctx.DrawRectangle(null, new Pen(Brushes.Black), new Rect(x_start, y_start, box_size, box_size));

            FormattedText txt = new("_", Typeface.Default, 14.0, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);

            for (int i = 0; i < tick_x_count; i++)
            {
                double portion = (double) (i * min_tick_x_size) / box_size;

                txt.Text = portion.ToString("0.000");
                ctx.DrawText(Brushes.Black, new Point(x_start + i * min_tick_x_size, y_start + box_size), txt.PlatformImpl);
            }

            for (int i = 0; i < tick_y_count - 1; i++)
            {
                double portion = (double) (i * min_tick_y_size) / box_size;

                txt.Text = (1 - portion).ToString("0.0000");
                ctx.DrawText(Brushes.Black, new Point(x_start - 50, y_start + i * min_tick_y_size), txt.PlatformImpl);
            }
        }

        context.DrawImage(_bitmap, new Rect(0, 0, Bounds.Width, Bounds.Height));
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        base.Render(context);
    }
}

