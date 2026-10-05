using System.Diagnostics;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickBrowse;

internal sealed class MainForm : Form
{
    private const string HomeUrl = "quickbrowse://home";
    private const int MaxHistoryItems = 500;
    private static readonly Color ChromeColor = Color.FromArgb(24, 24, 34);
    private static readonly Color AccentColor = Color.FromArgb(184, 129, 255);
    private static readonly Color MutedColor = Color.FromArgb(185, 181, 204);
    private const string StartPageHtml = """
        <!doctype html>
        <html lang="ru">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>Главная — QuickBrowse</title>
          <style>
            * { box-sizing: border-box; }
            body {
              margin: 0; min-height: 100vh; color: #f7f4ff;
              font: 16px "Segoe UI", Arial, sans-serif;
              background:
                radial-gradient(ellipse at 76% 18%, #563c79 0, transparent 36%),
                radial-gradient(ellipse at 18% 82%, #233a62 0, transparent 42%),
                linear-gradient(135deg, #191a2b, #242038 56%, #171927);
              display: flex; justify-content: center; align-items: center;
              padding: 48px 24px;
            }
            main { width: min(920px, 100%); margin: auto; }
            .top { display: flex; align-items: center; gap: 14px; margin-bottom: 68px; }
            .logo {
              width: 48px; height: 48px; border-radius: 17px; display: grid;
              place-items: center; font-size: 27px; font-weight: 700;
              background: linear-gradient(145deg, #d39cff, #8368ff);
              box-shadow: 0 8px 30px #9270ff55;
            }
            .brand { font-size: 18px; font-weight: 650; letter-spacing: .2px; }
            .tagline { color: #aaa4bd; font-size: 13px; margin-top: 4px; }
            .hello { color: #c2a0ff; font-size: 14px; font-weight: 600; margin-bottom: 12px; }
            h1 { margin: 0; font-size: clamp(34px, 6vw, 54px); letter-spacing: -1.8px; }
            .subtitle { color: #b6b1c6; margin: 13px 0 30px; }
            .search {
              display: flex; align-items: center; gap: 13px; padding: 8px 9px 8px 20px;
              border: 1px solid #ffffff20; border-radius: 18px;
              background: #ffffff12; backdrop-filter: blur(18px);
              box-shadow: 0 18px 55px #08091138;
            }
            .search span { color: #c5bdd7; font-size: 20px; }
            input {
              flex: 1; min-width: 0; color: white; font: inherit; font-size: 15px;
              padding: 12px 0; border: 0; outline: 0; background: transparent;
            }
            input::placeholder { color: #aaa5b9; }
            .go {
              border: 0; border-radius: 12px; padding: 13px 22px; cursor: pointer;
              color: #201a2a; font: inherit; font-size: 14px; font-weight: 700;
              background: linear-gradient(135deg, #e0b3ff, #b9a4ff);
            }
            .section { margin: 40px 0 16px; color: #d7d1e3; font-size: 14px; font-weight: 600; }
            .tiles { display: grid; grid-template-columns: repeat(6, 1fr); gap: 14px; }
            .tile {
              color: #f5f2fa; text-decoration: none; text-align: center;
              padding: 18px 8px 15px; border-radius: 17px;
              background: #ffffff0b; border: 1px solid #ffffff10;
              transition: transform .18s, background .18s, border-color .18s;
            }
            .tile:hover { transform: translateY(-4px); background: #ffffff16; border-color: #d5baff55; }
            .icon {
              width: 46px; height: 46px; margin: 0 auto 11px; border-radius: 15px;
              display: grid; place-items: center; font-size: 19px; font-weight: 700;
              background: linear-gradient(145deg, #52416d, #383454);
            }
            .tile:nth-child(2) .icon { background: linear-gradient(145deg, #c64c58, #812c57); }
            .tile:nth-child(3) .icon { background: linear-gradient(145deg, #5585d6, #4351a5); }
            .tile:nth-child(4) .icon { background: linear-gradient(145deg, #e8a34d, #b95254); }
            .tile:nth-child(5) .icon { background: linear-gradient(145deg, #48a98e, #34749d); }
            .tile:nth-child(6) .icon { background: linear-gradient(145deg, #7999be, #465774); }
            .label { color: #ded9e9; font-size: 12px; }
            footer { margin-top: 48px; color: #8f899f; font-size: 12px; }
            @media (max-width: 650px) {
              .top { margin-bottom: 48px; }
              .tiles { grid-template-columns: repeat(3, 1fr); }
              body { padding: 32px 18px; }
            }
          </style>
        </head>
        <body>
          <main>
            <div class="top">
              <div class="logo">Q</div>
              <div><div class="brand">QuickBrowse</div><div class="tagline">Твой интернет — в одном месте</div></div>
            </div>
            <div class="hello" id="greeting">Добро пожаловать</div>
            <h1>Куда отправимся?</h1>
            <p class="subtitle">Найди нужное или открой любимый сайт</p>
            <form class="search" action="https://www.google.com/search" method="get">
              <span>⌕</span>
              <input name="q" autofocus autocomplete="off" placeholder="Поиск в интернете">
              <button class="go" type="submit">Найти</button>
            </form>
            <div class="section">Быстрый доступ</div>
            <div class="tiles">
              <a class="tile" href="https://www.google.com"><div class="icon">G</div><div class="label">Google</div></a>
              <a class="tile" href="https://www.youtube.com"><div class="icon">▶</div><div class="label">YouTube</div></a>
              <a class="tile" href="https://mail.google.com"><div class="icon">✉</div><div class="label">Почта</div></a>
              <a class="tile" href="https://www.wikipedia.org"><div class="icon">W</div><div class="label">Википедия</div></a>
              <a class="tile" href="https://github.com"><div class="icon">⌘</div><div class="label">GitHub</div></a>
              <a class="tile" href="https://yandex.ru"><div class="icon">Я</div><div class="label">Яндекс</div></a>
            </div>
            <footer id="date"></footer>
          </main>
          <script>
            const now = new Date();
            document.getElementById('greeting').textContent =
              (now.getHours() < 12 ? 'Доброе утро' : now.getHours() < 18 ? 'Добрый день' : 'Добрый вечер') + ' 👋';
            document.getElementById('date').textContent =
              now.toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
          </script>
        </body>
        </html>
        """;

    private readonly TabControl _tabs = new();
    private readonly Panel _sidebar = new();
    private readonly ToolTip _toolTip = new();
    private readonly ToolStrip _toolbar = new();
    private readonly ToolStripLabel _brand = new("QuickBrowse");
    private readonly ToolStripTextBox _address = new();
    private readonly ToolStripButton _backButton = new("←");
    private readonly ToolStripButton _forwardButton = new("→");
    private readonly ToolStripButton _reloadButton = new("⟳");
    private readonly ToolStripButton _homeButton = new("⌂");
    private readonly ToolStripButton _bookmarkButton = new("☆");
    private readonly ToolStripDropDownButton _bookmarksButton = new("Закладки");
    private readonly ToolStripDropDownButton _historyButton = new("История");
    private readonly ToolStripDropDownButton _downloadsButton = new("Загрузки");
    private readonly ToolStripButton _newTabButton = new("+");
    private readonly ToolStripButton _goButton = new("Перейти");
    private readonly ToolStripStatusLabel _status = new("Готово");
    private readonly List<Bookmark> _bookmarks = [];
    private readonly List<HistoryEntry> _history = [];
    private readonly List<string> _downloads = [];
    private readonly HashSet<WebView2> _pendingStartPageNavigations = [];
    private readonly HashSet<WebView2> _startPageBrowsers = [];
    private readonly string _dataFolder;
    private readonly string _downloadsFolder;
    private readonly string _bookmarksFile;
    private readonly string _historyFile;

    public MainForm()
    {
        Text = "QuickBrowse";
        MinimumSize = new Size(900, 560);
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(27, 27, 39);
        Font = new Font("Segoe UI", 9F);
        KeyPreview = true;

        _dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickBrowse");
        _downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");
        _bookmarksFile = Path.Combine(_dataFolder, "bookmarks.json");
        _historyFile = Path.Combine(_dataFolder, "history.json");

        LoadData();
        BuildToolbar();
        BuildTabs();
        BuildSidebar();

        var statusStrip = new StatusStrip();
        statusStrip.BackColor = Color.FromArgb(246, 244, 250);
        statusStrip.ForeColor = Color.FromArgb(103, 96, 119);
        statusStrip.Font = new Font("Segoe UI", 9F);
        statusStrip.SizingGrip = false;
        statusStrip.Padding = new Padding(12, 3, 12, 3);
        statusStrip.Items.Add(_status);
        statusStrip.Dock = DockStyle.Bottom;

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        content.Controls.Add(_tabs);
        content.Controls.Add(statusStrip);
        content.Controls.Add(_toolbar);
        Controls.Add(content);
        Controls.Add(_sidebar);

        _tabs.SelectedIndexChanged += (_, _) => UpdateToolbar();
        _tabs.DrawItem += DrawTab;
        _tabs.MouseDown += CloseTabOnMiddleClick;
        _toolbar.Resize += (_, _) => ResizeAddressBox();
        Resize += (_, _) => ResizeAddressBox();
        _address.KeyDown += AddressKeyDown;
        _goButton.Click += (_, _) => NavigateAddress();
        _backButton.Click += (_, _) => GetCurrentBrowser()?.GoBack();
        _forwardButton.Click += (_, _) => GetCurrentBrowser()?.GoForward();
        _reloadButton.Click += (_, _) => GetCurrentBrowser()?.Reload();
        _homeButton.Click += (_, _) => NavigateCurrent(HomeUrl);
        _newTabButton.Click += (_, _) => CreateTab(HomeUrl);
        _bookmarkButton.Click += (_, _) => ToggleBookmark();
        _bookmarksButton.DropDownOpening += (_, _) => PopulateBookmarksMenu();
        _historyButton.DropDownOpening += (_, _) => PopulateHistoryMenu();
        _downloadsButton.DropDownOpening += (_, _) => PopulateDownloadsMenu();
        KeyDown += FormKeyDown;
        FormClosing += (_, _) => SaveData();

        CreateTab(HomeUrl);
    }

    private void BuildToolbar()
    {
        _toolbar.GripStyle = ToolStripGripStyle.Hidden;
        _toolbar.Padding = new Padding(12, 7, 12, 7);
        _toolbar.BackColor = ChromeColor;
        _toolbar.ForeColor = Color.White;
        _toolbar.Dock = DockStyle.Top;
        _toolbar.Renderer = new QuickBrowseRenderer();
        _brand.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _brand.ForeColor = Color.White;
        _brand.Margin = new Padding(2, 1, 14, 1);
        _toolbar.Items.AddRange([
            _brand, _backButton, _forwardButton, _reloadButton, _homeButton,
            new ToolStripSeparator(), _address, _goButton, _newTabButton,
            _bookmarkButton, _bookmarksButton, _historyButton, _downloadsButton
        ]);

        foreach (var button in new[]
                 {
                     _backButton, _forwardButton, _reloadButton, _homeButton,
                     _bookmarkButton, _newTabButton, _goButton
                 })
        {
            button.DisplayStyle = ToolStripItemDisplayStyle.Text;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            button.ForeColor = Color.White;
            button.Padding = new Padding(7, 4, 7, 4);
            button.Margin = new Padding(2, 2, 2, 2);
        }

        _backButton.ToolTipText = "Назад";
        _forwardButton.ToolTipText = "Вперёд";
        _reloadButton.ToolTipText = "Обновить страницу";
        _homeButton.ToolTipText = "Домашняя страница";
        _newTabButton.ToolTipText = "Новая вкладка (Ctrl+T)";
        _bookmarkButton.ToolTipText = "Добавить или удалить закладку";
        _goButton.ToolTipText = "Открыть адрес или найти запрос";
        _goButton.Text = "↵";
        _goButton.Font = new Font("Segoe UI Symbol", 12F);

        _address.AutoSize = false;
        _address.Width = 400;
        _address.Font = new Font("Segoe UI", 10F);
        _address.Margin = new Padding(8, 4, 2, 4);
        _address.ToolTipText = "Введите адрес сайта или поисковый запрос";
        _address.TextBox.BorderStyle = BorderStyle.FixedSingle;
        _address.TextBox.BackColor = Color.FromArgb(43, 42, 57);
        _address.TextBox.ForeColor = Color.FromArgb(245, 243, 250);
        _address.TextBox.Margin = new Padding(0);

        foreach (var menu in new[] { _bookmarksButton, _historyButton, _downloadsButton })
        {
            menu.ForeColor = Color.White;
            menu.Font = new Font("Segoe UI", 9F);
            menu.Padding = new Padding(6, 4, 6, 4);
            menu.Margin = new Padding(2, 2, 2, 2);
        }
    }

    private void BuildSidebar()
    {
        _sidebar.Dock = DockStyle.Left;
        _sidebar.Width = 74;
        _sidebar.BackColor = Color.FromArgb(20, 20, 30);

        var logo = new Label
        {
            Text = "Q",
            Dock = DockStyle.Top,
            Height = 64,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            BackColor = Color.FromArgb(20, 20, 30)
        };

        var navigation = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8, 8, 8, 8),
            BackColor = Color.FromArgb(20, 20, 30)
        };
        navigation.Controls.Add(CreateSidebarButton("⌂", "Главная", (_, _) => NavigateCurrent(HomeUrl)));
        navigation.Controls.Add(CreateSidebarButton("☆", "Закладки", (_, _) => _bookmarksButton.ShowDropDown()));
        navigation.Controls.Add(CreateSidebarButton("◷", "История", (_, _) => _historyButton.ShowDropDown()));
        navigation.Controls.Add(CreateSidebarButton("↓", "Загрузки", (_, _) => _downloadsButton.ShowDropDown()));

        _sidebar.Controls.Add(navigation);
        _sidebar.Controls.Add(logo);
    }

    private Button CreateSidebarButton(string symbol, string tooltip, EventHandler onClick)
    {
        var button = new Button
        {
            Text = symbol,
            Width = 56,
            Height = 54,
            Margin = new Padding(0, 3, 0, 7),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, 20, 30),
            ForeColor = Color.FromArgb(195, 188, 211),
            Font = new Font("Segoe UI Symbol", 19F),
            Cursor = Cursors.Hand,
            AccessibleName = tooltip
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 42, 65);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(67, 51, 91);
        _toolTip.SetToolTip(button, tooltip);
        button.Click += onClick;
        return button;
    }

    private void BuildTabs()
    {
        _tabs.Dock = DockStyle.Fill;
        _tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        _tabs.ItemSize = new Size(190, 34);
        _tabs.SizeMode = TabSizeMode.Fixed;
        _tabs.Padding = new Point(12, 6);
        _tabs.BackColor = Color.FromArgb(27, 27, 39);
    }

    private void ResizeAddressBox()
    {
        var fixedWidth = _toolbar.Items
            .Cast<ToolStripItem>()
            .Where(item => item != _address && item.Visible)
            .Sum(item => item.Width + item.Margin.Horizontal);
        _address.Width = Math.Max(180, _toolbar.ClientSize.Width - fixedWidth - 40);
    }

    private void CreateTab(string url)
    {
        var page = new TabPage(url == HomeUrl ? "Главная" : "Новая вкладка")
        {
            BackColor = Color.White,
            Padding = Padding.Empty
        };
        var browser = new WebView2 { Dock = DockStyle.Fill };
        page.Controls.Add(browser);
        page.Tag = browser;
        _tabs.TabPages.Add(page);
        _tabs.SelectedTab = page;
        InitializeBrowser(page, browser, url);
    }

    private async void InitializeBrowser(TabPage page, WebView2 browser, string url)
    {
        try
        {
            await browser.EnsureCoreWebView2Async();
            if (page.IsDisposed)
                return;

            browser.CoreWebView2.DocumentTitleChanged += (_, _) =>
            {
                if (page.IsDisposed)
                    return;
                page.Text = ShortTitle(browser.CoreWebView2.DocumentTitle);
                _tabs.Invalidate();
                if (_tabs.SelectedTab == page)
                    UpdateToolbar();
            };

            browser.CoreWebView2.NavigationStarting += (_, args) =>
            {
                if (_tabs.SelectedTab == page)
                    _status.Text = "Загрузка…";
                if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out var uri))
                    return;

                if (uri.Scheme == "data" && _pendingStartPageNavigations.Remove(browser))
                {
                    _startPageBrowsers.Add(browser);
                    return;
                }

                _pendingStartPageNavigations.Remove(browser);
                _startPageBrowsers.Remove(browser);
                if (uri.Scheme is not ("http" or "https" or "file" or "about"))
                {
                    args.Cancel = true;
                    OpenExternalUri(args.Uri);
                }
            };

            browser.CoreWebView2.NavigationCompleted += (_, args) =>
            {
                if (!args.IsSuccess)
                {
                    if (_tabs.SelectedTab == page)
                        _status.Text = $"Не удалось загрузить страницу: {args.WebErrorStatus}";
                    return;
                }

                var currentUrl = browser.Source?.ToString();
                if (!string.IsNullOrWhiteSpace(currentUrl) &&
                    Uri.TryCreate(currentUrl, UriKind.Absolute, out var uri) &&
                    uri.Scheme is "http" or "https")
                {
                    AddHistory(new HistoryEntry(
                        browser.CoreWebView2.DocumentTitle,
                        currentUrl,
                        DateTimeOffset.Now));
                }

                if (_tabs.SelectedTab == page)
                {
                    _status.Text = "Готово";
                    UpdateToolbar();
                }
            };

            browser.CoreWebView2.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                CreateTab(args.Uri);
            };

            browser.CoreWebView2.DownloadStarting += (_, args) =>
            {
                try
                {
                    Directory.CreateDirectory(_downloadsFolder);
                    var fileName = Path.GetFileName(args.ResultFilePath);
                    if (string.IsNullOrWhiteSpace(fileName))
                        fileName = "download";
                    args.ResultFilePath = GetUniqueDownloadPath(fileName);
                    _status.Text = $"Скачивание: {fileName}";
                    TrackDownload(args.DownloadOperation, args.ResultFilePath);
                }
                catch (Exception ex)
                {
                    ShowError("Не удалось подготовить папку загрузок.", ex);
                    args.Cancel = true;
                }
            };

            if (url == HomeUrl)
                NavigateToHome(browser);
            else
                browser.Source = new Uri(url);
        }
        catch (Exception ex)
        {
            if (!page.IsDisposed)
                ShowError("Не удалось запустить браузер. Установите Microsoft Edge WebView2 Runtime.", ex);
        }
    }

    private void TrackDownload(CoreWebView2DownloadOperation operation, string filePath)
    {
        operation.StateChanged += (_, _) =>
        {
            if (operation.State == CoreWebView2DownloadState.Completed)
            {
                _downloads.Insert(0, filePath);
                if (_downloads.Count > 20)
                    _downloads.RemoveAt(_downloads.Count - 1);
                _status.Text = $"Загрузка завершена: {Path.GetFileName(filePath)}";
            }
            else if (operation.State == CoreWebView2DownloadState.Interrupted)
            {
                _status.Text = $"Загрузка прервана: {Path.GetFileName(filePath)}";
            }
        };
    }

    private string GetUniqueDownloadPath(string fileName)
    {
        var candidate = Path.Combine(_downloadsFolder, fileName);
        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var index = 1;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(_downloadsFolder, $"{name} ({index++}){extension}");
        }
        return candidate;
    }

    private void NavigateAddress()
    {
        var input = _address.Text.Trim();
        if (input.Length == 0)
            return;

        var destination = Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
                         uri.Scheme is "http" or "https"
            ? uri.ToString()
            : Uri.TryCreate($"https://{input}", UriKind.Absolute, out var hostUri) &&
              input.Contains('.') && !input.Contains(' ')
                ? hostUri.ToString()
                : $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
        NavigateCurrent(destination);
    }

    private void NavigateCurrent(string url)
    {
        var browser = GetCurrentBrowser();
        if (browser is null)
        {
            CreateTab(url);
            return;
        }
        if (url == HomeUrl)
        {
            NavigateToHome(browser);
            return;
        }
        try
        {
            browser.Source = new Uri(url);
        }
        catch (UriFormatException)
        {
            _status.Text = "Некорректный адрес";
        }
    }

    private void AddressKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            NavigateAddress();
        }
    }

    private void FormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.L)
        {
            _address.Focus();
            _address.SelectAll();
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.T)
        {
            CreateTab(HomeUrl);
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.W)
        {
            CloseCurrentTab();
            e.SuppressKeyPress = true;
        }
    }

    private void UpdateToolbar()
    {
        var browser = GetCurrentBrowser();
        if (browser?.CoreWebView2 is not { } core)
        {
            _address.Text = string.Empty;
            _backButton.Enabled = false;
            _forwardButton.Enabled = false;
            _bookmarkButton.Text = "☆";
            return;
        }

        var currentUrl = browser.Source?.ToString();
        _address.Text = _startPageBrowsers.Contains(browser) || currentUrl is "about:blank"
            ? string.Empty
            : currentUrl ?? string.Empty;
        _backButton.Enabled = core.CanGoBack;
        _forwardButton.Enabled = core.CanGoForward;
        _bookmarkButton.Text = currentUrl is not null && _bookmarks.Any(b => b.Url == currentUrl)
            ? "★"
            : "☆";
    }

    private WebView2? GetCurrentBrowser() => _tabs.SelectedTab?.Tag as WebView2;

    private void ToggleBookmark()
    {
        var browser = GetCurrentBrowser();
        var url = browser?.Source?.ToString();
        if (browser?.CoreWebView2 is null ||
            string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https"))
            return;

        var existing = _bookmarks.FindIndex(bookmark => bookmark.Url == url);
        if (existing >= 0)
        {
            _bookmarks.RemoveAt(existing);
            _status.Text = "Закладка удалена";
        }
        else
        {
            _bookmarks.Insert(0, new Bookmark(browser.CoreWebView2.DocumentTitle, url));
            _status.Text = "Страница добавлена в закладки";
        }
        SaveData();
        UpdateToolbar();
    }

    private void PopulateBookmarksMenu()
    {
        _bookmarksButton.DropDownItems.Clear();
        if (_bookmarks.Count == 0)
        {
            _bookmarksButton.DropDownItems.Add(new ToolStripMenuItem("Закладок пока нет") { Enabled = false });
            return;
        }
        foreach (var bookmark in _bookmarks.ToArray())
            _bookmarksButton.DropDownItems.Add(CreateMenuItem(bookmark.Title, bookmark.Url));
    }

    private void PopulateHistoryMenu()
    {
        _historyButton.DropDownItems.Clear();
        if (_history.Count == 0)
            _historyButton.DropDownItems.Add(new ToolStripMenuItem("История пока пуста") { Enabled = false });
        else
            foreach (var item in _history.Take(30))
                _historyButton.DropDownItems.Add(CreateMenuItem(item.Title, item.Url));

        if (_history.Count > 0)
        {
            _historyButton.DropDownItems.Add(new ToolStripSeparator());
            _historyButton.DropDownItems.Add(new ToolStripMenuItem("Очистить историю", null, (_, _) =>
            {
                _history.Clear();
                SaveData();
                _status.Text = "История очищена";
            }));
        }
    }

    private void PopulateDownloadsMenu()
    {
        _downloadsButton.DropDownItems.Clear();
        if (_downloads.Count == 0)
            _downloadsButton.DropDownItems.Add(new ToolStripMenuItem("Загрузок пока нет") { Enabled = false });
        else
            foreach (var file in _downloads.ToArray())
                _downloadsButton.DropDownItems.Add(new ToolStripMenuItem(Path.GetFileName(file), null, (_, _) =>
                {
                    if (File.Exists(file))
                        Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
                }));

        _downloadsButton.DropDownItems.Add(new ToolStripSeparator());
        _downloadsButton.DropDownItems.Add(new ToolStripMenuItem("Открыть папку загрузок", null, (_, _) =>
        {
            Directory.CreateDirectory(_downloadsFolder);
            Process.Start(new ProcessStartInfo(_downloadsFolder) { UseShellExecute = true });
        }));
    }

    private static ToolStripMenuItem CreateMenuItem(string title, string url) =>
        new(string.IsNullOrWhiteSpace(title) ? url : title, null, (_, _) =>
        {
            if (Application.OpenForms.OfType<MainForm>().FirstOrDefault() is { } form)
                form.NavigateCurrent(url);
        });

    private void AddHistory(HistoryEntry entry)
    {
        if (_history.Count > 0 && _history[0].Url == entry.Url)
            return;
        _history.Insert(0, entry);
        if (_history.Count > MaxHistoryItems)
            _history.RemoveAt(_history.Count - 1);
        SaveData();
    }

    private void LoadData()
    {
        try
        {
            Directory.CreateDirectory(_dataFolder);
            if (File.Exists(_bookmarksFile))
                _bookmarks.AddRange(JsonSerializer.Deserialize<List<Bookmark>>(File.ReadAllText(_bookmarksFile)) ?? []);
            if (File.Exists(_historyFile))
                _history.AddRange(JsonSerializer.Deserialize<List<HistoryEntry>>(File.ReadAllText(_historyFile)) ?? []);
        }
        catch (Exception ex)
        {
            ShowError("Не удалось прочитать закладки или историю.", ex);
        }
    }

    private void SaveData()
    {
        try
        {
            Directory.CreateDirectory(_dataFolder);
            File.WriteAllText(_bookmarksFile, JsonSerializer.Serialize(_bookmarks));
            File.WriteAllText(_historyFile, JsonSerializer.Serialize(_history));
        }
        catch (Exception ex)
        {
            ShowError("Не удалось сохранить закладки или историю.", ex);
        }
    }

    private void DrawTab(object? sender, DrawItemEventArgs e)
    {
        var page = _tabs.TabPages[e.Index];
        var selected = (e.State & DrawItemState.Selected) != 0;
        var bounds = Rectangle.Inflate(e.Bounds, -3, -3);
        using var path = RoundedRectangle(bounds, 8);
        using var background = new SolidBrush(selected ? Color.FromArgb(43, 42, 57) : Color.FromArgb(31, 30, 43));
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.FillPath(background, path);
        if (selected)
        {
            using var accent = new Pen(AccentColor, 2F);
            e.Graphics.DrawLine(accent, bounds.Left + 12, bounds.Bottom - 1, bounds.Right - 12, bounds.Bottom - 1);
        }

        var textBounds = new Rectangle(bounds.X + 12, bounds.Y + 2, bounds.Width - 42, bounds.Height - 4);
        using var tabFont = new Font("Segoe UI", 9F, selected ? FontStyle.Bold : FontStyle.Regular);
        using var closeFont = new Font("Segoe UI", 12F);
        TextRenderer.DrawText(e.Graphics, page.Text, tabFont, textBounds,
            selected ? Color.White : Color.FromArgb(176, 170, 191),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var closeBounds = GetCloseBounds(e.Bounds);
        TextRenderer.DrawText(e.Graphics, "×", closeFont, closeBounds, Color.FromArgb(121, 134, 153),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void CloseTabOnMiddleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Middle)
        {
            for (var i = 0; i < _tabs.TabPages.Count; i++)
            {
                if (_tabs.GetTabRect(i).Contains(e.Location))
                {
                    CloseTab(_tabs.TabPages[i]);
                    return;
                }
            }
        }
        else if (e.Button == MouseButtons.Left)
        {
            for (var i = 0; i < _tabs.TabPages.Count; i++)
            {
                var bounds = _tabs.GetTabRect(i);
                if (GetCloseBounds(bounds).Contains(e.Location))
                {
                    CloseTab(_tabs.TabPages[i]);
                    return;
                }
            }
        }
    }

    private static Rectangle GetCloseBounds(Rectangle tabBounds) =>
        new(tabBounds.Right - 25, tabBounds.Top + 5, 20, tabBounds.Height - 10);

    private void CloseCurrentTab()
    {
        if (_tabs.SelectedTab is { } page)
            CloseTab(page);
    }

    private void CloseTab(TabPage page)
    {
        if (_tabs.TabPages.Count == 1)
        {
            var browser = page.Tag as WebView2;
            if (browser is not null)
                NavigateToHome(browser);
            page.Text = "Новая вкладка";
            return;
        }

        _tabs.TabPages.Remove(page);
        if (page.Tag is WebView2 browserToDispose)
            browserToDispose.Dispose();
        page.Dispose();
        UpdateToolbar();
    }

    private static string ShortTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Новая вкладка";
        return title.Length <= 24 ? title : title[..21] + "…";
    }

    private void OpenExternalUri(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowError("Не удалось открыть ссылку во внешнем приложении.", ex);
        }
    }

    private void ShowError(string message, Exception exception)
    {
        MessageBox.Show(this, $"{message}\n\n{exception.Message}", "QuickBrowse",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void NavigateToHome(WebView2 browser)
    {
        if (browser.CoreWebView2 is not null)
        {
            _pendingStartPageNavigations.Add(browser);
            browser.CoreWebView2.NavigateToString(StartPageHtml);
        }
    }

    private sealed record Bookmark(string Title, string Url);
    private sealed record HistoryEntry(string Title, string Url, DateTimeOffset VisitedAt);

    private sealed class QuickBrowseRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                e.AffectedBounds,
                ChromeColor,
                Color.FromArgb(30, 48, 77),
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(42, 59, 85));
            e.Graphics.DrawLine(pen, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
        }

        protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is ToolStripSeparator)
            {
                var x = e.Item.Bounds.Left + e.Item.Bounds.Width / 2;
                using var pen = new Pen(Color.FromArgb(71, 88, 113));
                e.Graphics.DrawLine(pen, x, 9, x, e.Item.Bounds.Height - 9);
                return;
            }

            if (e.Item is ToolStripTextBox)
                return;

            if (e.Item.Selected || e.Item.Pressed)
            {
                var bounds = Rectangle.Inflate(e.Item.Bounds, -1, -2);
                using var path = RoundedRectangle(bounds, 7);
                using var brush = new SolidBrush(e.Item.Pressed
                    ? Color.FromArgb(59, 91, 135)
                    : Color.FromArgb(48, 67, 96));
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = MutedColor;
            base.OnRenderArrow(e);
        }
    }
}
