using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Threading;
using System.Collections.Generic;
using SystemLogin.Domain;
using SystemLogin.Hardware.Mocks;
using SystemLogin.Services;


namespace SystemLogin;

public partial class MainWindow : Window
{
    private AccountService _accountService;
    private AppDbContext _appDbContext;
     private ComboBox? _colorComboBox;
    private Button? _startSortingButton;
    private Button? _stopSortingButton;
    private Button? _simulateArrivalButton;
    private TextBlock? _robotStatusText;

    private MockCamera? _mockCamera;
    private MockSensor? _mockSensor;
    private MockRobotArm? _mockRobot;
    private SortingJobRunner? _runner;

    private CancellationTokenSource? _runnerCts;
    private Task? _runnerTask;

    public MainWindow()
    {
        InitializeComponent();
        _colorComboBox = this.FindControl<ComboBox>("ColorComboBox");
        _startSortingButton = this.FindControl<Button>("StartSortingButton");
        _stopSortingButton = this.FindControl<Button>("StopSortingButton");
        _simulateArrivalButton = this.FindControl<Button>("SimulateArrivalButton");
        _robotStatusText = this.FindControl<TextBlock>("RobotStatusText");

        if (_startSortingButton != null) _startSortingButton.Click += (_, __) => StartSorting();
        if (_stopSortingButton != null) _stopSortingButton.Click += (_, __) => StopSorting();
        if (_simulateArrivalButton != null) _simulateArrivalButton.Click += (_, __) => _mockSensor?.SimulateBlockArrived();

        if (_colorComboBox != null)
            {
                _colorComboBox.SelectionChanged += (_, __) =>
                {
                    if (_mockCamera == null) return;
                    _mockCamera.SelectedColor = GetSelectedColorFromUi();
                    UiLog($"[UI] Selected color: {_mockCamera.SelectedColor}");
                };
            }
        DataContext = this;

        foreach (TabItem item in TabControl.Items)
            item.IsVisible = false;
        LoginTab.IsVisible = true;

        InitializeServices();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (await EnsureDatabaseCreatedWithExampleDataAsync())
            _log("Database did not exist. So I created one.");
    }

    private void _log(string s)
    {
        var now = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        LogOutput.Text += $"{now} | {s}\n";
    }
private void UiLog(string message)
{
    Dispatcher.UIThread.Post(() => _log(message));
}

private BlockColor GetSelectedColorFromUi()
{
    var selected = (_colorComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();

    return selected switch
    {
        "Red" => BlockColor.Red,
        "Green" => BlockColor.Green,
        "Blue" => BlockColor.Blue,
        _ => BlockColor.Unknown
    };
}
private void StartSorting()
{
    if (_runnerTask is { IsCompleted: false })
    {
        UiLog("[UI] Sorting is already running.");
        return;
    }

    UiLog("[UI] Starting sorting...");

    _mockCamera = new MockCamera { SelectedColor = GetSelectedColorFromUi() };
    _mockSensor = new MockSensor();
    _mockRobot = new MockRobotArm(UiLog);

    var bins = new List<CustomerBin>
    {
        new("A", "Customer A", BlockColor.Red),
        new("B", "Customer B", BlockColor.Green),
        new("C", "Customer C", BlockColor.Blue),
    };

    _runner = new SortingJobRunner(_mockSensor, _mockCamera, _mockRobot, bins, UiLog);

    _runnerCts = new CancellationTokenSource();
    _runnerTask = Task.Run(() => _runner.RunAsync(_runnerCts.Token));

    if (_robotStatusText != null) _robotStatusText.Text = "Status: Running";
    if (_startSortingButton != null) _startSortingButton.IsEnabled = false;
    if (_stopSortingButton != null) _stopSortingButton.IsEnabled = true;
    if (_simulateArrivalButton != null) _simulateArrivalButton.IsEnabled = true;
}

private async void StopSorting()
{
    if (_runnerCts == null)
        return;

    UiLog("[UI] Stopping sorting...");

    try
    {
        _runnerCts.Cancel();

        if (_mockRobot != null)
            await _mockRobot.StopAsync(CancellationToken.None);

        if (_runnerTask != null)
            await _runnerTask;
    }
    catch (TaskCanceledException)
    {
        // expected
    }
    catch (Exception ex)
    {
        UiLog("[UI] Stop error: " + ex.Message);
    }
    finally
    {
        _runnerCts.Dispose();
        _runnerCts = null;

        if (_robotStatusText != null) _robotStatusText.Text = "Status: Stopped";
        if (_startSortingButton != null) _startSortingButton.IsEnabled = true;
        if (_stopSortingButton != null) _stopSortingButton.IsEnabled = false;
        if (_simulateArrivalButton != null) _simulateArrivalButton.IsEnabled = false;

        UiLog("[UI] Sorting stopped.");
    }
}


    private void InitializeServices()
    {
        _appDbContext?.Dispose();
        // Because InitializeServices() can not only be called from the constructor but also from another method

        _appDbContext = new AppDbContext();
        _accountService = new AccountService(_appDbContext, new PasswordHasher());
    }

    private async void RecreateDatabaseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await _appDbContext.Database.EnsureDeletedAsync();
        await EnsureDatabaseCreatedWithExampleDataAsync();
    }

    public async Task<bool> EnsureDatabaseCreatedWithExampleDataAsync()
    {
        var databaseIsCreated = await _appDbContext.Database.EnsureCreatedAsync();
        if (!databaseIsCreated) return false;
        InitializeServices();
        await _accountService.NewAccountAsync("admin", "admin", true);
        await _accountService.NewAccountAsync("user", "user");

        return true;
    }

    private async void CreateUserButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var (username, password, isAdmin) =
            (CreateUserUsername.Text, CreateUserPassword.Text, CreateUserIsAdmin.IsChecked);

        if (await _accountService.UsernameExistsAsync(username))
        {
            _log($"Username {username} exists.");
            return;
        }

        await _accountService.NewAccountAsync(username, password, (bool)isAdmin);
        _log($"Created user: {username}.");
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!await _accountService.UsernameExistsAsync(LoginUsername.Text))
        {
            _log("Username does not exist.");
            return;
        }

        if (!await _accountService.CredentialsCorrectAsync(LoginUsername.Text, LoginPassword.Text))
        {
            _log("Password wrong.\n");
            return;
        }

        var account = await _accountService.GetAccountAsync(LoginUsername.Text);
        LogoutButton.IsVisible = true;

        LoginTab.IsVisible = false;
        RobotTab.IsVisible = true;

        if (account.isAdmin)
        {
            UsersTab.IsVisible = true;
            DatabaseTab.IsVisible = true;

            UsersTab.IsSelected = true;
        }
        else
        {
            RobotTab.IsSelected = true;
        }

        _log($"{account.Username} logged in.");
        LoginUsername.Text = "";
        LoginPassword.Text = "";
    }

    private async void LogoutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        foreach (TabItem item in TabControl.Items)
            item.IsVisible = false;
        LoginTab.IsVisible = true;
        LoginTab.IsSelected = true;
        LogoutButton.IsVisible = false;
        _log("Logged out.");
    }

    private void ClearLogButton_OnClick(object? sender, RoutedEventArgs e)
    {
        LogOutput.Text = "";
    }
}