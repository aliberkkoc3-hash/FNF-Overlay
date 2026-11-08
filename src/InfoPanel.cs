using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

public class InfoPanel : Form
{
    // 🎨 Bilgi Paneli Premium FNF Renk Paleti
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
    private Font fnfFontSmall;
    private PrivateFontCollection pfc = new PrivateFontCollection();
    private string[] gameKeyNames;
    private string exitKeyName;

    public InfoPanel(string[] gameKeyNames, string exitKeyName)
    {
        this.gameKeyNames = (string[])gameKeyNames.Clone();
        this.exitKeyName = exitKeyName;
        this.Size = new Size(520, 420);
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
                fnfFontTitle = new Font(pfc.Families[0], 18, FontStyle.Bold);
                fnfFontBody = new Font(pfc.Families[0], 10, FontStyle.Bold);
                fnfFontSmall = new Font(pfc.Families[0], 8, FontStyle.Regular);
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

        // Animasyon Timer'ı (60 FPS)
        animTimer = new Timer();
        animTimer.Interval = 16;
        animTimer.Tick += (s, e) => { tickCount++; this.Invalidate(); };
        animTimer.Start();
    }

    public void UpdateExitKey(string exitKeyName)
    {
        this.exitKeyName = exitKeyName;
        this.Invalidate();
    }

    private void LoadDefaultFonts()
    {
        try
        {
            fnfFontTitle = new Font("VCR OSD Mono", 18, FontStyle.Bold);
            fnfFontBody = new Font("VCR OSD Mono", 10, FontStyle.Bold);
            fnfFontSmall = new Font("VCR OSD Mono", 8, FontStyle.Regular);
        }
        catch
        {
            fnfFontTitle = new Font("Arial", 16, FontStyle.Bold);
            fnfFontBody = new Font("Arial", 10, FontStyle.Bold);
            fnfFontSmall = new Font("Arial", 8, FontStyle.Regular);
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

        // 1. Arka Plan Geçişi
        using (LinearGradientBrush bgBrush = new LinearGradientBrush(this.ClientRectangle, bgDarkGradStart, bgDarkGradEnd, LinearGradientMode.Vertical))
        {
            g.FillRectangle(bgBrush, this.ClientRectangle);
        }
        AnimationController.DrawBackground(g, this.ClientRectangle, ThemePalette.Current, tickCount);

        // 2. Kayan Damalı Arka Plan (Checkerboard)
        int checkSize = 25;
        int offset = (tickCount / 2) % checkSize;
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

        // 3. CRT Tarama Çizgileri
        using (Pen scanlinePen = new Pen(Color.FromArgb(12, shadowColor), 2))
        {
            for (int y = 0; y < this.Height; y += 4)
            {
                g.DrawLine(scanlinePen, 0, y, this.Width, y);
            }
        }

        // 4. Parlayan Neon Çerçeve
        int pulseAlpha = (int)(170 + Math.Sin(tickCount * 0.1) * 40);
        using (Pen blackBorder = new Pen(shadowColor, 8))
        {
            g.DrawRectangle(blackBorder, 4, 4, this.Width - 8, this.Height - 8);
        }
        using (Pen neonPen = new Pen(Color.FromArgb(pulseAlpha, neonBlue), 2))
        {
            g.DrawRectangle(neonPen, 7, 7, this.Width - 14, this.Height - 14);
        }

        // Köşe Süsleri
        using (Brush redBrush = new SolidBrush(neonRed))
        {
            g.FillRectangle(redBrush, 0, 0, this.Width, 6);
            g.FillRectangle(redBrush, 12, 12, 16, 4);      
            g.FillRectangle(redBrush, 12, 12, 4, 16);
            g.FillRectangle(redBrush, this.Width - 28, 12, 16, 4); 
            g.FillRectangle(redBrush, this.Width - 16, 12, 4, 16);
        }

        StringFormat sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        StringFormat sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        // 5. Başlık
        string titleText = "KONTROL PANELI";
        RectangleF titleRect = new RectangleF(0, 30, this.Width, 35);
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

        // 6. TUŞ REHBERİ LİSTESİ (Tuş İsimleri Güncellendi)
        int startY = 86;
        int rowHeight = 30;

        string[,] keysInfo = {
            { "[" + string.Join("/", gameKeyNames) + "]", "Oyun Tuşları (6 Tuş Göstergesi)" },
            { "[ALT + S]", "Ayarları ve temayı aç" },
            { "[CTRL (Basılı Tut)]", "Serbest Sürükle ve Bırak" },
            { "[CTRL + R]", "Otomatik Konum Takibi" },
            { "[CTRL + 1 / 2]", "Sol Alt Köşe / Sağ Alt Köşe" },
            { "[CTRL + 3 / 4]", "Sol Orta Köşe / Sağ Orta Köşe" },
            { "[C]", "Yapımcı Kartını Aç / Kapat" },
            { "[H]", "Bilgi Panelini Aç / Kapat" },
            { "[" + exitKeyName + "]", "Programı Kapat ve Çıkış Yap" }
        };

        for (int i = 0; i < keysInfo.GetLength(0); i++)
        {
            int currentY = startY + (i * rowHeight);
            Rectangle rowRect = new Rectangle(20, currentY, this.Width - 40, 25);
            using (GraphicsPath rowPath = CreateRoundedRectangle(rowRect, 6))
            using (SolidBrush rowBrush = new SolidBrush(Color.FromArgb(190, ThemePalette.Get(ThemePalette.Current).Surface)))
            {
                g.FillPath(rowBrush, rowPath);
            }
            using (Brush keyChip = new SolidBrush(Color.FromArgb(35, neonBlue)))
            {
                g.FillRectangle(keyChip, 28, currentY + 4, 158, 17);
            }

            RectangleF keyRect = new RectangleF(32, currentY + 1, 152, 22);
            RectangleF descRect = new RectangleF(198, currentY + 1, this.Width - 220, 22);

            using (Brush keyBrush = new SolidBrush(neonBlue))
            {
                g.DrawString(keysInfo[i, 0], fnfFontBody, keyBrush, keyRect, sfLeft);
            }

            using (Brush descBrush = new SolidBrush(textWhite))
            {
                g.DrawString(keysInfo[i, 1], fnfFontSmall, descBrush, descRect, sfLeft);
            }
        }

        // 7. YANIP SÖNEN KAPATMA UYARISI
        int blinkAlpha = (int)(150 + Math.Sin(tickCount * 0.15) * 105);
        using (Brush blinkBrush = new SolidBrush(Color.FromArgb(blinkAlpha, neonRed)))
        {
            g.DrawString("[H] TUSUNA BASARAK KAPAT", fnfFontSmall, blinkBrush, new RectangleF(0, 387, this.Width, 20), sfCenter);
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

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        animTimer.Stop();
        base.OnFormClosing(e);
    }
}