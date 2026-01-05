using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SystemLogin;

public partial class MainWindow : Window
{
    private AccountService _accountService;
    private AppDbContext _appDbContext;

    public MainWindow()
    {
        InitializeComponent();
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