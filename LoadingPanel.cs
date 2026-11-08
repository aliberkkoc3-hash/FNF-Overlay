using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

public class LoadingPanel : Form
{
    // 🎨 Geliştirilmiş Premium FNF Renk Paleti
    private Color bgDarkGradStart;
    private Color bgDarkGradEnd;
    private Color neonBlue;
    private Color neonRed;
    private Color textWhite;
    private Color shadowColor;

    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private string statusText = "INITIALIZING SYSTEM...";
    
    private Timer animTimer;
    private Font fnfFontLarge;
    private Font fnfFontSmall;
    private PrivateFontCollection pfc = new PrivateFontCollection();

    private int tickCount = 0;

    public LoadingPanel()
    {
        this.Size = new Size(460, 240);
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.DoubleBuffered = true;
        this.ShowInTaskbar = false;
        this.TopMost = true;

        // Font Yükleme
        string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VCR_OSD_MONO.ttf");
        if (File.Exists(fontPath))
        {
            try
            {
                pfc.AddFontFile(fontPath);
                fnfFontLarge = new Font(pfc.Families[0], 24, FontStyle.Bold);
                fnfFontSmall = new Font(pfc.Families[0], 10, FontStyle.Regular);
            }
            catch 
            { 
                LoadDefaultFonts(); 
            }
        }
        else
        {
            LoadDefaultFonts();
        }

        // Animasyon için Timer (60 FPS)
        animTimer = new Timer();
        animTimer.Interval = 16; 
        animTimer.Tick += AnimateProgress;
        animTimer.Start();
    }

    private void LoadDefaultFonts()
    {
        try
        {
            fnfFontLarge = new Font("VCR OSD Mono", 24, FontStyle.Bold);
            fnfFontSmall = new Font("VCR OSD Mono", 10, FontStyle.Regular);
        }
        catch
        {
            fnfFontLarge = new Font("Arial", 22, FontStyle.Bold);
            fnfFontSmall = new Font("Arial", 10, FontStyle.Bold);
        }
    }

    public void UpdateStatus(string message, int progressPercent)
    {
        statusText = message.ToUpper();
        targetProgress = Math.Min(100, Math.Max(0, progressPercent));
    }

    private void AnimateProgress(object sender, EventArgs e)
    {
        tickCount++;

        if (Math.Abs(currentProgress - targetProgress) > 0.1f)
        {
            currentProgress += (targetProgress - currentProgress) * 0.08f;
        }

        this.Invalidate(); // Her karede ekranı yeniden çiz

        if (currentProgress >= 99.5f && targetProgress >= 100f)
        {
            currentProgress = 100f;
            this.Opacity -= 0.05f;
            if (this.Opacity <= 0)
            {
                animTimer.Stop();
                this.Close();
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        ThemeColors theme = ThemePalette.Get(ThemePalette.Current);
        bgDarkGradStart = theme.BackgroundStart;
        bgDarkGradEnd = theme.BackgroundEnd;
        neonBlue = theme.Primary;
        neonRed = theme.Secondary;
        textWhite = theme.Text;
        shadowColor = theme.Shadow;
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        // 1. GRAFİK ARKA PLAN (Geçişli Koyu Tema)
        using (LinearGradientBrush bgBrush = new LinearGradientBrush(this.ClientRectangle, bgDarkGradStart, bgDarkGradEnd, LinearGradientMode.Vertical))
        {
            g.FillRectangle(bgBrush, this.ClientRectangle);
        }
        AnimationController.DrawBackground(g, this.ClientRectangle, ThemePalette.Current, tickCount);

        // 2. FNF TARZI HAREKETLİ DAMALI ARKA PLAN (Checkerboard)
        int checkSize = 30;
        int offset = (tickCount / 2) % checkSize; // Yavaşça kayma efekti
        using (SolidBrush checkBrush = new SolidBrush(Color.FromArgb(4, textWhite)))
        {
            for (int x = -checkSize; x < this.Width + checkSize; x += checkSize * 2)
            {
                for (int y = -checkSize; y < this.Height + checkSize; y += checkSize * 2)
                {
                    g.FillRectangle(checkBrush, x + offset, y + offset, checkSize, checkSize);
                    g.FillRectangle(checkBrush, x + checkSize + offset, y + checkSize + offset, checkSize, checkSize);
                }
            }
        }

        // 3. RETRO TARAMA ÇİZGİLERİ (CRT Scanlines)
        using (Pen scanlinePen = new Pen(Color.FromArgb(10, 0, 0, 0), 2))
        {
            for (int y = 0; y < this.Height; y += 4)
            {
                g.DrawLine(scanlinePen, 0, y, this.Width, y);
            }
        }

        // 4. NEFES ALAN (PULSING) NEON DIŞ ÇERÇEVE
        int pulseAlpha = (int)(160 + Math.Sin(tickCount * 0.1) * 45); // 115-205 arası yumuşak geçiş
        using (Pen blackBorder = new Pen(shadowColor, 8))
        {
            g.DrawRectangle(blackBorder, 4, 4, this.Width - 8, this.Height - 8);
        }
        using (Pen neonBluePen = new Pen(Color.FromArgb(pulseAlpha, neonBlue), 2))
        {
            g.DrawRectangle(neonBluePen, 7, 7, this.Width - 14, this.Height - 14);
        }

        // Sol-Sağ Üst Köşe L Süsleri
        using (Brush redBrush = new SolidBrush(neonRed))
        {
            g.FillRectangle(redBrush, 0, 0, this.Width, 6); // En üst kırmızı şerit
            g.FillRectangle(redBrush, 12, 12, 16, 4);      
            g.FillRectangle(redBrush, 12, 12, 4, 16);
            g.FillRectangle(redBrush, this.Width - 28, 12, 16, 4); 
            g.FillRectangle(redBrush, this.Width - 16, 12, 4, 16);
        }

        // 5. ATARİ TARZI BLINKING "● LIVE" GÖSTERGESİ (Sağ Üst)
        if (tickCount % 30 < 15) // Saniyede bir yanıp söner
        {
            using (Brush liveDot = new SolidBrush(neonRed))
            {
                g.FillEllipse(liveDot, this.Width - 30, 20, 10, 10);
            }
        }
        using (Brush liveTextBrush = new SolidBrush(Color.FromArgb(120, textWhite)))
        {
            g.DrawString("LIVE", fnfFontSmall, liveTextBrush, this.Width - 68, 18);
        }

        // 6. 3D BAŞLIK: "FNF OVERLAY"
        string titleText = "FNF OVERLAY";
        RectangleF titleRect = new RectangleF(0, 32, this.Width, 50);
        StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        // 3D Gölgelendirmeler
        using (Brush shadowBrush = new SolidBrush(shadowColor))
        {
            g.DrawString(titleText, fnfFontLarge, shadowBrush, new RectangleF(titleRect.X + 4, titleRect.Y + 4, titleRect.Width, titleRect.Height), sfCenter);
        }
        using (Brush midBrush = new SolidBrush(neonRed))
        {
            g.DrawString(titleText, fnfFontLarge, midBrush, new RectangleF(titleRect.X + 2, titleRect.Y + 2, titleRect.Width, titleRect.Height), sfCenter);
        }
        using (Brush frontBrush = new SolidBrush(neonBlue))
        {
            g.DrawString(titleText, fnfFontLarge, frontBrush, titleRect, sfCenter);
        }

        Rectangle statusCard = new Rectangle(28, 91, this.Width - 56, 103);
        using (GraphicsPath cardPath = CreateRoundedRectangle(statusCard, 12))
        using (SolidBrush cardBrush = new SolidBrush(Color.FromArgb(125, 6, 11, 27)))
        {
            g.FillPath(cardBrush, cardPath);
            using (Pen cardGlow = new Pen(Color.FromArgb(45, neonBlue), 1))
                g.DrawPath(cardGlow, cardPath);
        }
        using (Pen statusAccent = new Pen(Color.FromArgb(150, neonRed), 2))
            g.DrawLine(statusAccent, 53, 127, this.Width - 53, 127);

        // 7. YANIP SÖNEN STATUS METNİ
        string currentStatus = statusText;
        if (tickCount % 24 < 12 && currentProgress < 100)
        {
            currentStatus += " _";
        }
        using (Brush statusBrush = new SolidBrush(textWhite))
        {
            g.DrawString(currentStatus, fnfFontSmall, statusBrush, new RectangleF(25, 98, this.Width - 50, 25), sfCenter);
        }

        // 8. İLERLEME ÇUBUĞU (Progress Bar)
        int barX = 45;
        int barY = 138;
        int barWidth = this.Width - 90;
        int barHeight = 24;

        Rectangle barOuterRect = new Rectangle(barX, barY, barWidth, barHeight);
        using (SolidBrush darkBg = new SolidBrush(shadowColor))
        {
            g.FillRectangle(darkBg, barOuterRect);
        }
        using (Pen outlinePen = new Pen(Color.FromArgb(100, neonRed), 2))
        {
            g.DrawRectangle(outlinePen, barOuterRect);
        }

        // Bar Dolgusu ve İç Çapraz Çizgiler (Hatch Pattern)
        int fillWidth = (int)((barWidth * currentProgress) / 100f);
        if (fillWidth > 4)
        {
            Rectangle fillRect = new Rectangle(barX + 3, barY + 3, fillWidth - 6, barHeight - 6);
            
            // Neon mavi renk geçişi
            using (LinearGradientBrush lgb = new LinearGradientBrush(
                fillRect, neonBlue, ControlPaint.Dark(neonBlue, 0.6f), LinearGradientMode.Horizontal))
            {
                g.FillRectangle(lgb, fillRect);
            }

            // Atari tarzı havalı çapraz çizgiler (Hatch style)
            using (HatchBrush hatch = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(45, Color.White), Color.Transparent))
            {
                g.FillRectangle(hatch, fillRect);
            }

            // Parlama çizgisi
            using (SolidBrush glossBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
            {
                g.FillRectangle(glossBrush, barX + 3, barY + 3, fillWidth - 6, 4);
            }
        }

        // 9. YÜZDE GÖSTERGESİ
        string percentStr = ((int)currentProgress).ToString() + "%";
        using (Brush pctBrush = new SolidBrush(neonBlue))
        {
            using (Brush pctShadow = new SolidBrush(shadowColor))
            {
                g.DrawString(percentStr, fnfFontSmall, pctShadow, new RectangleF(1, 171, this.Width, 20), sfCenter);
            }
            g.DrawString(percentStr, fnfFontSmall, pctBrush, new RectangleF(0, 170, this.Width, 20), sfCenter);
        }

        // Süs "SYSTEM OK" ibaresi
        using (Brush footerBrush = new SolidBrush(Color.FromArgb(100, textWhite)))
        {
            g.DrawString("SYSTEM STATUS: OK", fnfFontSmall, footerBrush, new RectangleF(0, 202, this.Width, 20), sfCenter);
        }
        AnimationController.DrawOverlay(g, this.ClientRectangle, ThemePalette.Current, tickCount);
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        int diameter = radius * 2;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}