using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Reflection;

public class FNFOverlay : Form
{
    // ==========================================
    // WIN32 API KANCALARI
    // ==========================================
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", ExactSpelling = true)] private static extern IntPtr GetDC(IntPtr hdc);
    [DllImport("user32.dll", ExactSpelling = true)] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll", ExactSpelling = true)] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport("gdi32.dll", ExactSpelling = true)] private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll", ExactSpelling = true)] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll", ExactSpelling = true)] private static extern bool DeleteObject(IntPtr hObject);
    [DllImport("user32.dll", ExactSpelling = true)] private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

    // ==========================================
    // WIN32 STRUCT YAPILARI
    // ==========================================
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int x; public int y; }
    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE { public int cx; public int cy; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BLENDFUNCTION { public byte BlendOp; public byte BlendFlags; public byte SourceConstantAlpha; public byte AlphaFormat; }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const byte AC_SRC_OVER = 0x00;
    private const byte AC_SRC_ALPHA = 0x01;
    private const uint ULW_ALPHA = 0x00000002;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOACTIVATE = 0x0010;

    private string[] fnfTargetWindows = new string[0];

    [DataContract]
    private class SupportedGamesConfig
    {
        [DataMember(Name = "games", IsRequired = true)]
        public string[] Games { get; set; }
    }

    // 🎨 Renk Paletleri
    private Color bfDarkBlue;
    private Color bfLightBlue;
    private Color bfLightRed;
    private Color bfDarkRed;
    private Color neonBorder;
    private Color bfAccentSecondary;

    private Color dragYellow;
    private Color dragYellowDark;
    private Color dragYellowGlow;

    private int btnWidth = 44;
    private int btnHeight = 42;
    private int spacing = 6;
    private int centerGap = 16;
    private const int OverlayLogicalWidth = 334;
    private const int OverlayLogicalHeight = 320;

    private int[] keys = { 0x51, 0x57, 0x42, 0x4F, 0x50, 0x20 };
    private string[] keyNames = { "Q", "W", "B", "O", "P", "Space" };
    private int exitKey = 0x7B;
    private int[] keyX = new int[6];
    private bool[] keyStates = new bool[6];
    private const int SustainHoldThresholdFrames = 8;
    private readonly Queue<DateTime> recentKeyPresses = new Queue<DateTime>();
    private DateTime lastTrackedKeyPress;
    private long lastKeyIntervalMilliseconds;
    private bool hasTrackedKeyPress;
    private bool hasKeyInterval;
    private FNFNote[] heldNotes = new FNFNote[6];
    private class FNFNote
    {
        public int X;
        public float Y;
        public float SustainLength;
        public int HoldFrames;
        public bool IsHolding;
        public bool SustainStarted;
    }
    private List<FNFNote> activeNotes = new List<FNFNote>();

    private Timer gameLoop = new Timer();
    private PrivateFontCollection pfc = new PrivateFontCollection();
    private Font fnfFontLarge;
    private Font fnfFontMedium;
    private Font fnfFontSmall;

    private bool isTransparent = true;
    private bool manualPosition = false;
    private bool isDragging = false;
    private Point dragStartCursor = new Point(0, 0);
    private Point dragStartForm = new Point(0, 0);

    private int dragAnimTick = 0;
    private float dragPulseAlpha = 0f;
    private bool dragPulseGrowing = true;

    private string presetNotification = "";
    private int presetNotifyTimer = 0;
    private float presetNotifyAlpha = 0f;
    private bool[] presetCooldown = { false, false, false, false };

    private bool isGameFound = false;

    // Paneller ve Çıkış Kontrolü
    private CreatorsPanel creatorsForm = null;
    private bool cKeyCooldown = false;
    private InfoPanel infoForm = null;
    private bool hKeyCooldown = false;
    private SettingsPanel settingsForm = null;
    private bool sKeyCooldown = false;
    private OverlayTheme currentTheme = OverlayTheme.Default;
    private bool isExiting = false; // Çift tetiklemeyi önlemek için

    public FNFOverlay()
    {
        LoadSettings();
        ApplyTheme(currentTheme);
        LoadSupportedGames();

        keyX[0] = 10;
        keyX[1] = keyX[0] + btnWidth + spacing;
        keyX[2] = keyX[1] + btnWidth + centerGap;
        keyX[3] = keyX[2] + btnWidth + centerGap;
        keyX[4] = keyX[3] + btnWidth + spacing;
        keyX[5] = keyX[4] + btnWidth + spacing;

        int totalWidth = keyX[5] + btnWidth + 10;
        this.Size = new Size(totalWidth, OverlayLogicalHeight);

        this.FormBorderStyle = FormBorderStyle.None;
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.StartPosition = FormStartPosition.Manual;

        string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VCR_OSD_MONO.ttf");
        if (File.Exists(fontPath))
        {
            try
            {
                pfc.AddFontFile(fontPath);
                fnfFontLarge = new Font(pfc.Families[0], 14, FontStyle.Bold);
                fnfFontMedium = new Font(pfc.Families[0], 10, FontStyle.Bold);
                fnfFontSmall = new Font(pfc.Families[0], 8, FontStyle.Regular);
            }
            catch { LoadFallbackFonts(); }
        }
        else
        {
            LoadFallbackFonts();
        }

        gameLoop.Interval = 15;
        gameLoop.Tick += UpdateFrame;
    }

    private void LoadFallbackFonts()
    {
        try
        {
            fnfFontLarge = new Font("VCR OSD Mono", 14, FontStyle.Bold);
            fnfFontMedium = new Font("VCR OSD Mono", 10, FontStyle.Bold);
            fnfFontSmall = new Font("VCR OSD Mono", 8, FontStyle.Regular);
        }
        catch
        {
            fnfFontLarge = new Font("Arial", 14, FontStyle.Bold);
            fnfFontMedium = new Font("Arial", 10, FontStyle.Bold);
            fnfFontSmall = new Font("Arial", 8, FontStyle.Regular);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        SetClickThrough(true);
        gameLoop.Start();
    }

    private void SetClickThrough(bool clickThrough)
    {
        int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        if (clickThrough)
        {
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        }
        else
        {
            SetWindowLong(this.Handle, GWL_EXSTYLE, (initialStyle | WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_TOOLWINDOW) & ~WS_EX_TRANSPARENT & ~WS_EX_NOACTIVATE);
        }
    }

    private void MoveOverlayTo(int x, int y)
    {
        if (this.Left == x && this.Top == y)
            return;

        SetWindowPos(this.Handle, HWND_TOPMOST, x, y, this.Width, this.Height, SWP_NOACTIVATE);
    }

    protected override CreateParams CreateParams
    {
        get { CreateParams cp = base.CreateParams; cp.ExStyle |= WS_EX_TOPMOST | WS_EX_NOACTIVATE | WS_EX_LAYERED; return cp; }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && !isTransparent)
        {
            isDragging = true;
            manualPosition = true;
            dragStartCursor = Cursor.Position;
            dragStartForm = this.Location;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (isDragging)
        {
            Point currentCursor = Cursor.Position;
            int deltaX = currentCursor.X - dragStartCursor.X;
            int deltaY = currentCursor.Y - dragStartCursor.Y;
            this.Location = new Point(dragStartForm.X + deltaX, dragStartForm.Y + deltaY);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        isDragging = false;
    }

    private void ApplyPreset(int presetNumber)
    {
        Screen screen = Screen.FromControl(this);
        Rectangle screenBounds = screen.WorkingArea;
        int screenW = screenBounds.Width;
        int screenH = screenBounds.Height;
        int margin = 30;
        int targetX = 0;
        int targetY = 0;

        switch (presetNumber)
        {
            case 1:
                targetX = screenBounds.Left + margin;
                targetY = screenBounds.Top + screenH - this.Height - margin - 40;
                presetNotification = "SOL ALT KÖŞE";
                break;
            case 2:
                targetX = screenBounds.Left + screenW - this.Width - margin;
                targetY = screenBounds.Top + screenH - this.Height - margin - 40;
                presetNotification = "SAĞ ALT KÖŞE";
                break;
            case 3:
                targetX = screenBounds.Left + margin;
                targetY = screenBounds.Top + (screenH - this.Height) / 2;
                presetNotification = "SOL ORTA KÖŞE";
                break;
            case 4:
                targetX = screenBounds.Left + screenW - this.Width - margin;
                targetY = screenBounds.Top + (screenH - this.Height) / 2;
                presetNotification = "SAĞ ORTA KÖŞE";
                break;
        }

        manualPosition = true;
        this.Location = new Point(targetX, targetY);
        MoveOverlayTo(targetX, targetY);
        presetNotifyTimer = 90;
        presetNotifyAlpha = 255f;
    }

    private void ToggleCreatorsPanel()
    {
        if (creatorsForm == null || creatorsForm.IsDisposed)
        {
            creatorsForm = new CreatorsPanel();
            creatorsForm.Show();
        }
        else
        {
            creatorsForm.Close();
            creatorsForm = null;
        }
    }

    private void ToggleInfoPanel()
    {
        if (infoForm == null || infoForm.IsDisposed)
        {
            infoForm = new InfoPanel(keyNames,
                new KeysConverter().ConvertToString((Keys)exitKey));
            infoForm.Show();
        }
        else
        {
            infoForm.Close();
            infoForm = null;
        }
    }

    private void ToggleSettingsPanel()
    {
        if (settingsForm == null || settingsForm.IsDisposed)
        {
            settingsForm = new SettingsPanel(keys, currentTheme, exitKey,
                Screen.FromControl(this), SaveAndApplySettings);
            settingsForm.FormClosed += delegate { settingsForm = null; };
            settingsForm.Show();
            settingsForm.BringToFront();
        }
        else
        {
            settingsForm.Close();
            settingsForm = null;
        }
    }

    private string GetSettingsPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FNFOverlay",
            "settings.ini");
    }

    private void LoadSupportedGames()
    {
        string fileName = "supported_games.json";
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", fileName);
        Stream configStream = null;
        string configSource = configPath;

        try
        {
            if (File.Exists(configPath))
                configStream = File.OpenRead(configPath);
            else
            {
                configStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("SupportedGames.json");
                configSource = "EXE içindeki varsayılan oyun listesi";
            }

            if (configStream == null)
            {
                ShowSupportedGamesError(configPath, "JSON dosyası ve EXE içindeki varsayılan liste bulunamadı.");
                return;
            }

            SupportedGamesConfig config;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SupportedGamesConfig));
            using (configStream)
                config = (SupportedGamesConfig)serializer.ReadObject(configStream);

            if (config == null || config.Games == null)
                throw new SerializationException("JSON içinde 'games' dizisi bulunamadı.");

            List<string> gameNames = new List<string>();
            HashSet<string> uniqueNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string game in config.Games)
            {
                if (string.IsNullOrWhiteSpace(game))
                    continue;

                string gameName = game.Trim();
                if (uniqueNames.Add(gameName))
                    gameNames.Add(gameName);
            }

            if (gameNames.Count == 0)
                throw new SerializationException("'games' dizisinde en az bir oyun adı olmalı.");

            fnfTargetWindows = gameNames.ToArray();
        }
        catch (IOException ex)
        {
            ShowSupportedGamesError(configSource, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowSupportedGamesError(configSource, ex.Message);
        }
        catch (SerializationException ex)
        {
            ShowSupportedGamesError(configSource, ex.Message);
        }
    }

    private static void ShowSupportedGamesError(string configPath, string error)
    {
        MessageBox.Show(
            "Desteklenen oyun listesi okunamadı:\n" + configPath + "\n\n" + error +
            "\n\nOtomatik oyun takibi bu oturumda kapalı olacak.",
            "Oyun listesi yüklenemedi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void LoadSettings()
    {
        string settingsPath = GetSettingsPath();
        if (!File.Exists(settingsPath))
            return;

        try
        {
            string[] lines = File.ReadAllLines(settingsPath);
            OverlayTheme loadedTheme;
            string[] storedKeys = lines.Length >= 2 ? lines[1].Split(',') : new string[0];
            int[] loadedKeys = (int[])keys.Clone();
            HashSet<int> uniqueKeys = new HashSet<int>();

            if ((lines.Length < 2 || lines.Length > 3) ||
                !Enum.TryParse<OverlayTheme>(lines[0], out loadedTheme) ||
                !ThemePalette.IsValid(loadedTheme) ||
                (storedKeys.Length != keys.Length && storedKeys.Length != keys.Length - 1))
                throw new FormatException("Ayar dosyasının biçimi tanınmadı.");

            for (int i = 0; i < storedKeys.Length; i++)
            {
                int virtualKey;
                if (!int.TryParse(storedKeys[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out virtualKey) ||
                    virtualKey < 1 || virtualKey > 255 || !uniqueKeys.Add(virtualKey))
                    throw new FormatException("Ayar dosyasında geçersiz veya yinelenen bir tuş var.");
                loadedKeys[i] = virtualKey;
            }

            if (storedKeys.Length == keys.Length - 1 && uniqueKeys.Contains(loadedKeys[keys.Length - 1]))
            {
                int[] migrationKeys = { 0x20, 0x09, 0x0D, 0x1B };
                bool foundAvailableKey = false;
                foreach (int migrationKey in migrationKeys)
                {
                    if (uniqueKeys.Add(migrationKey))
                    {
                        loadedKeys[keys.Length - 1] = migrationKey;
                        foundAvailableKey = true;
                        break;
                    }
                }

                if (!foundAvailableKey)
                    throw new FormatException("Dodge için kullanılabilir varsayılan tuş bulunamadı.");
            }

            int loadedExitKey = exitKey;
            bool hasSavedExitKey = lines.Length == 3 &&
                lines[2].StartsWith("ExitKey=", StringComparison.Ordinal);
            if (hasSavedExitKey)
            {
                if (!int.TryParse(lines[2].Substring("ExitKey=".Length), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out loadedExitKey) ||
                    loadedExitKey < 1 || loadedExitKey > 255 || !uniqueKeys.Add(loadedExitKey))
                    throw new FormatException("Ayar dosyasındaki çıkış tuşu geçersiz veya başka bir tuşla aynı.");
            }
            else if (!uniqueKeys.Add(loadedExitKey))
            {
                int[] fallbackExitKeys = { 0x7B, 0x7A, 0x79, 0x78 };
                bool foundAvailableExitKey = false;
                foreach (int candidate in fallbackExitKeys)
                {
                    if (uniqueKeys.Add(candidate))
                    {
                        loadedExitKey = candidate;
                        foundAvailableExitKey = true;
                        break;
                    }
                }

                if (!foundAvailableExitKey)
                    throw new FormatException("Uygulamadan çıkış için kullanılabilir varsayılan tuş bulunamadı.");
            }

            Array.Copy(loadedKeys, keys, keys.Length);
            currentTheme = loadedTheme;
            exitKey = loadedExitKey;
            UpdateKeyNames();
        }
        catch (IOException ex)
        {
            MessageBox.Show("Ayarlar okunamadı. Varsayılan ayarlar kullanılacak.\n\n" + ex.Message,
                "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show("Ayarlar okunamadı. Varsayılan ayarlar kullanılacak.\n\n" + ex.Message,
                "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (FormatException ex)
        {
            MessageBox.Show("Ayar dosyası geçersiz. Varsayılan ayarlar kullanılacak.\n\n" + ex.Message,
                "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private bool SaveAndApplySettings(int[] newKeys, OverlayTheme theme, int newExitKey)
    {
        if (newExitKey < 1 || newExitKey > 255)
            throw new ArgumentOutOfRangeException("newExitKey");
        for (int i = 0; i < newKeys.Length; i++)
            if (newKeys[i] == newExitKey)
            {
                MessageBox.Show("Çıkış tuşu, oyun göstergelerinde kullanılan bir tuşla aynı olamaz.",
                    "Çakışan tuş", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

        string settingsPath = GetSettingsPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            string[] lines = new string[3];
            lines[0] = theme.ToString();
            lines[1] = string.Join(",", Array.ConvertAll(newKeys, delegate(int key)
            {
                return key.ToString(CultureInfo.InvariantCulture);
            }));
            lines[2] = "ExitKey=" + newExitKey.ToString(CultureInfo.InvariantCulture);
            File.WriteAllLines(settingsPath, lines);
        }
        catch (IOException ex)
        {
            MessageBox.Show("Ayarlar kaydedilemedi.\n\n" + ex.Message,
                "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show("Ayarlar kaydedilemedi.\n\n" + ex.Message,
                "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        Array.Copy(newKeys, keys, keys.Length);
        currentTheme = theme;
        exitKey = newExitKey;
        UpdateKeyNames();
        ApplyTheme(currentTheme);
        if (infoForm != null && !infoForm.IsDisposed)
            infoForm.UpdateExitKey(new KeysConverter().ConvertToString((Keys)exitKey));
        Array.Clear(keyStates, 0, keyStates.Length);
        Array.Clear(heldNotes, 0, heldNotes.Length);
        activeNotes.Clear();
        return true;
    }

    private void UpdateKeyNames()
    {
        KeysConverter converter = new KeysConverter();
        for (int i = 0; i < keys.Length; i++)
            keyNames[i] = converter.ConvertToString((Keys)keys[i]);
    }

    private void ApplyTheme(OverlayTheme theme)
    {
        ThemePalette.SetCurrent(theme);
        ThemeColors colors = ThemePalette.Get(theme);
        bfDarkBlue = colors.BackgroundStart;
        bfLightBlue = colors.Primary;
        bfLightRed = colors.Secondary;
        bfDarkRed = ControlPaint.Dark(colors.Secondary, 0.55f);
        neonBorder = colors.Highlight;
        bfAccentSecondary = colors.Secondary;
        dragYellow = colors.Primary;
        dragYellowDark = ControlPaint.Dark(colors.Primary, 0.55f);
        dragYellowGlow = colors.Highlight;
    }

    private void UpdateFrame(object sender, EventArgs e)
    {
        if (isExiting) return;

        dragAnimTick++;
        DateTime now = DateTime.UtcNow;
        DateTime oneSecondAgo = now.AddSeconds(-1);
        while (recentKeyPresses.Count > 0 && recentKeyPresses.Peek() < oneSecondAgo)
            recentKeyPresses.Dequeue();

        if (!isTransparent)
        {
            if (dragPulseGrowing)
            {
                dragPulseAlpha += 4f;
                if (dragPulseAlpha >= 120f) dragPulseGrowing = false;
            }
            else
            {
                dragPulseAlpha -= 4f;
                if (dragPulseAlpha <= 30f) dragPulseGrowing = true;
            }
        }

        if (presetNotifyTimer > 0)
        {
            presetNotifyTimer--;
            if (presetNotifyTimer < 20)
            {
                presetNotifyAlpha -= 12f;
                if (presetNotifyAlpha < 0) presetNotifyAlpha = 0;
            }
        }

        bool settingsOpen = settingsForm != null && !settingsForm.IsDisposed && settingsForm.Visible;
        if (!settingsOpen)
        {
            bool isAltSDown = (GetAsyncKeyState(0x12) & 0x8000) != 0 &&
                (GetAsyncKeyState(0x53) & 0x8000) != 0;
            if (isAltSDown)
            {
                if (!sKeyCooldown)
                {
                    sKeyCooldown = true;
                    ToggleSettingsPanel();
                }
            }
            else
            {
                sKeyCooldown = false;
            }
        }
        settingsOpen = settingsForm != null && !settingsForm.IsDisposed && settingsForm.Visible;

        bool isCtrlDown = !settingsOpen && (GetAsyncKeyState(0x11) & 0x8000) != 0;
        if (!settingsOpen && isCtrlDown)
        {
            if (isTransparent) { SetClickThrough(false); isTransparent = false; }

            if ((GetAsyncKeyState(0x52) & 0x8000) != 0)
            {
                manualPosition = false;
                presetNotification = "OTOMATİK TAKİP MODU";
                presetNotifyTimer = 90;
                presetNotifyAlpha = 255f;
            }

            if ((GetAsyncKeyState(0x31) & 0x8000) != 0) { if (!presetCooldown[0]) { ApplyPreset(1); presetCooldown[0] = true; } } else { presetCooldown[0] = false; }
            if ((GetAsyncKeyState(0x32) & 0x8000) != 0) { if (!presetCooldown[1]) { ApplyPreset(2); presetCooldown[1] = true; } } else { presetCooldown[1] = false; }
            if ((GetAsyncKeyState(0x33) & 0x8000) != 0) { if (!presetCooldown[2]) { ApplyPreset(3); presetCooldown[2] = true; } } else { presetCooldown[2] = false; }
            if ((GetAsyncKeyState(0x34) & 0x8000) != 0) { if (!presetCooldown[3]) { ApplyPreset(4); presetCooldown[3] = true; } } else { presetCooldown[3] = false; }
        }
        else if (!settingsOpen)
        {
            if (!isTransparent) { SetClickThrough(true); isTransparent = true; isDragging = false; }
        }

        // C Tuşu (Yapımcı Kartı)
        bool isCDown = !settingsOpen && (GetAsyncKeyState(0x43) & 0x8000) != 0;
        if (isCDown)
        {
            if (!cKeyCooldown)
            {
                cKeyCooldown = true;
                ToggleCreatorsPanel();
            }
        }
        else
        {
            cKeyCooldown = false;
        }

        // H Tuşu (Info Paneli)
        bool isHDown = !settingsOpen && (GetAsyncKeyState(0x48) & 0x8000) != 0;
        if (isHDown)
        {
            if (!hKeyCooldown)
            {
                hKeyCooldown = true;
                ToggleInfoPanel();
            }
        }
        else
        {
            hKeyCooldown = false;
        }

        // Oyun Penceresi Takibi
        if (!settingsOpen)
        {
            IntPtr activeHwnd = GetForegroundWindow();
            StringBuilder windowTitle = new StringBuilder(256);
            GetWindowText(activeHwnd, windowTitle, 256);

            isGameFound = false;
            foreach (string target in fnfTargetWindows)
            {
                if (windowTitle.ToString().IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    RECT rect;
                    if (GetWindowRect(activeHwnd, out rect))
                    {
                        isGameFound = true;

                        if (!manualPosition)
                        {
                            int gameWidth = rect.Right - rect.Left;
                            int gameHeight = rect.Bottom - rect.Top;
                            int targetX = rect.Left + gameWidth - this.Width - 30;
                            int targetY = rect.Top + gameHeight - this.Height - 30;
                            SetWindowPos(this.Handle, HWND_TOPMOST, targetX, targetY, this.Width, this.Height, SWP_NOACTIVATE);
                        }
                        break;
                    }
                }
            }

            if (!isGameFound)
            {
                Screen currentScreen = Screen.FromControl(this);

                if (!manualPosition)
                {
                    Rectangle workingArea = currentScreen.WorkingArea;
                    int defaultX = workingArea.Right - this.Width - 40;
                    int defaultY = workingArea.Bottom - this.Height - 80;
                    MoveOverlayTo(defaultX, defaultY);
                }
            }

            // Klavye Tuşları
            for (int i = 0; i < keys.Length; i++)
            {
                bool isDown = (GetAsyncKeyState(keys[i]) & 0x8000) != 0;
                int panelY = OverlayLogicalHeight - btnHeight - 12;

                if (isDown)
                {
                    if (!keyStates[i])
                    {
                        keyStates[i] = true;
                        if (hasTrackedKeyPress)
                        {
                            lastKeyIntervalMilliseconds = (long)(now - lastTrackedKeyPress).TotalMilliseconds;
                            hasKeyInterval = true;
                        }
                        lastTrackedKeyPress = now;
                        hasTrackedKeyPress = true;
                        recentKeyPresses.Enqueue(now);

                        FNFNote note = new FNFNote
                        {
                            X = keyX[i] + 7,
                            Y = panelY - 6,
                            IsHolding = true
                        };
                        heldNotes[i] = note;
                        activeNotes.Add(note);
                    }
                }
                else
                {
                    keyStates[i] = false;
                    if (heldNotes[i] != null)
                    {
                        heldNotes[i].IsHolding = false;
                        heldNotes[i] = null;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < heldNotes.Length; i++)
            {
                keyStates[i] = false;
                if (heldNotes[i] != null)
                {
                    heldNotes[i].IsHolding = false;
                    heldNotes[i] = null;
                }
            }
        }

        // Notaların Hareketi
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            FNFNote note = activeNotes[i];
            note.Y -= 8;
            if (note.IsHolding)
            {
                note.HoldFrames++;
                if (note.HoldFrames >= SustainHoldThresholdFrames)
                {
                    note.SustainStarted = true;
                    note.SustainLength += 8;
                }
            }
            if (note.Y + 36 + note.SustainLength < 5)
                activeNotes.RemoveAt(i);
        }

        RenderAndUploadLayeredWindow();

        // Çıkış, Ayarlar panelinde seçilen tuşla tetiklenir.
        if (!settingsOpen && (GetAsyncKeyState(exitKey) & 0x8000) != 0 && !isExiting)
        {
            isExiting = true;
            gameLoop.Stop();
            this.Hide(); // Overlay'i hemen gizle

            // Açık paneller varsa kapat
            if (creatorsForm != null && !creatorsForm.IsDisposed) creatorsForm.Close();
            if (infoForm != null && !infoForm.IsDisposed) infoForm.Close();
            if (settingsForm != null && !settingsForm.IsDisposed) settingsForm.Close();

            // Veda Panelini Başlat
            GoodByePanel bye = new GoodByePanel();
            bye.Show();
        }
    }

    private void RenderAndUploadLayeredWindow()
    {
        IntPtr screenDc = GetDC(IntPtr.Zero);
        IntPtr memDc = CreateCompatibleDC(screenDc);
        IntPtr hBitmap = IntPtr.Zero;
        IntPtr oldBitmap = IntPtr.Zero;

        try
        {
            using (Bitmap bitmap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.Clear(Color.FromArgb(0, 0, 0, 0));
                    AnimationController.DrawBackground(g,
                        new Rectangle(0, 0, OverlayLogicalWidth, OverlayLogicalHeight),
                        ThemePalette.Current, dragAnimTick);

                    int panelY = OverlayLogicalHeight - btnHeight - 12;

                    if (!isTransparent)
                    {
                        int pulseA = (int)dragPulseAlpha;
                        using (Brush glowBrush = new SolidBrush(Color.FromArgb(pulseA / 3, dragYellow)))
                        {
                            g.FillRectangle(glowBrush, 0, 0, OverlayLogicalWidth, OverlayLogicalHeight);
                        }
                        using (Pen dragPen = new Pen(Color.FromArgb(180 + pulseA / 2, dragYellow), 3))
                        {
                            dragPen.DashStyle = DashStyle.Dash;
                            dragPen.DashOffset = dragAnimTick % 20;
                            g.DrawRectangle(dragPen, 3, 3, OverlayLogicalWidth - 6, OverlayLogicalHeight - 6);
                        }

                        int cornerSize = 10;
                        using (Brush cornerBrush = new SolidBrush(Color.FromArgb(200, dragYellow)))
                        {
                            g.FillRectangle(cornerBrush, 3, 3, cornerSize, 3); g.FillRectangle(cornerBrush, 3, 3, 3, cornerSize);
                            g.FillRectangle(cornerBrush, OverlayLogicalWidth - 3 - cornerSize, 3, cornerSize, 3); g.FillRectangle(cornerBrush, OverlayLogicalWidth - 6, 3, 3, cornerSize);
                            g.FillRectangle(cornerBrush, 3, OverlayLogicalHeight - 6, cornerSize, 3); g.FillRectangle(cornerBrush, 3, OverlayLogicalHeight - 3 - cornerSize, 3, cornerSize);
                            g.FillRectangle(cornerBrush, OverlayLogicalWidth - 3 - cornerSize, OverlayLogicalHeight - 6, cornerSize, 3); g.FillRectangle(cornerBrush, OverlayLogicalWidth - 6, OverlayLogicalHeight - 3 - cornerSize, 3, cornerSize);
                        }

                        using (Brush darkBrush = new SolidBrush(Color.FromArgb(230, bfDarkBlue)))
                        {
                            g.FillRectangle(darkBrush, 8, 8, OverlayLogicalWidth - 16, 52);
                        }
                        using (Pen panelBorder = new Pen(Color.FromArgb(190, dragYellowDark), 1))
                        {
                            g.DrawRectangle(panelBorder, 8, 8, OverlayLogicalWidth - 16, 52);
                        }

                        using (Brush titleBrush = new SolidBrush(dragYellow))
                        {
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                            g.DrawString("SURUKLEME MODU AKTIF", fnfFontMedium, titleBrush, new RectangleF(8, 12, OverlayLogicalWidth - 16, 16), sf);
                        }
                        
                        using (Brush infoBrush = new SolidBrush(dragYellowGlow))
                        {
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                            string infoText = "CTRL+1..4: Hazir Konumlar | CTRL+R: Oto\nTus Yardimi Icin [H] Tusuna Basin";
                            g.DrawString(infoText, fnfFontSmall, infoBrush, new RectangleF(8, 28, OverlayLogicalWidth - 16, 32), sf);
                        }
                    }

                    if (presetNotifyTimer > 0 && presetNotifyAlpha > 0 && isTransparent)
                    {
                        int notifyW = 180;
                        int notifyH = 36;
                        int notifyX = (OverlayLogicalWidth - notifyW) / 2;
                        int notifyY = 10;
                        int alpha = (int)presetNotifyAlpha;
                        if (alpha > 255) alpha = 255;

                        ThemeColors notificationColors = ThemePalette.Get(currentTheme);
                        Color notificationAccent = notificationColors.Primary;
                        Color notificationBackground = notificationColors.BackgroundStart;
                        using (Brush notifyBg = new SolidBrush(Color.FromArgb(alpha * 3 / 4, notificationBackground)))
                        {
                            g.FillRectangle(notifyBg, notifyX, notifyY, notifyW, notifyH);
                        }
                        using (Pen notifyBorder = new Pen(Color.FromArgb(alpha, notificationAccent), 2))
                        {
                            g.DrawRectangle(notifyBorder, notifyX, notifyY, notifyW, notifyH);
                        }
                        using (Brush notifyText = new SolidBrush(Color.FromArgb(alpha, notificationAccent)))
                        {
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(presetNotification, fnfFontMedium, notifyText, new RectangleF(notifyX, notifyY, notifyW, notifyH), sf);
                        }
                    }

                    foreach (var note in activeNotes)
                    {
                        int noteY = (int)note.Y;
                        int sustainLength = (int)note.SustainLength;
                        Rectangle noteRect = new Rectangle(note.X, noteY, 30, 36);

                        int noteAlpha = (int)Math.Max(0, Math.Min(255, ((note.Y + 36 + note.SustainLength) / 260f) * 255));
                        if (noteAlpha < 5) continue; 

                        if (note.SustainStarted && sustainLength > 0)
                        {
                            Rectangle sustainRect = new Rectangle(note.X + 7, noteY + 30, 16, sustainLength + 6);
                            Rectangle visibleSustain = Rectangle.Intersect(
                                sustainRect, new Rectangle(0, 0, OverlayLogicalWidth, panelY + btnHeight));
                            if (visibleSustain.Width > 0 && visibleSustain.Height > 0)
                            {
                                using (LinearGradientBrush sustainBrush = new LinearGradientBrush(
                                    sustainRect,
                                    Color.FromArgb(noteAlpha, bfLightBlue),
                                    Color.FromArgb(noteAlpha, bfDarkRed),
                                    LinearGradientMode.Horizontal))
                                    g.FillRectangle(sustainBrush, visibleSustain);

                                using (Pen sustainOutline = new Pen(Color.FromArgb(noteAlpha * 190 / 255, neonBorder), 1))
                                    g.DrawRectangle(sustainOutline, visibleSustain.X, visibleSustain.Y,
                                        Math.Max(0, visibleSustain.Width - 1), Math.Max(0, visibleSustain.Height - 1));
                            }
                        }

                        using (Brush shadowBrush = new SolidBrush(Color.FromArgb(noteAlpha * 60 / 255, 0, 0, 0)))
                        {
                            g.FillRectangle(shadowBrush, note.X + 3, noteY + 3, 30, 36);
                        }

                        Color startColor = Color.FromArgb(noteAlpha, bfLightRed);
                        Color endColor = Color.FromArgb(noteAlpha, bfDarkRed);
                        using (LinearGradientBrush lgb = new LinearGradientBrush(noteRect, startColor, endColor, LinearGradientMode.Vertical))
                        {
                            g.FillRectangle(lgb, noteRect);
                        }

                        using (Brush glossBrush = new SolidBrush(Color.FromArgb(noteAlpha * 80 / 255, 255, 255, 255)))
                        {
                            g.FillRectangle(glossBrush, note.X + 2, noteY + 2, 26, 6);
                        }

                        using (Pen strokePen = new Pen(Color.FromArgb(noteAlpha * 230 / 255, bfDarkBlue), 2))
                        {
                            g.DrawRectangle(strokePen, noteRect);
                        }
                    }

                    for (int i = 0; i < keys.Length; i++)
                    {
                        bool pressed = keyStates[i];
                        bool inDragMode = !isTransparent;

                        Color bg, border, textC;
                        int borderW;

                        int pressOffset = pressed ? 3 : 0;
                        int currentKeyY = panelY + pressOffset;

                        if (inDragMode)
                        {
                            bg = pressed ? dragYellow : bfDarkBlue;
                            border = pressed ? dragYellowGlow : dragYellowDark;
                            textC = pressed ? bfDarkBlue : dragYellow;
                            borderW = pressed ? 3 : 2;
                        }
                        else
                        {
                            if (pressed)
                            {
                                bg = bfLightBlue;
                                border = neonBorder;
                                textC = bfDarkBlue;
                            }
                            else
                            {
                                bg = bfDarkBlue;
                                border = bfAccentSecondary;
                                textC = bfLightBlue;
                            }

                            borderW = pressed ? 3 : 2;
                        }

                        if (!pressed)
                        {
                            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, 8, 12, 24)))
                            {
                                g.FillRectangle(shadowBrush, keyX[i] + 3, panelY + 3, btnWidth, btnHeight);
                            }
                        }

                        using (Brush b = pressed
                            ? (Brush)new LinearGradientBrush(
                                new Rectangle(keyX[i], currentKeyY, btnWidth, btnHeight - pressOffset),
                                neonBorder,
                                bfLightBlue,
                                LinearGradientMode.Vertical)
                            : new SolidBrush(bg))
                        {
                            g.FillRectangle(b, keyX[i], currentKeyY, btnWidth, btnHeight - pressOffset);
                        }

                        if (pressed)
                        {
                            Color glowColor = inDragMode ? Color.FromArgb(40, dragYellow) : Color.FromArgb(40, bg);
                            using (Brush glowBrush = new SolidBrush(glowColor))
                            {
                                g.FillRectangle(glowBrush, keyX[i] - 5, currentKeyY - 5, btnWidth + 10, btnHeight + 10);
                            }
                        }

                        using (Pen arrowPen = new Pen(Color.FromArgb(pressed ? 40 : 20, textC), 2))
                        {
                            arrowPen.LineJoin = LineJoin.Round;
                            int centerX = keyX[i] + btnWidth / 2;
                            int centerY = currentKeyY + (btnHeight - pressOffset) / 2;

                            if (i == 0) 
                            {
                                g.DrawLine(arrowPen, centerX + 6, centerY, centerX - 6, centerY);
                                g.DrawLine(arrowPen, centerX - 6, centerY, centerX - 2, centerY - 4);
                                g.DrawLine(arrowPen, centerX - 6, centerY, centerX - 2, centerY + 4);
                            }
                            else if (i == 1) 
                            {
                                g.DrawLine(arrowPen, centerX, centerY - 6, centerX, centerY + 6);
                                g.DrawLine(arrowPen, centerX, centerY + 6, centerX - 4, centerY + 2);
                                g.DrawLine(arrowPen, centerX, centerY + 6, centerX + 4, centerY + 2);
                            }
                            else if (i == 3) 
                            {
                                g.DrawLine(arrowPen, centerX, centerY + 6, centerX, centerY - 6);
                                g.DrawLine(arrowPen, centerX, centerY - 6, centerX - 4, centerY - 2);
                                g.DrawLine(arrowPen, centerX, centerY - 6, centerX + 4, centerY - 2);
                            }
                            else if (i == 4) 
                            {
                                g.DrawLine(arrowPen, centerX - 6, centerY, centerX + 6, centerY);
                                g.DrawLine(arrowPen, centerX + 6, centerY, centerX + 2, centerY - 4);
                                g.DrawLine(arrowPen, centerX + 6, centerY, centerX + 2, centerY + 4);
                            }
                        }

                        if (!pressed)
                        {
                            using (Brush glossBrush = new SolidBrush(Color.FromArgb(25, 255, 255, 255)))
                            {
                                g.FillRectangle(glossBrush, keyX[i] + 2, currentKeyY + 2, btnWidth - 4, 5);
                            }
                        }

                        using (Pen p = new Pen(border, borderW))
                        {
                            g.DrawRectangle(p, keyX[i], currentKeyY, btnWidth, btnHeight - pressOffset);
                        }

                        using (Brush b = new SolidBrush(textC))
                        {
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(keyNames[i], fnfFontLarge, b, new RectangleF(keyX[i], currentKeyY, btnWidth, btnHeight - pressOffset), sf);
                        }
                    }
                    DrawKeyRateCounter(g);
                    AnimationController.DrawOverlay(g,
                        new Rectangle(0, 0, OverlayLogicalWidth, OverlayLogicalHeight),
                        ThemePalette.Current, dragAnimTick);
                }

                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBitmap = SelectObject(memDc, hBitmap);

                POINT pointSource = new POINT { x = 0, y = 0 };
                POINT topPos = new POINT { x = this.Left, y = this.Top };
                SIZE size = new SIZE { cx = this.Width, cy = this.Height };
                BLENDFUNCTION blend = new BLENDFUNCTION { BlendOp = AC_SRC_OVER, BlendFlags = 0, SourceConstantAlpha = 255, AlphaFormat = AC_SRC_ALPHA };
                UpdateLayeredWindow(this.Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, ULW_ALPHA);
            }
        }
        finally
        {
            if (hBitmap != IntPtr.Zero) { SelectObject(memDc, oldBitmap); DeleteObject(hBitmap); }
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    private void DrawKeyRateCounter(Graphics graphics)
    {
        ThemeColors colors = ThemePalette.Get(currentTheme);
        int width = Math.Min(92, OverlayLogicalWidth - 16);
        int height = 42;
        int x = OverlayLogicalWidth - width - 8;
        int y = 8;

        if (!isTransparent)
            y = 68;
        else if (presetNotifyTimer > 0 && presetNotifyAlpha > 0 &&
            x < (OverlayLogicalWidth + 180) / 2 && x + width > (OverlayLogicalWidth - 180) / 2)
            y = 52;

        y = Math.Max(4, Math.Min(y, OverlayLogicalHeight - height - 4));
        Rectangle bounds = new Rectangle(x, y, width, height);

        using (Brush background = new SolidBrush(Color.FromArgb(220, colors.BackgroundStart)))
            graphics.FillRectangle(background, bounds);
        using (Pen border = new Pen(Color.FromArgb(210, colors.Primary), 1))
            graphics.DrawRectangle(border, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

        using (Brush labelBrush = new SolidBrush(colors.MutedText))
            graphics.DrawString("TUS HIZI", fnfFontSmall, labelBrush,
                new PointF(bounds.X + 5, bounds.Y + 3));

        using (Brush cpsBrush = new SolidBrush(colors.Primary))
            graphics.DrawString("CPS: " + recentKeyPresses.Count.ToString(CultureInfo.InvariantCulture),
                fnfFontMedium, cpsBrush, new PointF(bounds.X + 5, bounds.Y + 15));

        string interval = hasKeyInterval
            ? lastKeyIntervalMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms"
            : "-- ms";
        using (Brush intervalBrush = new SolidBrush(colors.Secondary))
            graphics.DrawString(interval, fnfFontSmall, intervalBrush,
                new PointF(bounds.X + 5, bounds.Y + 29));
    }

    protected override void OnPaint(PaintEventArgs e) {}

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        FNFOverlay overlay = new FNFOverlay();
        LoadingPanel loader = new LoadingPanel();
        loader.Show();

        loader.UpdateStatus("Kaynaklar Kontrol Ediliyor...", 20);
        System.Threading.Thread.Sleep(600);
        loader.UpdateStatus("Oluşturucu Optimize Ediliyor...", 50);
        System.Threading.Thread.Sleep(600);
        loader.UpdateStatus("Arayüz Motoru Hazırlanıyor...", 80);
        System.Threading.Thread.Sleep(600);
        loader.UpdateStatus("Hazır! Çıkış için ayarladığın tuşa bas.", 100);

        while (loader.Created && loader.Opacity > 0) { Application.DoEvents(); System.Threading.Thread.Sleep(10); }

        Application.Run(overlay);
    }
}