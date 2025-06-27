namespace Aplikacja_2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OpenAccount(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new Account());
        }
    }
}
