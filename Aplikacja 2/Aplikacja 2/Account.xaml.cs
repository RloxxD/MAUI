namespace Aplikacja_2;

public partial class Account : ContentPage
{
	public Account()
	{
		InitializeComponent();
        if(DBConnect.UserID != 0)
        {
            Loginw.IsEnabled = false;
            Password.IsEnabled = false;
            LoginBut.IsEnabled = false;
            LogoutBut.IsEnabled = true;
        }
	}

    private async void Close(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void Register(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Register());
    }

    private void Login(object sender, EventArgs e)
    {
        DBConnect.Login(Loginw.Text, Password.Text, this);
    }

    private async void Logout(object sender, EventArgs e)
    {
        DBConnect.UserID = 0;
        DBConnect.Save_User();
        await Navigation.PopModalAsync();
    }
}