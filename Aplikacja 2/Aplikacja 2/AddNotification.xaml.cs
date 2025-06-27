using Plugin.LocalNotification;
namespace Aplikacja_2;

public partial class AddNotification : ContentPage
{
	public AddNotification()
	{
		InitializeComponent();
        PickedDate.MinimumDate = DateTime.Today;
        Time_Before.SelectedIndex = 0;
        frequency_Pick.SelectedIndex = 0;
	}

    private void EnableFrequency(object sender, EventArgs e)
    {
        if (frequency_Pick.IsEnabled == false)
        {
            frequency_Pick.IsEnabled = true;
        }
        else if (frequency_Pick.IsEnabled == true)
        {
            frequency_Pick.IsEnabled = false;
        }
    }

    private async void NotificationAdd(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Description.Text))
        {
            await DisplayAlert("Error", "Description can't be empty", "OK");
            return;
        }

        DateTime wholeDate = new DateTime(DateOnly.FromDateTime(PickedDate.Date).Year, DateOnly.FromDateTime(PickedDate.Date).Month, DateOnly.FromDateTime(PickedDate.Date).Day, TimeOnly.FromTimeSpan(PickedTime.Time).Hour, TimeOnly.FromTimeSpan(PickedTime.Time).Minute, TimeOnly.FromTimeSpan(PickedTime.Time).Second);
        if (DateTime.Now.Subtract(wholeDate) >= TimeSpan.Zero || wholeDate - TimeSpan.Parse(Time_Before.SelectedItem.ToString()) <= DateTime.Now)
        {
            await DisplayAlert("Error", "Can't set past notification", "OK");
            return;
        }
        if (repeat_check.IsChecked == false)
        {
            int id = 1;
            if (Lists.Notifications.Count > 0)
            {
                id = Lists.Notifications[Lists.Notifications.Count - 1].Request.NotificationId + 1;
            }
            var request = new NotificationRequest
            {
                NotificationId = id,
                Title = Description.Text,
                Subtitle = "Notification",
                Description = wholeDate.ToString(),
                BadgeNumber = 42,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = wholeDate - TimeSpan.Parse(Time_Before.SelectedItem.ToString())
                }

            };

            Notification notification = new Notification(Description.Text, DateOnly.FromDateTime(PickedDate.Date), TimeOnly.FromTimeSpan(PickedTime.Time), Time_Before.SelectedItem.ToString(), repeat_check.IsChecked, request);
            Lists.Notifications.Add(notification);
            LocalNotificationCenter.Current.Show(request);

            if (DBConnect.UserID > 0)
            {
                DBConnect.InsertNotification(this, notification);
            }

        }
        else
        {
            TimeSpan repeat_frequency = new TimeSpan();
            DateTime date1;
            DateTime date2;
            switch (frequency_Pick.SelectedItem.ToString())
            {
                case "Everyday":
                    repeat_frequency = TimeSpan.FromDays(1);
                    break;
                case "Weekly":
                    repeat_frequency = TimeSpan.FromDays(7);
                    break;
                case "Monthly":
                    date1 = PickedDate.Date;
                    date2 = PickedDate.Date.AddMonths(1);
                    repeat_frequency = TimeSpan.FromDays((date2 - date1).TotalDays);
                    break;
                case "Yearly":
                    date1 = PickedDate.Date;
                    date2 = PickedDate.Date.AddYears(1);
                    repeat_frequency = TimeSpan.FromDays((date2 - date1).TotalDays);
                    break;
            }
            int id = 1;
            if(Lists.Notifications.Count > 0)
            {
                id = Lists.Notifications[Lists.Notifications.Count - 1].Request.NotificationId + 1;
            }
            var request = new NotificationRequest
            {

                NotificationId = id,
                Title = Description.Text,
                Subtitle = "Notification",
                Description = wholeDate.ToString(),
                BadgeNumber = 42,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = wholeDate - TimeSpan.Parse(Time_Before.SelectedItem.ToString()),
                    NotifyRepeatInterval = repeat_frequency
                }

            };
            Notification notification = new Notification(Description.Text, DateOnly.FromDateTime(PickedDate.Date), TimeOnly.FromTimeSpan(PickedTime.Time), Time_Before.SelectedItem.ToString(), repeat_check.IsChecked, frequency_Pick.SelectedItem.ToString(), request);
            Lists.Notifications.Add(notification);
            LocalNotificationCenter.Current.Show(request);
            if (DBConnect.UserID > 0)
            {
                DBConnect.InsertNotification(this, notification);
            }
        }
       
        Lists.SaveNotifications();
        await Navigation.PopModalAsync();
    }

    private async void Close(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}