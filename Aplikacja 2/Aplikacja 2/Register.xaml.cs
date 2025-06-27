namespace Aplikacja_2;

public partial class Register : ContentPage
{
	public Register()
	{
		InitializeComponent();
	}

    private async void Done(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Login.Text))
        {
            await DisplayAlert("Error", "Login can't be empty", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password.Text))
        {
            await DisplayAlert("Error", "Password has to be at least 8 characters long", "OK");
            return;
        }

        if (Password.Text.Length < 8) 
        {
            await DisplayAlert("Error", "Password has to be at least 8 characters long", "OK");
            return;
        }
        if (Password.Text != Password_Repeat.Text) 
        {
            await DisplayAlert("Error", "Passwords don't match", "OK");
            return;
        }
        DBConnect.RegisterAccount(Login.Text, Password.Text, this);

    }

    private async void Close(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}