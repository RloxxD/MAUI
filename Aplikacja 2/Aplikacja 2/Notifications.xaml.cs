using Plugin.LocalNotification;
using System.Diagnostics;
using System.Drawing;

namespace Aplikacja_2;

public partial class Notifications : ContentPage
{

    public Notifications()
	{
        if(Lists.Notifications.Count == 0 && File.Exists(FileSystem.Current.AppDataDirectory + "/Notifications.xml") && DBConnect.UserID == 0)
        {
            Lists.LoadNotifications();
        }
        Device.StartTimer(TimeSpan.FromMinutes(1), () =>
        {
           var notificationQuery1 =
           from notification in Lists.Notifications
           where notification.WholeDate.Subtract(DateTime.Now) <= TimeSpan.Zero && notification.Repeat == false
           select notification;

            foreach (Notification n in notificationQuery1.ToList())
            {
                if (DBConnect.UserID != 0)
                {
                    DBConnect.DeleteNotification(this, n);
                }
                Lists.Notifications.Remove(n);
            }

            var notificationQuery2 =
            from notification in Lists.Notifications
            where notification.Repeat == true && notification.WholeDate.Subtract(DateTime.Now) < TimeSpan.Zero
            select notification;

            foreach(Notification n in notificationQuery2.ToList())
            {
                switch (n.Frequency)
                {
                    case "Everyday":
                        n.Date = DateOnly.Parse(n.Date).AddDays(1).ToString();
                        n.WholeDate = n.WholeDate.AddDays(1);
                        n.Time_of_notification = n.Time_of_notification.AddDays(1);
                        n.Time_mess = "Notify at: " + n.Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        n.Day_time = n.Date + " " + n.Time;
                        n.Request.Description = n.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, n);
                        }
                        Lists.Notifications.Remove(n);
                        Lists.Notifications.Add(n);
                        break;
                    case "Weekly":
                        n.Date = DateOnly.Parse(n.Date).AddDays(7).ToString();
                        n.WholeDate = n.WholeDate.AddDays(7);
                        n.Time_of_notification = n.Time_of_notification.AddDays(7);
                        n.Time_mess = "Notify at: " + n.Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        n.Day_time = n.Date + " " + n.Time;
                        n.Request.Description = n.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, n);
                        }
                        Lists.Notifications.Remove(n);
                        Lists.Notifications.Add(n);
                        break;
                    case "Monthly":
                        n.Date = DateOnly.Parse(n.Date).AddMonths(1).ToString();
                        n.WholeDate = n.WholeDate.AddMonths(1);
                        n.Time_of_notification = n.Time_of_notification.AddMonths(1);
                        n.Time_mess = "Notify at: " + n.Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        n.Day_time = n.Date + " " + n.Time;
                        n.Request.Description = n.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, n);
                        }
                        Lists.Notifications.Remove(n);
                        Lists.Notifications.Add(n);
                        break;
                    case "Yearly":
                        n.Date = DateOnly.Parse(n.Date).AddYears(1).ToString();
                        n.WholeDate = n.WholeDate.AddYears(1);
                        n.Time_of_notification = n.Time_of_notification.AddYears(1);
                        n.Time_mess = "Notify at: " + n.Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        n.Day_time = n.Date + " " + n.Time;
                        n.Request.Description = n.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, n);
                        }
                        Lists.Notifications.Remove(n);
                        Lists.Notifications.Add(n);
                        break;
                }
            }

            listNotifications.ItemsSource = Lists.Notifications;
            Lists.SaveNotifications();
            return true;
        });
        InitializeComponent();
       
        listNotifications.ItemsSource = Lists.Notifications;
    }

	private async void Delete(object sender, EventArgs e)
	{
		delete_Button.IsEnabled = true;
	}

	private async void DeleteNotification(object sender, EventArgs e)
	{
		int index = Lists.Notifications.IndexOf(listNotifications.SelectedItem as Notification);
		Notification notification = listNotifications.SelectedItem as Notification;
        LocalNotificationCenter.Current.Cancel(notification.Request.NotificationId);
        if (DBConnect.UserID != 0)
        {
            DBConnect.DeleteNotification(this, notification);
        }
        Lists.Notifications.RemoveAt(index);
		delete_Button.IsEnabled=false;
        Lists.SaveNotifications();
	}
	private async void Modal_Add(object sender, EventArgs e)
	{
		await Navigation.PushModalAsync(new AddNotification());
    }
    protected override void OnAppearing()
    {
        for (int i = Lists.Notifications.Count - 1; i >= 0; i--)
        {
            if (Lists.Notifications[i].Repeat == true && Lists.Notifications[i].WholeDate.Subtract(DateTime.Now) < TimeSpan.Zero)
            {
                switch (Lists.Notifications[i].Frequency)
                {
                    case "Everyday":
                        Notification notification = Lists.Notifications[i];
                        notification.Date = DateOnly.Parse(notification.Date).AddDays(1).ToString();
                        notification.WholeDate = notification.WholeDate.AddDays(1);
                        notification.Time_of_notification = notification.Time_of_notification.AddDays(1);
                        notification.Time_mess = "Notify at: " + Lists.Notifications[i].Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        notification.Day_time = Lists.Notifications[i].Date + " " + Lists.Notifications[i].Time;
                        notification.Request.Description = notification.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, notification);
                        }
                        Lists.Notifications.RemoveAt(i);
                        Lists.Notifications.Add(notification);
                        break;
                    case "Weekly":
                        Notification notification2 = Lists.Notifications[i];
                        notification2.Date = DateOnly.Parse(notification2.Date).AddDays(7).ToString();
                        notification2.WholeDate = notification2.WholeDate.AddDays(7);
                        notification2.Time_of_notification = notification2.Time_of_notification.AddDays(7);
                        notification2.Time_mess = "Notify at: " + Lists.Notifications[i].Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        notification2.Day_time = Lists.Notifications[i].Date + " " + Lists.Notifications[i].Time;
                        notification2.Request.Description = notification2.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, notification2);
                        }
                        Lists.Notifications.RemoveAt(i);
                        Lists.Notifications.Add(notification2);
                        break;
                    case "Monthly":
                        Notification notification3 = Lists.Notifications[i];
                        notification3.Date = DateOnly.Parse(notification3.Date).AddMonths(1).ToString();
                        notification3.WholeDate = notification3.WholeDate.AddMonths(1);
                        notification3.Time_of_notification = notification3.Time_of_notification.AddMonths(1);
                        notification3.Time_mess = "Notify at: " + Lists.Notifications[i].Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        notification3.Day_time = Lists.Notifications[i].Date + " " + Lists.Notifications[i].Time;
                        notification3.Request.Description = notification3.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, notification3);
                        }
                        Lists.Notifications.RemoveAt(i);
                        Lists.Notifications.Add(notification3);
                        break;
                    case "Yearly":
                        Notification notification4 = Lists.Notifications[i];
                        notification4.Date = DateOnly.Parse(notification4.Date).AddYears(1).ToString();
                        notification4.WholeDate = notification4.WholeDate.AddYears(1);
                        notification4.Time_of_notification = notification4.Time_of_notification.AddYears(1);
                        notification4.Time_mess = "Notify at: " + Lists.Notifications[i].Time_of_notification.ToString("yyyy-MM-dd HH:mm");
                        notification4.Day_time = Lists.Notifications[i].Date + " " + Lists.Notifications[i].Time;
                        notification4.Request.Description = notification4.WholeDate.ToString();
                        if (DBConnect.UserID != 0)
                        {
                            DBConnect.UpdateNotification(this, notification4);
                        }
                        Lists.Notifications.RemoveAt(i);
                        Lists.Notifications.Add(notification4);
                        break;
                }
            }
            if(Lists.Notifications[i].WholeDate.Subtract(DateTime.Now) <= TimeSpan.Zero && Lists.Notifications[i].Repeat == false)
            {
                if (DBConnect.UserID != 0)
                {
                    DBConnect.DeleteNotification(this, Lists.Notifications[i]);
                }
                Lists.Notifications.RemoveAt(i);
               
            }
        }
        DBConnect.ReloadNotifications(this);
        listNotifications.ItemsSource = Lists.Notifications;
        Lists.SaveNotifications();
        
        base.OnAppearing();
    }

}