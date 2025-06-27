using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.LocalNotification;

namespace Aplikacja_2
{
    [Serializable]
    public class Notification
    {
        private string description;
        private string date;
        private string time;
        private DateTime time_of_notification;
        private DateTime wholeDate;
        private bool repeat;
        private string frequency;
        private string repeat_mess;
        private string day_time;
        private string time_mess;
        private NotificationRequest request;
        private int databaseID;

        public Notification(string description, DateOnly date, TimeOnly time, string time_to_notify , bool repeat, string frequency, NotificationRequest request)
        {
            this.Description = description;
            this.Date = date.ToString();
            this.Time = time.ToString();
            this.WholeDate = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
            this.Time_of_notification = WholeDate - TimeSpan.Parse(time_to_notify);
            this.time_mess = "Notify at: " + Time_of_notification.ToString("yyyy-MM-dd HH:mm");
            this.Repeat = repeat;
            this.Frequency = frequency;
            this.Repeat_mess = "Repeat Frequency: " + frequency;
            this.Day_time = date + " " + time;
            this.Request = request;

        }
        public Notification()
        {

        }
        public Notification(string description, DateOnly date, TimeOnly time, string time_to_notify, bool repeat, NotificationRequest request)
        {
            this.Description = description;
            this.Date = date.ToString();
            this.Time = time.ToString();
            this.WholeDate = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
            this.Time_of_notification = WholeDate - TimeSpan.Parse(time_to_notify);
            this.time_mess = "Notify at: " + Time_of_notification.ToString("yyyy-MM-dd HH:mm");
            this.Repeat = repeat;
            this.Repeat_mess = "Don't Repeat";
            this.Day_time = date + " " + time;
            this.Request = request;

        }

        public string Description { get => description; set => description = value; }
        public string Date { get => date; set => date = value; }
        public string Time { get => time; set => time = value; }
        public bool Repeat { get => repeat; set => repeat = value; }
        public string Frequency { get => frequency; set => frequency = value; }
        public string Repeat_mess { get => repeat_mess; set => repeat_mess = value; }
        public string Day_time { get => day_time; set => day_time = value; }
        public DateTime Time_of_notification { get => time_of_notification; set => time_of_notification = value; }
        public string Time_mess { get => time_mess; set => time_mess = value; }
        public DateTime WholeDate { get => wholeDate; set => wholeDate = value; }
        public NotificationRequest Request { get => request; set => request = value; }
        public int DatabaseID { get => databaseID; set => databaseID = value; }
    }
}
