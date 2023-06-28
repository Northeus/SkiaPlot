// COPIED FROM AVALONIE GIT REPO

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

namespace FirstSteps
{
    public class RenderTargetBitmapPage : Control
    {
        private RenderTargetBitmap _bitmap = null!;
        private static int size = 4096;
        private static int view_size = 512;

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _bitmap = new RenderTargetBitmap(new PixelSize(view_size, view_size), new Vector(96, 96));
            base.OnAttachedToLogicalTree(e);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _bitmap.Dispose();
            _bitmap = null!;
            base.OnDetachedFromLogicalTree(e);
        }

        readonly Stopwatch _st = Stopwatch.StartNew();
        public override unsafe void Render(DrawingContext context)
        {
            // From sample
            // using (var ctxi = _bitmap.CreateDrawingContext(null))
            // using(var ctx = new DrawingContext(ctxi, false))
            // using (ctx.PushPostTransform(Matrix.CreateTranslation(-100, -100)
            //                              * Matrix.CreateRotation(_st.Elapsed.TotalSeconds)
            //                              * Matrix.CreateTranslation(100, 100)))
            // {
            //     ctxi.Clear(default);
            //     ctx.FillRectangle(Brushes.Fuchsia, new Rect(50, 50, 100, 100));
            // }

            WriteableBitmap bitmap = new WriteableBitmap(
                new PixelSize(size, size),
                new Vector(96, 96),
                PixelFormat.Rgba8888,
                //AlphaFormat.Premul
                AlphaFormat.Opaque
            );

            byte[] data = Enumerable.Repeat((byte)0, size * size * 4).ToArray();

            byte t = (byte) (_st.Elapsed.TotalMilliseconds / 10);
            for (int x = 0; x < size * size; x++)
            {
                int idx = x * 4;
                data[idx++] = (byte) (t % 256);
                data[idx++] = 0;
                data[idx++] = 0;
                data[idx++] = 255;

                t++;
            }

            using (var fb = bitmap.Lock())
            {
                Marshal.Copy(data, 0, fb.Address, size * size * 4);

                /* Bytes are set correctly
                var ptr = (byte*) fb.Address;
                for (int x = 0; x < 5; x++)
                {
                    byte r = *ptr++;
                    byte g = *ptr++;
                    byte b = *ptr++;
                    byte a = *ptr++;

                    Console.WriteLine($"RGBA({r}, {g}, {b}, {a})");
                }
                */
            }

            using (var ctx = _bitmap.CreateDrawingContext(null))
            {
                ctx.Clear(Colors.Aqua);
                ctx.DrawBitmap(bitmap.PlatformImpl, 1.0, new Rect(0, 0, size, size), new Rect(0, 0, view_size, view_size), BitmapInterpolationMode.Default);
            }

            context.DrawImage(_bitmap, new Rect(0, 0, view_size, view_size));
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
            base.Render(context);
        }
    }
}
