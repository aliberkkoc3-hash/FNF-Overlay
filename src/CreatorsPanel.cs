using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

public class CreatorsPanel : Form
{
    // 🎨 Yapımcı Kartı Premium Renk Paleti
    private Color bgDarkGradStart;
    private Color bgDarkGradEnd;
    private Color neonBlue;
    private Color neonRed;
    private Color textWhite;
    private Color shadowColor;

    private Timer animTimer;
    private int tickCount = 0;
    
    private Font fnfFontTitle;
    private Font fnfFontBody;
    private Font fnfFontFooter;
    private PrivateFontCollection pfc = new PrivateFontCollection();

    public CreatorsPanel()
    {
        // Panel Boyutları ve Form Ayarları
        this.Size = new Size(380, 220);
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
                fnfFontTitle = new Font(pfc.Families[0], 20, FontStyle.Bold);
                fnfFontBody = new Font(pfc.Families[0], 12, FontStyle.Bold);
                fnfFontFooter = new Font(pfc.Families[0], 8, FontStyle.Regular);
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

        // Animasyon ve Akıcılık için Timer (60 FPS)
        animTimer = new Timer();
        animTimer.Interval = 16;
        animTimer.Tick += (s, e) => { tickCount++; this.Invalidate(); };
        animTimer.Start();
    }

    private void LoadDefaultFonts()
    {
        try
        {
            fnfFontTitle = new Font("VCR OSD Mono", 20, FontStyle.Bold);
            fnfFontBody = new Font("VCR OSD Mono", 12, FontStyle.Bold);
            fnfFontFooter = new Font("VCR OSD Mono", 8, FontStyle.Regular);
        }
        catch
        {
            fnfFontTitle = new Font("Arial", 18, FontStyle.Bold);
            fnfFontBody = new Font("Arial", 11, FontStyle.Bold);
            fnfFontFooter = new Font("Arial", 8, FontStyle.Regular);
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

        // 1. ARKA PLAN GEÇİŞİ (Gradient)
        using (LinearGradientBrush bgBrush = new LinearGradientBrush(this.ClientRectangle, bgDarkGradStart, bgDarkGradEnd, LinearGradientMode.Vertical))
        {
            g.FillRectangle(bgBrush, this.ClientRectangle);
        }
        AnimationController.DrawBackground(g, this.ClientRectangle, ThemePalette.Current, tickCount);

        // 2. FNF TARZI DİNAMİK DAMALI DESEN (Kayan Checkerboard)
        int checkSize = 25;
        int offset = (tickCount / 2) % checkSize;
        using (SolidBrush checkBrush = new SolidBrush(Color.FromArgb(5, textWhite)))
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

        // 3. RETRO CRT TARAMA ÇİZGİLERİ
        using (Pen scanlinePen = new Pen(Color.FromArgb(12, shadowColor), 2))
        {
            for (int y = 0; y < this.Height; y += 4)
            {
                g.DrawLine(scanlinePen, 0, y, this.Width, y);
            }
        }

        // 4. NEON GEÇİŞLİ KALIN ÇERÇEVE
        int pulseAlpha = (int)(170 + Math.Sin(tickCount * 0.1) * 40);
        using (Pen blackBorder = new Pen(shadowColor, 8))
        {
            g.DrawRectangle(blackBorder, 4, 4, this.Width - 8, this.Height - 8);
        }
        using (Pen neonPen = new Pen(Color.FromArgb(pulseAlpha, neonBlue), 2))
        {
            g.DrawRectangle(neonPen, 7, 7, this.Width - 14, this.Height - 14);
        }

        // Süsleme Köşeleri
        using (Brush redBrush = new SolidBrush(neonRed))
        {
            g.FillRectangle(redBrush, 0, 0, this.Width, 6); // Üst kırmızı şerit
            g.FillRectangle(redBrush, 12, 12, 16, 4);      
            g.FillRectangle(redBrush, 12, 12, 4, 16);
            g.FillRectangle(redBrush, this.Width - 28, 12, 16, 4); 
            g.FillRectangle(redBrush, this.Width - 16, 12, 4, 16);
        }

        StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        Rectangle creatorCard = new Rectangle(34, 77, this.Width - 68, 96);
        Color creatorCardStart = Color.FromArgb(210, bgDarkGradStart);
        Color creatorCardEnd = Color.FromArgb(220, bgDarkGradEnd);
        using (GraphicsPath cardPath = CreateRoundedRectangle(creatorCard, 12))
        using (LinearGradientBrush cardBrush = new LinearGradientBrush(
            creatorCard, creatorCardStart, creatorCardEnd, LinearGradientMode.Horizontal))
        {
            g.FillPath(cardBrush, cardPath);
            using (Pen cardBorder = new Pen(Color.FromArgb(pulseAlpha / 2, neonBlue), 1))
                g.DrawPath(cardBorder, cardPath);
        }
        using (Brush accentBrush = new SolidBrush(neonRed))
            g.FillRectangle(accentBrush, 34, 89, 3, 68);

        // 5. 3D BAŞLIK: "YAPIMCI KARTI"
        string titleText = "YAPIMCI KARTI";
        RectangleF titleRect = new RectangleF(0, 30, this.Width, 40);
        
        using (Brush shadowBrush = new SolidBrush(shadowColor))
        {
            g.DrawString(titleText, fnfFontTitle, shadowBrush, new RectangleF(titleRect.X + 3, titleRect.Y + 3, titleRect.Width, titleRect.Height), sfCenter);
        }
        using (Brush midBrush = new SolidBrush(neonRed))
        {
            g.DrawString(titleText, fnfFontTitle, midBrush, new RectangleF(titleRect.X + 1, titleRect.Y + 1, titleRect.Width, titleRect.Height), sfCenter);
        }
        using (Brush frontBrush = new SolidBrush(neonBlue))
        {
            g.DrawString(titleText, fnfFontTitle, frontBrush, titleRect, sfCenter);
        }

        // 6. YAPIMCI İSMİ (BF_AliBerk_BF - Kalın 3D Pop Art)
        string devName = "BF_ALIBERK_BF";
        RectangleF devRect = new RectangleF(0, 82, this.Width, 35);
        using (Brush nameShadow = new SolidBrush(shadowColor))
        {
            g.DrawString(devName, fnfFontBody, nameShadow, new RectangleF(devRect.X + 2, devRect.Y + 2, devRect.Width, devRect.Height), sfCenter);
        }
        using (Brush nameFront = new SolidBrush(textWhite))
        {
            g.DrawString(devName, fnfFontBody, nameFront, devRect, sfCenter);
        }

        RectangleF creatorRoleRect = new RectangleF(42, 119, 142, 20);
        RectangleF twinRoleRect = new RectangleF(196, 119, 142, 20);
        using (Brush creatorRoleBrush = new SolidBrush(neonBlue))
            g.DrawString("BFAli  /  YAPIMCI", fnfFontFooter, creatorRoleBrush, creatorRoleRect, sfCenter);
        using (Brush twinRoleBrush = new SolidBrush(neonRed))
            g.DrawString("BFLec  /  TWIN", fnfFontFooter, twinRoleBrush, twinRoleRect, sfCenter);

        // ===================================================
        // 7. 3D ŞEKİLLİ SÜRÜM BİLGİSİ: "FNF OVERLAY v0.0.6"
        // ===================================================
        string versionText = "FNF OVERLAY v0.0.6";
        RectangleF versionRect = new RectangleF(0, 143, this.Width, 20);
        
        // Sürüm Siyah 3D Gölge
        using (Brush verShadow = new SolidBrush(shadowColor))
        {
            g.DrawString(versionText, fnfFontFooter, verShadow, new RectangleF(versionRect.X + 1, versionRect.Y + 1, versionRect.Width, versionRect.Height), sfCenter);
        }
        // Sürüm Neon Mavi Canlı Ön Katman
        using (Brush verFront = new SolidBrush(neonBlue))
        {
            g.DrawString(versionText, fnfFontFooter, verFront, versionRect, sfCenter);
        }

        // Teşekkür Yazısı (Alt Süsleme)
        string thanksText = "KULLANDIĞINIZ İÇİN TEŞEKKÜRLER!";
        RectangleF thanksRect = new RectangleF(0, 164, this.Width, 17);
        using (Brush thanksBrush = new SolidBrush(Color.FromArgb(180, textWhite)))
        {
            g.DrawString(thanksText, fnfFontFooter, thanksBrush, thanksRect, sfCenter);
        }

        // 8. NEON KAPATMA TALİMATI (C basınca kapanma uyarısı - Yanıp Sönen Efekt)
        int blinkAlpha = (int)(150 + Math.Sin(tickCount * 0.15) * 105); // Yumuşak yanıp sönme
        using (Brush blinkBrush = new SolidBrush(Color.FromArgb(blinkAlpha, neonRed)))
        {
            g.DrawString("[C] TUSUNA BASARAK KAPAT", fnfFontFooter, blinkBrush, new RectangleF(0, 188, this.Width, 18), sfCenter);
        }
        AnimationController.DrawOverlay(g, this.ClientRectangle, ThemePalette.Current, tickCount);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        animTimer.Stop(); // Bellek sızıntısını önlemek için timer'ı kapat
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