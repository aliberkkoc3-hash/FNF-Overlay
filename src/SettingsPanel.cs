using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

public class SettingsPanel : Form
{
    private Color BackgroundStart { get { return ThemePalette.Get(selectedTheme).BackgroundStart; } }
    private Color BackgroundEnd { get { return ThemePalette.Get(selectedTheme).BackgroundEnd; } }
    private Color AccentBlue { get { return ThemePalette.Get(selectedTheme).Primary; } }
    private Color AccentRed { get { return ThemePalette.Get(selectedTheme).Secondary; } }
    private Color TextColor { get { return ThemePalette.Get(selectedTheme).Text; } }
    private Color MutedText { get { return ThemePalette.Get(selectedTheme).MutedText; } }
    private Color CardColor { get { return ThemePalette.Get(selectedTheme).Surface; } }
    private readonly Button[] keyButtons = new Button[6];
    private readonly Button exitKeyButton = new Button();
    private readonly Panel[] themeCards = new Panel[6];
    private readonly Panel[] keyRows = new Panel[7];
    private readonly int[] selectedKeys;
    private readonly Func<int[], OverlayTheme, int, bool> applySettings;
    private int capturingKeyIndex = -1;
    private bool capturingExitKey;
    private int selectedExitKey;
    private OverlayTheme selectedTheme;
    private readonly Timer animationTimer = new Timer();
    private int animationFrame;

    public SettingsPanel(int[] keys, OverlayTheme theme, int exitKey, Screen targetScreen,
        Func<int[], OverlayTheme, int, bool> applySettings)
    {
        if (keys == null || keys.Length != keyButtons.Length)
            throw new ArgumentException("Ayarlar için tam olarak altı tuş gereklidir.", "keys");
        if (exitKey < 1 || exitKey > 255)
            throw new ArgumentOutOfRangeException("exitKey");
        if (applySettings == null)
            throw new ArgumentNullException("applySettings");

        selectedKeys = (int[])keys.Clone();
        selectedExitKey = exitKey;
        selectedTheme = theme;
        this.applySettings = applySettings;
        Rectangle workingArea = (targetScreen ?? Screen.PrimaryScreen).WorkingArea;
        int availableWidth = Math.Max(1, workingArea.Width - 24);
        int availableHeight = Math.Max(1, workingArea.Height - 24);
        this.Size = new Size(Math.Min(500, availableWidth), Math.Min(835, availableHeight));
        this.Location = new Point(
            workingArea.Left + (workingArea.Width - this.Width) / 2,
            workingArea.Top + (workingArea.Height - this.Height) / 2);
        this.AutoScroll = true;
        this.AutoScrollMinSize = new Size(500, 835);
        this.FormBorderStyle = FormBorderStyle.None;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.Manual;
        this.ShowInTaskbar = false;
        this.TopMost = true;
        this.KeyPreview = true;
        this.BackColor = BackgroundStart;
        this.ForeColor = TextColor;
        this.Text = "FNF Overlay | Ayarlar";
        this.Icon = LoadSettingsIcon();

        AddHeader();
        AddKeySettings();
        AddExitKeySetting();
        AddThemeSettings();
        AddFooterButtons();
        this.KeyDown += SettingsPanelKeyDown;
        this.Paint += PaintPanel;
        animationTimer.Interval = 45;
        animationTimer.Tick += delegate
        {
            animationFrame++;
            Invalidate();
            for (int i = 0; i < themeCards.Length; i++)
                if (themeCards[i] != null)
                    themeCards[i].Invalidate();
        };
        animationTimer.Start();
        this.FormClosed += delegate
        {
            animationTimer.Stop();
            animationTimer.Dispose();
        };
    }

    private static Icon LoadSettingsIcon()
    {
        Stream iconStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("FNFOverlay.ico");
        if (iconStream == null)
            return null;

        using (iconStream)
            return new Icon(iconStream);
    }

    private void AddHeader()
    {
        PictureBox icon = new PictureBox();
        icon.Image = this.Icon == null ? null : this.Icon.ToBitmap();
        icon.SizeMode = PictureBoxSizeMode.Zoom;
        icon.Location = new Point(30, 23);
        icon.Size = new Size(48, 48);
        this.Controls.Add(icon);

        Label title = CreateLabel("AYARLAR", 92, 20, 350, 34, 19, AccentBlue, true);
        this.Controls.Add(title);
        this.Controls.Add(CreateLabel("FNF OVERLAY  /  KONTROLLER & GÖRÜNÜM", 94, 53, 360, 22, 8, MutedText, false));
        this.Controls.Add(CreateLabel("Oyundaki tuşlarını ve overlay temasını kişiselleştir.", 30, 82, 430, 25, 9, TextColor, false));

        Button closeButton = CreateActionButton("×", 454, 25, 28, 28, AccentRed);
        closeButton.Font = new Font("Arial", 12, FontStyle.Bold);
        closeButton.Click += delegate { this.Close(); };
        this.Controls.Add(closeButton);
    }

    private void AddKeySettings()
    {
        AddSectionHeading("01", "TUŞ DÜZENİ", "Değiştirmek için tuşa tıkla, ardından klavyeden yeni tuşu bas.", 122);

        string[] keyLabels = { "Sol", "Aşağı", "Taunt", "Yukarı", "Sağ", "Dodge" };
        string[] keyHints = { "SOL OK", "ALT OK", "ÖZEL AKSİYON", "ÜST OK", "SAĞ OK", "KAÇIŞ / DODGE" };

        for (int i = 0; i < keyButtons.Length; i++)
        {
            int keyIndex = i;
            int rowY = 176 + i * 42;
            Panel row = CreateCard(26, rowY, 432, 36);
            row.Controls.Add(CreateLabel(keyLabels[i], 16, 1, 120, 18, 9, TextColor, true));
            row.Controls.Add(CreateLabel(keyHints[i], 16, 18, 170, 14, 7, MutedText, false));

            Button button = CreateKeyButton(GetKeyName(selectedKeys[i]), 314, 4, 104, 28);
            button.Click += delegate
            {
                capturingExitKey = false;
                capturingKeyIndex = keyIndex;
                UpdateKeyButtonLabels();
                button.Text = "BAS...";
                button.ForeColor = AccentBlue;
                this.Activate();
            };
            keyButtons[i] = button;
            row.Controls.Add(button);
            keyRows[i] = row;
            this.Controls.Add(row);
        }
    }

    private void AddThemeSettings()
    {
        AddSectionHeading("02", "TEMA", "Karakterini yansitan renkleri ve animasyonlu stilleri sec.", 485);
        AddThemeCard(26, 535, OverlayTheme.Default, "BFAli • YAPIMCI", "NEO PULSE");
        AddThemeCard(247, 535, OverlayTheme.YellowBlack, "BFLec • TWIN", "GOLD RUSH");
        AddThemeCard(26, 603, OverlayTheme.BlackWhite, "SIYAH & BEYAZ", "MONOCHROME");
        AddThemeCard(247, 603, OverlayTheme.YellowRed, "SARI & KIRMIZI", "BOLD CLASSIC");
        AddThemeCard(26, 671, OverlayTheme.LightBlueRed, "AÇIK MAVI & KIRMIZI", "SKYLINE");
        AddThemeCard(247, 671, OverlayTheme.Corruption, "CRIMSON CORRUPTION", "BLACKOUT  /  RED STATIC");
    }

    private void AddExitKeySetting()
    {
        Panel row = CreateCard(26, 432, 432, 38);
        row.Controls.Add(CreateLabel("Uygulamadan çıkış", 16, 2, 220, 18, 9, TextColor, true));
        row.Controls.Add(CreateLabel("Çıkış tuşunu değiştirmek için bas ve yeni tuşa bas.", 16, 19, 280, 15, 7, MutedText, false));

        exitKeyButton.Text = GetKeyName(selectedExitKey);
        exitKeyButton.Location = new Point(314, 5);
        exitKeyButton.Size = new Size(104, 28);
        exitKeyButton.BackColor = CardColor;
        exitKeyButton.ForeColor = AccentRed;
        exitKeyButton.FlatStyle = FlatStyle.Flat;
        exitKeyButton.FlatAppearance.BorderColor = Color.FromArgb(100, AccentRed);
        exitKeyButton.FlatAppearance.BorderSize = 1;
        exitKeyButton.Font = new Font("Arial", 9, FontStyle.Bold);
        exitKeyButton.Cursor = Cursors.Hand;
        exitKeyButton.Click += delegate
        {
            capturingKeyIndex = -1;
            capturingExitKey = true;
            UpdateKeyButtonLabels();
            exitKeyButton.Text = "BAS...";
            exitKeyButton.ForeColor = AccentBlue;
            this.Activate();
        };
        row.Controls.Add(exitKeyButton);
        keyRows[6] = row;
        this.Controls.Add(row);
    }

    private void AddThemeCard(int x, int y, OverlayTheme theme, string title, string subtitle)
    {
        Panel card = new Panel();
        card.Location = new Point(x, y);
        card.Size = new Size(211, 58);
        card.BackColor = CardColor;
        card.Cursor = Cursors.Hand;
        card.Paint += delegate(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool selected = selectedTheme == theme;
            Color primary = GetThemePrimary(theme);
            Color secondary = GetThemeSecondary(theme);
            Rectangle bounds = new Rectangle(1, 1, card.Width - 2, card.Height - 2);
            ThemeColors colors = ThemePalette.Get(theme);
            using (LinearGradientBrush cardBackground = new LinearGradientBrush(
                bounds, colors.BackgroundStart, colors.BackgroundEnd, LinearGradientMode.ForwardDiagonal))
                g.FillRectangle(cardBackground, bounds);

            GraphicsState state = g.Save();
            g.SetClip(bounds);
            AnimationController.DrawBackground(g, bounds, theme, animationFrame);
            if (theme == OverlayTheme.Corruption)
            {
                int glitchShift = selected ? (int)(Math.Sin(animationFrame * 0.45) * 4) : 0;
                using (Pen glitchBars = new Pen(Color.FromArgb(selected ? 70 : 40, primary), 2))
                using (Pen glitchAccent = new Pen(Color.FromArgb(selected ? 95 : 55, secondary), 2))
                {
                    for (int barY = 7; barY < card.Height; barY += 13)
                    {
                        int offset = ((barY % 2 == 0) ? 8 : 0) + glitchShift;
                        g.DrawLine(glitchBars, card.Width - 82 + offset, barY, card.Width - 12, barY);
                    }
                    g.DrawLine(glitchAccent, card.Width - 64, 12, card.Width - 37, 12);
                    g.DrawLine(glitchAccent, card.Width - 31, 31, card.Width - 9, 31);
                    if (selected && animationFrame % 8 < 3)
                    {
                        using (Brush glitchFlash = new SolidBrush(Color.FromArgb(85, secondary)))
                            g.FillRectangle(glitchFlash, card.Width - 82, 23 + (animationFrame % 11), 49, 3);
                    }
                }
            }
            else if (theme == OverlayTheme.YellowBlack || theme == OverlayTheme.YellowRed)
            {
                using (Pen hazardStripe = new Pen(Color.FromArgb(selected ? 32 : 19, primary), 4))
                {
                    for (int stripeX = -card.Height; stripeX < card.Width; stripeX += 17)
                        g.DrawLine(hazardStripe, stripeX, card.Height, stripeX + card.Height, 0);
                }
            }
            else if (theme == OverlayTheme.BlackWhite)
            {
                using (Pen grid = new Pen(Color.FromArgb(selected ? 33 : 20, primary), 1))
                {
                    for (int gridX = card.Width - 65; gridX < card.Width; gridX += 12)
                        g.DrawLine(grid, gridX, 0, gridX, card.Height);
                    for (int gridY = 4; gridY < card.Height; gridY += 12)
                        g.DrawLine(grid, card.Width - 65, gridY, card.Width, gridY);
                }
            }
            else
            {
                using (Brush pulseDots = new SolidBrush(Color.FromArgb(selected ? 42 : 26, primary)))
                {
                    for (int dotX = card.Width - 72; dotX < card.Width + 18; dotX += 15)
                        for (int dotY = 8; dotY < card.Height; dotY += 15)
                            g.FillEllipse(pulseDots, dotX, dotY, 4, 4);
                }
                using (Pen energyLine = new Pen(Color.FromArgb(selected ? 90 : 55, secondary), 1))
                {
                    Point[] pulse = {
                        new Point(card.Width - 76, 46), new Point(card.Width - 60, 46),
                        new Point(card.Width - 53, 29), new Point(card.Width - 45, 53),
                        new Point(card.Width - 36, 36), new Point(card.Width - 27, 46),
                        new Point(card.Width + 4, 46)
                    };
                    g.DrawLines(energyLine, pulse);
                }
            }
            g.Restore(state);

            using (Brush swatch = new SolidBrush(primary))
                g.FillRectangle(swatch, 10, 12, 4, 42);
            using (Brush secondSwatch = new SolidBrush(secondary))
                g.FillRectangle(secondSwatch, 17, 12, 4, 42);
            using (Pen border = new Pen(selected ? primary : Color.FromArgb(75, 120, 130, 155), selected ? 2 : 1))
                g.DrawRectangle(border, bounds);
            if (selected)
            {
                using (Brush selectionGlow = new SolidBrush(Color.FromArgb(28, primary)))
                    g.FillRectangle(selectionGlow, 1, 1, card.Width - 2, 3);
                using (Brush selectedDot = new SolidBrush(primary))
                    g.FillEllipse(selectedDot, card.Width - 15, 7, 6, 6);
            }
        };
        card.Click += delegate { selectedTheme = theme; RefreshThemeCards(); };

        Label titleLabel = CreateLabel(title, 38, 8, 166, 20, 8.5f,
            GetThemePrimary(theme), true);
        Label subtitleLabel = CreateLabel(subtitle, 38, 29, 164, 17, 7, ThemePalette.Get(theme).MutedText, false);
        titleLabel.Tag = "theme-primary";
        subtitleLabel.Tag = "theme-muted";
        titleLabel.Cursor = Cursors.Hand;
        subtitleLabel.Cursor = Cursors.Hand;
        titleLabel.Click += delegate { selectedTheme = theme; RefreshThemeCards(); };
        subtitleLabel.Click += delegate { selectedTheme = theme; RefreshThemeCards(); };
        card.Controls.Add(titleLabel);
        card.Controls.Add(subtitleLabel);
        themeCards[(int)theme] = card;
        this.Controls.Add(card);
    }

    private void AddSectionHeading(string number, string title, string subtitle, int y)
    {
        this.Controls.Add(CreateLabel(number, 29, y, 32, 24, 10, AccentRed, true));
        this.Controls.Add(CreateLabel(title, 65, y, 200, 24, 10, TextColor, true));
        this.Controls.Add(CreateLabel(subtitle, 65, y + 22, 400, 19, 8, MutedText, false));
    }

    private void AddFooterButtons()
    {
        Button applyButton = CreateActionButton("UYGULA  ✓", 255, 760, 203, 38, AccentBlue);
        applyButton.Click += ApplyButtonClick;
        this.Controls.Add(applyButton);

        Button closeButton = CreateActionButton("VAZGEÇ", 26, 760, 211, 38, MutedText);
        closeButton.Click += delegate { this.Close(); };
        this.Controls.Add(closeButton);
        this.Controls.Add(CreateLabel("ALT + S  /  AYARLARI AÇ-KAPAT", 26, 800, 432, 16, 7, MutedText, false));
    }

    private void PaintPanel(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        Color primary = GetThemePrimary(selectedTheme);
        Color secondary = GetThemeSecondary(selectedTheme);
        using (LinearGradientBrush background = new LinearGradientBrush(
            this.ClientRectangle,
            ThemePalette.Get(selectedTheme).BackgroundStart,
            ThemePalette.Get(selectedTheme).BackgroundEnd,
            LinearGradientMode.Vertical))
            g.FillRectangle(background, this.ClientRectangle);

        using (Pen scanline = new Pen(Color.FromArgb(9, 0, 0, 0), 1))
        {
            for (int y = 0; y < this.Height; y += 4)
                g.DrawLine(scanline, 0, y, this.Width, y);
        }

        using (LinearGradientBrush topLine = new LinearGradientBrush(
            new Rectangle(0, 0, this.Width, 5), secondary, primary, LinearGradientMode.Horizontal))
            g.FillRectangle(topLine, 0, 0, this.Width, 5);
        using (Pen headerGlow = new Pen(Color.FromArgb(26, primary), 1))
            g.DrawLine(headerGlow, 20, 112, this.Width - 20, 112);
        AnimationController.DrawBackground(g, this.ClientRectangle, selectedTheme, animationFrame);
        AnimationController.DrawOverlay(g, this.ClientRectangle, selectedTheme, animationFrame);
    }

    private static Color GetThemePrimary(OverlayTheme theme)
    {
        return ThemePalette.Get(theme).Primary;
    }

    private static Color GetThemeSecondary(OverlayTheme theme)
    {
        return ThemePalette.Get(theme).Secondary;
    }

    private Panel CreateCard(int x, int y, int width, int height)
    {
        Panel card = new Panel();
        card.Location = new Point(x, y);
        card.Size = new Size(width, height);
        card.BackColor = CardColor;
        card.Paint += delegate(object sender, PaintEventArgs e)
        {
            using (Pen border = new Pen(Color.FromArgb(85, AccentBlue), 1))
                e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
        };
        return card;
    }

    private Label CreateLabel(string text, int x, int y, int width, int height,
        float fontSize, Color color, bool bold)
    {
        ThemeColors colors = ThemePalette.Get(selectedTheme);
        Label label = new Label
        {
            Text = text,
            ForeColor = color,
            BackColor = Color.Transparent,
            Font = new Font("Arial", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
            Location = new Point(x, y),
            Size = new Size(width, height),
            TextAlign = ContentAlignment.MiddleLeft
        };
        label.Tag = color.ToArgb() == colors.MutedText.ToArgb() ? "muted" :
            color.ToArgb() == colors.Primary.ToArgb() ? "primary" :
            color.ToArgb() == colors.Secondary.ToArgb() ? "secondary" : "text";
        return label;
    }

    private Button CreateKeyButton(string text, int x, int y, int width, int height)
    {
        Button button = CreateActionButton(text, x, y, width, height, AccentBlue);
        button.Font = new Font("Arial", 9, FontStyle.Bold);
        return button;
    }

    private Button CreateActionButton(string text, int x, int y, int width, int height, Color accent)
    {
        Button button = new Button();
        button.Text = text;
        button.Location = new Point(x, y);
        button.Size = new Size(width, height);
        button.BackColor = CardColor;
        button.ForeColor = accent;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(100, accent);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(CardColor, 0.12f);
        button.UseVisualStyleBackColor = false;
        button.Font = new Font("Arial", 9, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        return button;
    }

    private static string GetKeyName(int virtualKey)
    {
        return new KeysConverter().ConvertToString((Keys)virtualKey);
    }

    private void SettingsPanelKeyDown(object sender, KeyEventArgs e)
    {
        if (capturingExitKey)
        {
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey ||
                e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
                return;

            selectedExitKey = (int)e.KeyCode;
            capturingExitKey = false;
            UpdateKeyButtonLabels();
            e.SuppressKeyPress = true;
            return;
        }

        if (capturingKeyIndex < 0)
            return;

        if (e.KeyCode == Keys.Escape)
        {
            capturingKeyIndex = -1;
            UpdateKeyButtonLabels();
            e.SuppressKeyPress = true;
            return;
        }

        if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey ||
            e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            return;

        selectedKeys[capturingKeyIndex] = (int)e.KeyCode;
        capturingKeyIndex = -1;
        UpdateKeyButtonLabels();
        e.SuppressKeyPress = true;
    }

    private void UpdateKeyButtonLabels()
    {
        for (int i = 0; i < keyButtons.Length; i++)
        {
            if (keyButtons[i] == null)
                continue;
            keyButtons[i].Text = i == capturingKeyIndex
                ? "BAS..."
                : GetKeyName(selectedKeys[i]);
            keyButtons[i].ForeColor = i == capturingKeyIndex ? AccentBlue : TextColor;
        }
        exitKeyButton.Text = capturingExitKey ? "BAS..." : GetKeyName(selectedExitKey);
        exitKeyButton.ForeColor = capturingExitKey ? AccentBlue : AccentRed;
    }

    private void RefreshThemeCards()
    {
        for (int i = 0; i < themeCards.Length; i++)
        {
            if (themeCards[i] != null)
                themeCards[i].Invalidate();
        }
        ThemeColors colors = ThemePalette.Get(selectedTheme);
        this.BackColor = colors.BackgroundStart;
        foreach (Panel row in keyRows)
            if (row != null)
                row.BackColor = colors.Surface;
        foreach (Control control in GetAllControls(this))
        {
            Label label = control as Label;
            if (label != null)
            {
                string role = label.Tag as string;
                int cardIndex = Array.IndexOf(themeCards, label.Parent);
                if (cardIndex >= 0)
                {
                    ThemeColors cardColors = ThemePalette.Get((OverlayTheme)cardIndex);
                    label.ForeColor = role == "theme-primary" ? cardColors.Primary : cardColors.MutedText;
                }
                else if (role == "muted") label.ForeColor = colors.MutedText;
                else if (role == "primary") label.ForeColor = colors.Primary;
                else if (role == "secondary") label.ForeColor = colors.Secondary;
                else label.ForeColor = colors.Text;
            }

            Button button = control as Button;
            if (button != null)
            {
                button.BackColor = colors.Surface;
                button.FlatAppearance.BorderColor = Color.FromArgb(100, colors.Primary);
                button.ForeColor = button.Text == "VAZGEÇ" ? colors.MutedText :
                    (button.Text == "×" ? colors.Secondary : colors.Primary);
            }
        }
        Invalidate();
    }

    private static System.Collections.Generic.IEnumerable<Control> GetAllControls(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (Control descendant in GetAllControls(child))
                yield return descendant;
        }
    }

    private void ApplyButtonClick(object sender, EventArgs e)
    {
        for (int i = 0; i < selectedKeys.Length; i++)
        {
            if (selectedKeys[i] == selectedExitKey)
            {
                MessageBox.Show(this, "Çıkış tuşu, oyun göstergelerinde kullanılan bir tuşla aynı olamaz.",
                    "Çakışan tuş", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for (int j = i + 1; j < selectedKeys.Length; j++)
            {
                if (selectedKeys[i] == selectedKeys[j])
                {
                    MessageBox.Show(this, "Her gösterge için farklı bir tuş seçmelisin.",
                        "Geçersiz tuşlar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        if (applySettings((int[])selectedKeys.Clone(), selectedTheme, selectedExitKey))
            this.Close();
    }
}
