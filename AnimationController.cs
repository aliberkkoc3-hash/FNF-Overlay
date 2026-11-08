using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public static class AnimationController
{
    public static void DrawBackground(Graphics graphics, Rectangle bounds, OverlayTheme theme, int frame)
    {
        if (graphics == null || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        if (theme == OverlayTheme.Corruption)
        {
            ThemePalette.DrawCorruptionBackground(graphics, bounds, frame, theme);
            return;
        }

        ThemeColors colors = ThemePalette.Get(theme);
        GraphicsState state = graphics.Save();
        graphics.SetClip(bounds);

        int bandHeight = Math.Max(2, bounds.Height / 12);
        int bandY = bounds.Top + PositiveModulo(frame * 2, bounds.Height + bandHeight) - bandHeight;
        Rectangle band = new Rectangle(bounds.Left, bandY, bounds.Width, bandHeight);
        using (LinearGradientBrush glow = new LinearGradientBrush(
            band, Color.Transparent, Color.Transparent, LinearGradientMode.Vertical))
        {
            ColorBlend blend = new ColorBlend();
            blend.Colors = new[]
            {
                Color.FromArgb(0, colors.Primary),
                Color.FromArgb(18, colors.Primary),
                Color.FromArgb(0, colors.Secondary)
            };
            blend.Positions = new[] { 0f, 0.5f, 1f };
            glow.InterpolationColors = blend;
            graphics.FillRectangle(glow, band);
        }

        switch (theme)
        {
            case OverlayTheme.YellowBlack:
                DrawMovingStripes(graphics, bounds, frame, colors.Primary, 13);
                break;
            case OverlayTheme.BlackWhite:
                DrawMovingGrid(graphics, bounds, frame, colors.Primary);
                break;
            case OverlayTheme.YellowRed:
                DrawMovingStripes(graphics, bounds, frame, colors.Secondary, 18);
                break;
            case OverlayTheme.LightBlueRed:
                DrawMovingColumns(graphics, bounds, frame, colors.Primary, colors.Secondary);
                break;
            default:
                DrawPulseRings(graphics, bounds, frame, colors.Primary, colors.Secondary);
                break;
        }

        DrawParticles(graphics, bounds, frame, colors.Primary, colors.Secondary);
        graphics.Restore(state);
    }

    public static void DrawOverlay(Graphics graphics, Rectangle bounds, OverlayTheme theme, int frame)
    {
        if (graphics == null || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        if (theme == OverlayTheme.Corruption)
        {
            ThemePalette.DrawCorruptionOverlay(graphics, bounds, frame, theme);
            return;
        }

        ThemeColors colors = ThemePalette.Get(theme);
        int accentWidth = Math.Max(18, bounds.Width / 5);
        int x = bounds.Left + PositiveModulo(frame * 3, bounds.Width + accentWidth) - accentWidth;
        int alpha = 22 + (int)((Math.Sin(frame * 0.07) + 1) * 12);

        using (Pen accent = new Pen(Color.FromArgb(alpha, colors.Highlight), 1))
        {
            graphics.DrawLine(accent, x, bounds.Top, x + accentWidth / 3, bounds.Top);
            graphics.DrawLine(accent, x + accentWidth * 2 / 3, bounds.Bottom - 1,
                x + accentWidth, bounds.Bottom - 1);
        }

        using (Brush glint = new SolidBrush(Color.FromArgb(alpha, colors.Secondary)))
        {
            int y = bounds.Top + PositiveModulo(frame * 5, bounds.Height);
            graphics.FillRectangle(glint, bounds.Left, y, Math.Min(2, bounds.Width), 2);
        }
    }

    private static void DrawMovingStripes(Graphics graphics, Rectangle bounds, int frame, Color color, int spacing)
    {
        int offset = PositiveModulo(frame, spacing);
        using (Pen stripe = new Pen(Color.FromArgb(9, color), 2))
        {
            for (int x = bounds.Left - bounds.Height + offset; x < bounds.Right; x += spacing * 2)
                graphics.DrawLine(stripe, x, bounds.Bottom, x + bounds.Height, bounds.Top);
        }
    }

    private static void DrawMovingGrid(Graphics graphics, Rectangle bounds, int frame, Color color)
    {
        int offset = PositiveModulo(frame / 2, 24);
        using (Pen grid = new Pen(Color.FromArgb(11, color), 1))
        {
            for (int x = bounds.Left + offset; x < bounds.Right; x += 24)
                graphics.DrawLine(grid, x, bounds.Top, x, bounds.Bottom);
            for (int y = bounds.Top + offset; y < bounds.Bottom; y += 24)
                graphics.DrawLine(grid, bounds.Left, y, bounds.Right, y);
        }
    }

    private static void DrawMovingColumns(Graphics graphics, Rectangle bounds, int frame, Color primary, Color secondary)
    {
        using (Pen stream = new Pen(Color.FromArgb(12, primary), 1))
        using (Pen echo = new Pen(Color.FromArgb(9, secondary), 1))
        {
            for (int i = 0; i < 5; i++)
            {
                int x = bounds.Left + PositiveModulo(frame * (i + 1) + i * 31, bounds.Width);
                graphics.DrawLine(i % 2 == 0 ? stream : echo, x, bounds.Top, x - 8, bounds.Bottom);
            }
        }
    }

    private static void DrawPulseRings(Graphics graphics, Rectangle bounds, int frame, Color primary, Color secondary)
    {
        int maxSize = Math.Max(bounds.Width, bounds.Height);
        int size = 12 + PositiveModulo(frame * 2, Math.Max(1, maxSize / 2));
        int alpha = 8 + (int)((Math.Sin(frame * 0.06) + 1) * 6);
        using (Pen ring = new Pen(Color.FromArgb(alpha, primary), 1))
        using (Pen echo = new Pen(Color.FromArgb(alpha / 2, secondary), 1))
        {
            graphics.DrawEllipse(ring, bounds.Right - size, bounds.Top - size / 3, size, size);
            graphics.DrawEllipse(echo, bounds.Right - size / 2, bounds.Bottom - size / 2, size / 2, size / 2);
        }
    }

    private static void DrawParticles(Graphics graphics, Rectangle bounds, int frame, Color primary, Color secondary)
    {
        for (int i = 0; i < 7; i++)
        {
            int x = bounds.Left + PositiveModulo(frame * (i + 1) + i * 47, bounds.Width);
            int y = bounds.Top + PositiveModulo(frame * (i + 2) + i * 29, bounds.Height);
            int size = i % 3 == 0 ? 3 : 2;
            int alpha = 13 + (int)((Math.Sin(frame * 0.09 + i) + 1) * 8);
            using (Brush particle = new SolidBrush(Color.FromArgb(alpha, i % 2 == 0 ? primary : secondary)))
                graphics.FillEllipse(particle, x, y, size, size);
        }
    }

    private static int PositiveModulo(int value, int divisor)
    {
        int result = value % divisor;
        return result < 0 ? result + divisor : result;
    }
}
