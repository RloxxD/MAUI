using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aplikacja_2
{
    public static class Lists
    {
        private static ObservableCollection<Notification> notifications = new ObservableCollection<Notification>();

        private static ObservableCollection<Task> daily_tasks = new ObservableCollection<Task>();

        private static ObservableCollection<Task> tasks = new ObservableCollection<Task>();

        private static List<Step> steps = new List<Step>();

        internal static ObservableCollection<Notification> Notifications { get => notifications; set => notifications = value; }
        internal static ObservableCollection<Task> Tasks { get => tasks; set => tasks = value; }
        internal static List<Step> Steps { get => steps; set => steps = value; }
        internal static ObservableCollection<Task> Daily_tasks { get => daily_tasks; set => daily_tasks = value; }

        public static void SaveDailyTasks()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Task>));
            TextWriter writer = new StreamWriter(FileSystem.Current.AppDataDirectory + "/DailyTasks.xml");
            serializer.Serialize(writer, Daily_tasks);
            writer.Close();
           
        }

        public static void LoadDailyTasks()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Task>));
            FileStream fs = new FileStream(FileSystem.Current.AppDataDirectory + "/DailyTasks.xml", FileMode.Open);
            Daily_tasks = (ObservableCollection<Task>) serializer.Deserialize(fs);
            fs.Close();
        }

        public static void SaveTasks()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Task>));
            TextWriter writer = new StreamWriter(FileSystem.Current.AppDataDirectory + "/Tasks.xml");
            serializer.Serialize(writer, Tasks);
            writer.Close();

        }

        public static void LoadTasks()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Task>));
            FileStream fs = new FileStream(FileSystem.Current.AppDataDirectory + "/Tasks.xml", FileMode.Open);
            Tasks = (ObservableCollection<Task>)serializer.Deserialize(fs);
            fs.Close();
        }

        public static void SaveNotifications()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Notification>));
            TextWriter writer = new StreamWriter(FileSystem.Current.AppDataDirectory + "/Notifications.xml");
            serializer.Serialize(writer, Notifications);
            writer.Close();
        }

        public static void LoadNotifications()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Notification>));
            FileStream fs = new FileStream(FileSystem.Current.AppDataDirectory + "/Notifications.xml", FileMode.Open);
            Notifications = (ObservableCollection<Notification>)serializer.Deserialize(fs);

            foreach (Notification n in Notifications)
            {
                n.Date = DateOnly.FromDateTime(n.WholeDate).ToString();
                n.Time = TimeOnly.FromDateTime(n.WholeDate).ToString();
            }
            fs.Close();
        }
        public static void SaveSteps()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Step>));
            TextWriter writer = new StreamWriter(FileSystem.Current.AppDataDirectory + "/Steps.xml");
            serializer.Serialize(writer, Steps);
            writer.Close();
        }

        public static void LoadSteps()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Step>));
            FileStream fs = new FileStream(FileSystem.Current.AppDataDirectory + "/Steps.xml", FileMode.Open);
            Steps = (List<Step>)serializer.Deserialize(fs);
            fs.Close();

        }
    }
}
