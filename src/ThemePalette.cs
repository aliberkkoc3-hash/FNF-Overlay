using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public enum OverlayTheme
{
    Default,
    YellowBlack,
    BlackWhite,
    YellowRed,
    LightBlueRed,
    Corruption
}

public sealed class ThemeColors
{
    public Color BackgroundStart { get; private set; }
    public Color BackgroundEnd { get; private set; }
    public Color Surface { get; private set; }
    public Color Primary { get; private set; }
    public Color Secondary { get; private set; }
    public Color Highlight { get; private set; }
    public Color Text { get; private set; }
    public Color MutedText { get; private set; }
    public Color Shadow { get; private set; }

    public ThemeColors(Color backgroundStart, Color backgroundEnd, Color surface,
        Color primary, Color secondary, Color highlight, Color text, Color mutedText, Color shadow)
    {
        BackgroundStart = backgroundStart;
        BackgroundEnd = backgroundEnd;
        Surface = surface;
        Primary = primary;
        Secondary = secondary;
        Highlight = highlight;
        Text = text;
        MutedText = mutedText;
        Shadow = shadow;
    }
}

public static class ThemePalette
{
    private static readonly ThemeColors BFAliColors = new ThemeColors(
        Color.FromArgb(8, 16, 36), Color.FromArgb(29, 13, 39), Color.FromArgb(25, 27, 48),
        Color.FromArgb(49, 214, 255), Color.FromArgb(255, 67, 118), Color.FromArgb(190, 247, 255),
        Color.FromArgb(241, 245, 249), Color.FromArgb(155, 170, 191), Color.FromArgb(4, 8, 20));
    private static readonly ThemeColors BFLecColors = new ThemeColors(
        Color.FromArgb(12, 12, 14), Color.FromArgb(31, 23, 11), Color.FromArgb(32, 27, 17),
        Color.FromArgb(255, 205, 40), Color.FromArgb(239, 137, 28), Color.FromArgb(255, 244, 186),
        Color.FromArgb(255, 248, 225), Color.FromArgb(186, 164, 117), Color.FromArgb(4, 4, 5));
    private static readonly ThemeColors BlackWhiteColors = new ThemeColors(
        Color.FromArgb(8, 8, 10), Color.FromArgb(27, 27, 30), Color.FromArgb(35, 35, 39),
        Color.FromArgb(245, 245, 245), Color.FromArgb(155, 160, 170), Color.White,
        Color.FromArgb(250, 250, 250), Color.FromArgb(170, 174, 182), Color.Black);
    private static readonly ThemeColors YellowRedColors = new ThemeColors(
        Color.FromArgb(24, 10, 12), Color.FromArgb(43, 14, 15), Color.FromArgb(48, 22, 22),
        Color.FromArgb(255, 213, 48), Color.FromArgb(239, 55, 65), Color.FromArgb(255, 238, 150),
        Color.FromArgb(255, 245, 232), Color.FromArgb(205, 155, 144), Color.FromArgb(12, 4, 5));
    private static readonly ThemeColors LightBlueRedColors = new ThemeColors(
        Color.FromArgb(8, 20, 32), Color.FromArgb(30, 15, 27), Color.FromArgb(25, 34, 47),
        Color.FromArgb(112, 218, 255), Color.FromArgb(245, 63, 77), Color.FromArgb(207, 243, 255),
        Color.FromArgb(243, 249, 255), Color.FromArgb(155, 177, 194), Color.FromArgb(3, 8, 15));
    private static readonly ThemeColors CorruptionColors = new ThemeColors(
        Color.FromArgb(5, 5, 8), Color.FromArgb(30, 4, 11), Color.FromArgb(34, 10, 17),
        Color.FromArgb(255, 42, 63), Color.FromArgb(142, 14, 34), Color.FromArgb(255, 126, 139),
        Color.FromArgb(255, 242, 244), Color.FromArgb(184, 145, 153), Color.FromArgb(2, 1, 3));

    public static OverlayTheme Current { get; private set; }

    static ThemePalette()
    {
        Current = OverlayTheme.Default;
    }

    public static void SetCurrent(OverlayTheme theme)
    {
        Current = IsValid(theme) ? theme : OverlayTheme.Default;
    }

    public static bool IsValid(OverlayTheme theme)
    {
        return theme == OverlayTheme.Default ||
            theme == OverlayTheme.YellowBlack ||
            theme == OverlayTheme.BlackWhite ||
            theme == OverlayTheme.YellowRed ||
            theme == OverlayTheme.LightBlueRed ||
            theme == OverlayTheme.Corruption;
    }

    public static ThemeColors Get(OverlayTheme theme)
    {
        switch (theme)
        {
            case OverlayTheme.YellowBlack:
                return BFLecColors;
            case OverlayTheme.BlackWhite:
                return BlackWhiteColors;
            case OverlayTheme.YellowRed:
                return YellowRedColors;
            case OverlayTheme.LightBlueRed:
                return LightBlueRedColors;
            case OverlayTheme.Corruption:
                return CorruptionColors;
            default:
                return BFAliColors;
        }
    }

    public static void DrawCorruptionBackground(Graphics graphics, Rectangle bounds, int tick)
    {
        DrawCorruptionBackground(graphics, bounds, tick, Current);
    }

    public static void DrawCorruptionBackground(Graphics graphics, Rectangle bounds, int tick, OverlayTheme theme)
    {
        if (theme != OverlayTheme.Corruption || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        using (Pen scanlines = new Pen(Color.FromArgb(15, 255, 42, 63), 1))
        {
            for (int y = (tick % 4); y < bounds.Height; y += 5)
                graphics.DrawLine(scanlines, bounds.Left, bounds.Top + y, bounds.Right, bounds.Top + y);
        }

        int sweepY = (tick * 3) % bounds.Height;
        int sweepHeight = Math.Min(24, bounds.Height - sweepY);
        if (sweepHeight > 0)
        {
            Rectangle sweepBounds = new Rectangle(bounds.Left, bounds.Top + sweepY, bounds.Width, sweepHeight);
            using (LinearGradientBrush sweep = new LinearGradientBrush(
                sweepBounds, Color.Transparent, Color.Transparent, LinearGradientMode.Vertical))
            {
                ColorBlend sweepColors = new ColorBlend();
                sweepColors.Colors = new[]
                {
                    Color.FromArgb(0, 255, 42, 63),
                    Color.FromArgb(34, 255, 42, 63),
                    Color.FromArgb(0, 142, 14, 34)
                };
                sweepColors.Positions = new[] { 0f, 0.52f, 1f };
                sweep.InterpolationColors = sweepColors;
                graphics.FillRectangle(sweep, bounds.Left, bounds.Top + sweepY, bounds.Width, sweepHeight);
            }
        }

        DrawGlitchFragments(graphics, bounds, tick, 20);
        DrawRedEmbers(graphics, bounds, tick);
    }

    public static void DrawCorruptionOverlay(Graphics graphics, Rectangle bounds, int tick)
    {
        DrawCorruptionOverlay(graphics, bounds, tick, Current);
    }

    public static void DrawCorruptionOverlay(Graphics graphics, Rectangle bounds, int tick, OverlayTheme theme)
    {
        if (theme != OverlayTheme.Corruption || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        if (tick % 34 < 5 || tick % 57 < 4)
        {
            int bandHeight = Math.Min(4, bounds.Height);
            int bandY = bounds.Top + ((tick * 11) % Math.Max(1, bounds.Height - bandHeight + 1));
            int shift = (tick * 7) % Math.Max(1, bounds.Width / 8);
            using (Brush redBand = new SolidBrush(Color.FromArgb(40, 255, 42, 63)))
                graphics.FillRectangle(redBand, bounds.Left + shift, bandY, Math.Max(1, bounds.Width - shift), bandHeight);

            int fragmentY = Math.Min(bounds.Bottom - 1, bandY + bandHeight + 1);
            int fragmentWidth = Math.Max(1, bounds.Width / 3);
            using (Brush darkRedBand = new SolidBrush(Color.FromArgb(58, 142, 14, 34)))
                graphics.FillRectangle(darkRedBand, bounds.Left, fragmentY, fragmentWidth, Math.Min(2, bounds.Bottom - fragmentY));
        }

        DrawGlitchFragments(graphics, bounds, tick, 12);
    }

    private static void DrawGlitchFragments(Graphics graphics, Rectangle bounds, int tick, int interval)
    {
        if (tick % interval >= 3)
            return;

        int heightLimit = Math.Max(1, bounds.Height - 6);
        int y1 = bounds.Top + ((tick * 17 + 13) % heightLimit);
        int y2 = bounds.Top + ((tick * 29 + 37) % heightLimit);
        int width1 = Math.Max(1, bounds.Width / 4);
        int width2 = Math.Max(1, bounds.Width / 7);

        using (Brush brightFragment = new SolidBrush(Color.FromArgb(74, 255, 42, 63)))
            graphics.FillRectangle(brightFragment, bounds.Left + ((tick * 7) % Math.Max(1, bounds.Width - width1 + 1)), y1, width1, 2);
        using (Brush deepFragment = new SolidBrush(Color.FromArgb(65, 142, 14, 34)))
            graphics.FillRectangle(deepFragment, bounds.Left + ((tick * 11) % Math.Max(1, bounds.Width - width2 + 1)), y2, width2, 3);
    }

    private static void DrawRedEmbers(Graphics graphics, Rectangle bounds, int tick)
    {
        for (int i = 0; i < 5; i++)
        {
            int x = bounds.Left + ((tick * (i + 2) + i * 71) % bounds.Width);
            int y = bounds.Top + ((tick * (i + 3) + i * 47) % bounds.Height);
            int size = i % 2 == 0 ? 2 : 3;
            using (Brush ember = new SolidBrush(Color.FromArgb(35 + (i * 7), 255, 42, 63)))
                graphics.FillEllipse(ember, x, y, size, size);
        }
    }
}
