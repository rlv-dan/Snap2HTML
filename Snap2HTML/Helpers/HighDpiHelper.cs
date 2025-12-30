// Source: https://stackoverflow.com/a/43110725/1087811

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class HighDpiHelper
{
    public static void AdjustControlImagesDpiScale(Control container)
    {
        var dpiScale = GetDpiScale(container).Value;
        if (CloseToOne(dpiScale))
            return;

        AdjustControlImagesDpiScale(container.Controls, dpiScale);
    }

    private static void AdjustButtonImageDpiScale(ButtonBase button, float dpiScale)
    {
        var image = button.Image;
        if (image == null)
            return;

        button.Image = ScaleImage(image, dpiScale);
    }

    private static void AdjustControlImagesDpiScale(Control.ControlCollection controls, float dpiScale)
    {
        foreach (Control control in controls)
        {
            var button = control as ButtonBase;
            if (button != null)
                AdjustButtonImageDpiScale(button, dpiScale);
            else
            {
                var pictureBox = control as PictureBox;
                if (pictureBox != null)
                    AdjustPictureBoxDpiScale(pictureBox, dpiScale);
            }

            AdjustControlImagesDpiScale(control.Controls, dpiScale);
        }
    }

    private static void AdjustPictureBoxDpiScale(PictureBox pictureBox, float dpiScale)
    {
        var image = pictureBox.Image;
        if (image == null)
            return;

        if (pictureBox.SizeMode == PictureBoxSizeMode.AutoSize)
            pictureBox.Image = ScaleImage(pictureBox.Image, dpiScale);
    }

    private static bool CloseToOne(float dpiScale)
    {
        return Math.Abs(dpiScale - 1) < 0.001;
    }

    private static Lazy<float> GetDpiScale(Control control)
    {
        return new Lazy<float>(() =>
        {
            using (var graphics = control.CreateGraphics())
                return graphics.DpiX / 96.0f;
        });
    }

    private static Image ScaleImage(Image image, float dpiScale)
    {
        var newSize = ScaleSize(image.Size, dpiScale);
        var newBitmap = new Bitmap(newSize.Width, newSize.Height);

        using (var g = Graphics.FromImage(newBitmap))
        {
            // According to this blog post http://blogs.msdn.com/b/visualstudio/archive/2014/03/19/improving-high-dpi-support-for-visual-studio-2013.aspx
            // NearestNeighbor is more adapted for 200% and 200%+ DPI

            var interpolationMode = InterpolationMode.HighQualityBicubic;
            if (dpiScale >= 2.0f)
                interpolationMode = InterpolationMode.NearestNeighbor;

            g.InterpolationMode = interpolationMode;
            g.DrawImage(image, new Rectangle(new Point(), newSize));
        }

        return newBitmap;
    }

    private static Size ScaleSize(Size size, float scale)
    {
        return new Size((int)(size.Width * scale), (int)(size.Height * scale));
    }
}