using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

public class GoodByePanel : Form
{
    // 🎨 Veda Paneli Premium Renk Paleti
    private Color bgDarkGradStart;
    private Color bgDarkGradEnd;
    private Color neonBlue;
    private Color neonRed;
    private Color textWhite;
    private Color shadowColor;

    private int tickCount = 0;
    private Timer animTimer;

    private Font fnfFontTitle;
    private Font fnfFontBody;
    private Font fnfFontSmall;
    private PrivateFontCollection pfc = new PrivateFontCollection();

    public GoodByePanel()
    {
        // Veda Paneli Boyutları ve Form Ayarları
        this.Size = new Size(420, 220);
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
                fnfFontTitle = new Font(pfc.Families[0], 24, FontStyle.Bold);
                fnfFontBody = new Font(pfc.Families[0], 16, FontStyle.Bold);
                fnfFontSmall = new Font(pfc.Families[0], 8, FontStyle.Regular);
            }
            catch { LoadDefaultFonts(); }
        }
        else
        {
            LoadDefaultFonts();
        }

        // Akıcı Animasyon Zamanlayıcısı (60 FPS)
        animTimer = new Timer();
        animTimer.Interval = 16;
        animTimer.Tick += AnimateShutdown;
        animTimer.Start();
    }

    private void LoadDefaultFonts()
    {
        try
        {
            fnfFontTitle = new Font("VCR OSD Mono", 24, FontStyle.Bold);
            fnfFontBody = new Font("VCR OSD Mono", 16, FontStyle.Bold);
            fnfFontSmall = new Font("VCR OSD Mono", 8, FontStyle.Regular);
        }
        catch
        {
            fnfFontTitle = new Font("Arial", 22, FontStyle.Bold);
            fnfFontBody = new Font("Arial", 14, FontStyle.Bold);
            fnfFontSmall = new Font("Arial", 8, FontStyle.Regular);
        }
    }

    private void AnimateShutdown(object sender, EventArgs e)
    {
        tickCount++;

        // 🎯 TIMEOUT (ZAMAN AŞIMI):
        // 150 kare boyunca (yaklaşık 2.5 saniye) ekran tam parlaklıkta kalır ve animasyonları oynatır.
        if (tickCount > 150)
        {
            // 2.5 saniye dolunca yavaşça karararak (Fade Out) uygulamayı kapatır
            this.Opacity -= 0.04f; 
            if (this.Opacity <= 0)
            {
                animTimer.Stop();
                this.Close();
                Application.Exit(); // Uygulamayı tamamen sistemden siler
            }
        }

        this.Invalidate(); // Ekran çizimini tetikler
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

        // 1. Arka Plan Geçişi
        using (LinearGradientBrush bgBrush = new LinearGradientBrush(this.ClientRectangle, bgDarkGradStart, bgDarkGradEnd, LinearGradientMode.Vertical))
        {
            g.FillRectangle(bgBrush, this.ClientRectangle);
        }
        AnimationController.DrawBackground(g, this.ClientRectangle, ThemePalette.Current, tickCount);

        // 2. Kayan Damalı Arka Plan
        int checkSize = 25;
        int offset = (tickCount / 2) % checkSize;
        using (SolidBrush checkBrush = new SolidBrush(Color.FromArgb(4, 255, 255, 255)))
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

        // 3. RETRO CRT TARAMA LAZERİ (Dikey süzülen neon çizgi)
        float laserY = (float)((Math.Sin(tickCount * 0.05) + 1.0) / 2.0) * (this.Height - 20) + 10;
        using (LinearGradientBrush laserBrush = new LinearGradientBrush(new PointF(0, laserY - 4), new PointF(0, laserY + 4), Color.Transparent, Color.FromArgb(40, neonRed)))
        {
            g.FillRectangle(laserBrush, 8, laserY - 4, this.Width - 16, 8);
        }
        using (Pen laserPen = new Pen(Color.FromArgb(120, neonRed), 1))
        {
            g.DrawLine(laserPen, 8, laserY, this.Width - 16, laserY);
        }

        // Standart CRT çizgileri
        using (Pen scanlinePen = new Pen(Color.FromArgb(12, 0, 0, 0), 2))
        {
            for (int y = 0; y < this.Height; y += 4)
            {
                g.DrawLine(scanlinePen, 0, y, this.Width, y);
            }
        }

        // 4. Kırmızı/Mavi Neon Çerçeve
        int pulseAlpha = (int)(180 + Math.Sin(tickCount * 0.15) * 40);
        using (Pen blackBorder = new Pen(shadowColor, 8))
        {
            g.DrawRectangle(blackBorder, 4, 4, this.Width - 8, this.Height - 8);
        }
        using (Pen neonPen = new Pen(Color.FromArgb(pulseAlpha, neonRed), 2))
        {
            g.DrawRectangle(neonPen, 7, 7, this.Width - 14, this.Height - 14);
        }

        // L Köşe Süsleri
        using (Brush redBrush = new SolidBrush(neonBlue))
        {
            g.FillRectangle(redBrush, 0, 0, this.Width, 6);
            g.FillRectangle(redBrush, 12, 12, 16, 4);      
            g.FillRectangle(redBrush, 12, 12, 4, 16);
            g.FillRectangle(redBrush, this.Width - 28, 12, 16, 4); 
            g.FillRectangle(redBrush, this.Width - 16, 12, 4, 16);
        }

        StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        Rectangle farewellCard = new Rectangle(35, 82, this.Width - 70, 65);
        using (GraphicsPath cardPath = CreateRoundedRectangle(farewellCard, 12))
        using (SolidBrush cardBrush = new SolidBrush(Color.FromArgb(190, bgDarkGradStart)))
        {
            g.FillPath(cardBrush, cardPath);
            using (Pen cardBorder = new Pen(Color.FromArgb(pulseAlpha / 2, neonBlue), 1))
                g.DrawPath(cardBorder, cardPath);
        }

        // ===================================================
        // 5. 3D BOUNCING BAŞLIK: "GÖRÜŞMEK ÜZERE!"
        // ===================================================
        // FNF orijinal menüdeki zıplama/esneme ritmi (Matematiksel Sinüs dalgası)
        float bounceY = (float)Math.Sin(tickCount * 0.12) * 8f; 
        string titleText = "GÖRÜŞMEK ÜZERE!";
        RectangleF titleRect = new RectangleF(0, 35 + bounceY, this.Width, 40);

        using (Brush shadowBrush = new SolidBrush(shadowColor))
        {
            g.DrawString(titleText, fnfFontTitle, shadowBrush, new RectangleF(titleRect.X + 4, titleRect.Y + 4, titleRect.Width, titleRect.Height), sfCenter);
        }
        using (Brush midBrush = new SolidBrush(neonBlue))
        {
            g.DrawString(titleText, fnfFontTitle, midBrush, new RectangleF(titleRect.X + 2, titleRect.Y + 2, titleRect.Width, titleRect.Height), sfCenter);
        }
        using (Brush frontBrush = new SolidBrush(neonRed))
        {
            g.DrawString(titleText, fnfFontTitle, frontBrush, titleRect, sfCenter);
        }

        // ===================================================
        // 6. NEFES ALAN NEON ALT BAŞLIK: "YİNE BEKLERİZ..."
        // ===================================================
        // Yazı boyutunun yumuşak şekilde büyüyüp küçülmesi (Pulsing Effect)
        float pulseScale = (float)(1.0 + Math.Sin(tickCount * 0.08) * 0.06);
        Font pulsedFont = new Font(fnfFontBody.FontFamily, fnfFontBody.Size * pulseScale, FontStyle.Bold);

        string subtitleText = "YİNE BEKLERİZ...";
        RectangleF subRect = new RectangleF(0, 95, this.Width, 40);
        
        using (Brush subShadow = new SolidBrush(shadowColor))
        {
            g.DrawString(subtitleText, pulsedFont, subShadow, new RectangleF(subRect.X + 2, subRect.Y + 2, subRect.Width, subRect.Height), sfCenter);
        }
        using (Brush subFront = new SolidBrush(neonBlue))
        {
            g.DrawString(subtitleText, pulsedFont, subFront, subRect, sfCenter);
        }
        pulsedFont.Dispose(); // RAM sızıntısı önleme

        // 7. Alt Bilgilendirme ve Kapatma Efekti Metni
        string footerText = "KAPATILIYOR";
        if (tickCount % 24 < 12 && tickCount < 150) // Yanıp sönen retro imleç
        {
            footerText += " _";
        }
        using (Brush footerBrush = new SolidBrush(Color.FromArgb(140, textWhite)))
        {
            g.DrawString(footerText, fnfFontSmall, footerBrush, new RectangleF(0, 155, this.Width, 20), sfCenter);
        }

        // Yapımcı Kapanış İmzası
        using (Brush signatureBrush = new SolidBrush(Color.FromArgb(80, textWhite)))
        {
            g.DrawString("BY BF_ALIBERK_BF - v0.0.1a", fnfFontSmall, signatureBrush, new RectangleF(0, 190, this.Width, 20), sfCenter);
        }

        Rectangle shutdownTrack = new Rectangle(44, 178, this.Width - 88, 4);
        using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(70, textWhite)))
            g.FillRectangle(trackBrush, shutdownTrack);
        if (tickCount > 150)
        {
            int remainingWidth = (int)(shutdownTrack.Width * Math.Max(0, 1f - (tickCount - 150) / 25f));
            using (LinearGradientBrush shutdownBrush = new LinearGradientBrush(
                shutdownTrack, neonBlue, neonRed, LinearGradientMode.Horizontal))
                g.FillRectangle(shutdownBrush, shutdownTrack.X, shutdownTrack.Y, remainingWidth, shutdownTrack.Height);
        }
        AnimationController.DrawOverlay(g, this.ClientRectangle, ThemePalette.Current, tickCount);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        animTimer.Stop(); // Form kapanırken timer'ı hafızadan sil
        base.OnFormClosing(e);
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