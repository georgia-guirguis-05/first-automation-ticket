using Microsoft.Playwright;

namespace PlaywrightDemo.Pages;

public class LoginPage
{
    private IPage _page;
    private readonly ILocator _lnkLogin;
    private readonly ILocator _txtPassword;
    private readonly ILocator _txtUsername;
    private readonly ILocator _btnLogin;
    private readonly ILocator _lnkEmployeeDetails;
    
    
    public LoginPage(IPage page)
    {
        _page = page;
        _lnkLogin = _page.Locator("text=Login");
        _txtPassword = _page.Locator("text=#Password");
        _txtUsername = _page.Locator("text=#Username");
        _btnLogin = _page.Locator("text=Log in");
        _lnkEmployeeDetails = _page.Locator("text='Employee Details'");
    }
    
    public async Task ClickLogin() => await _lnkLogin.CheckAsync();

    public async Task Login(string username, string password)
    {
        await _txtUsername.FillAsync(username);
        await _txtPassword.FillAsync(password);
        await _btnLogin.ClickAsync();
    }
    public async Task<bool> IsEmployeeDetailsExists() => await _lnkEmployeeDetails.IsVisibleAsync();
    
}

