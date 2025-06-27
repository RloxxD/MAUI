using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using Plugin.LocalNotification;
using Microsoft.Maui.Controls.Compatibility;


namespace Aplikacja_2
{
    internal class DBConnect
    {
        private static int userID = 0;

        private static string connectionString = "Data Source=tcp:192.168.0.15,56584;Database=Planer;User Id=Planer;Password=zaq1@WSX;TrustServerCertificate=true";
        public static int UserID { get => userID; set => userID = value; }



        public static async void Load(Page m)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();


                    var sql = "SELECT * FROM DailyTasks WHERE AccountID = @accountID AND Shown LIKE @shown;";

                    await using var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Task t = new Task(reader["Description"].ToString(), bool.Parse(reader["Status"].ToString()), DateOnly.Parse(reader["Date"].ToString()));
                                t.DatabaseID = int.Parse(reader["DailyTaskID"].ToString());
                                Lists.Daily_tasks.Add(t);

                            }
                        }
                    }


                    sql = "SELECT * FROM Tasks WHERE AccountID = @accountID AND Shown LIKE @shown;";

                    await using var command2 = new SqlCommand(sql, connection);
                    command2.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command2.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                    using (SqlDataReader reader = await command2.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Task t = new Task(reader["Description"].ToString(), bool.Parse(reader["Status"].ToString()));
                                t.DatabaseID = int.Parse(reader["TaskID"].ToString());
                                Lists.Tasks.Add(t);

                            }
                        }
                    }

                    sql = "SELECT * FROM Notifications WHERE AccountID = @accountID;";

                    await using var command3 = new SqlCommand(sql, connection);
                    command3.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;

                    using (SqlDataReader reader = await command3.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (!bool.Parse(reader["Repeat"].ToString()))
                                {
                                    var request = new NotificationRequest
                                    {
                                        NotificationId = int.Parse(reader["RequestID"].ToString()),
                                        Title = reader["Description"].ToString(),
                                        Subtitle = "Notification",
                                        Description = reader["WholeDate"].ToString(),
                                        BadgeNumber = 42,
                                        Schedule = new NotificationRequestSchedule
                                        {
                                            NotifyTime = DateTime.Parse(reader["WholeDate"].ToString())
                                        }
                                    };

                                    Notification notification = new Notification(reader["Description"].ToString(), DateOnly.Parse(reader["Date"].ToString())
                                        , TimeOnly.Parse(reader["Time"].ToString()), "00:00", bool.Parse(reader["Repeat"].ToString()), request);
                                    notification.DatabaseID = int.Parse(reader["NotificationID"].ToString());
                                    LocalNotificationCenter.Current.Show(notification.Request);
                                    Lists.Notifications.Add(notification);
                                }
                                else
                                {
                                    TimeSpan repeat_frequency = TimeSpan.Parse("0");
                                    DateTime date1;
                                    DateTime date2;
                                    switch (reader["Frequency"].ToString())
                                    {
                                        case "Everyday":
                                            repeat_frequency = TimeSpan.FromDays(1);
                                            break;
                                        case "Weekly":
                                            repeat_frequency = TimeSpan.FromDays(7);
                                            break;
                                        case "Monthly":
                                            date1 = DateTime.Parse(reader["Date"].ToString());
                                            date2 = DateTime.Parse(reader["Date"].ToString()).AddMonths(1);
                                            repeat_frequency = TimeSpan.FromDays((date2 - date1).TotalDays);
                                            break;
                                        case "Yearly":
                                            date1 = DateTime.Parse(reader["Date"].ToString());
                                            date2 = DateTime.Parse(reader["Date"].ToString()).AddYears(1);
                                            repeat_frequency = TimeSpan.FromDays((date2 - date1).TotalDays);
                                            break;
                                    }
                                    var request = new NotificationRequest
                                    {
                                        NotificationId = int.Parse(reader["RequestID"].ToString()),
                                        Title = reader["Description"].ToString(),
                                        Subtitle = "Notification",
                                        Description = reader["WholeDate"].ToString(),
                                        BadgeNumber = 42,
                                        Schedule = new NotificationRequestSchedule
                                        {
                                            NotifyTime = DateTime.Parse(reader["WholeDate"].ToString()),
                                            NotifyRepeatInterval = repeat_frequency
                                        }
                                    };

                                    Notification notification = new Notification(reader["Description"].ToString(), DateOnly.Parse(reader["Date"].ToString())
                                        , TimeOnly.Parse(reader["Time"].ToString()), "00:00", bool.Parse(reader["Repeat"].ToString()), reader["Frequency"].ToString(), request);
                                    notification.DatabaseID = int.Parse(reader["NotificationID"].ToString());
                                    LocalNotificationCenter.Current.Show(notification.Request);
                                    Lists.Notifications.Add(notification);
                                }

                            }
                        }
                    }

                    await connection.CloseAsync();
                    
                }

                catch (SqlException e)
                {
                    await m.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await m.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }
        public static async void MergeData(Page a)
        {
            var taskQuery =
                          from task in Lists.Tasks
                          where task.DatabaseID == null
                          select task;

            var dailyTaskQuery =
                           from task in Lists.Daily_tasks
                           where task.DatabaseID == null
                           select task;

            var notificationQuery =
                           from notfication in Lists.Notifications
                           where notfication.DatabaseID == null
                           select notfication;

            if (taskQuery.Count() > 0 || dailyTaskQuery.Count() > 0 || notificationQuery.Count() > 0)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    try
                    {
                        await connection.OpenAsync();
                        if (taskQuery.Count() > 0)
                        {
                            foreach (Task task in taskQuery)
                            {
                                var sql = "INSERT INTO Tasks (AccountID, Description, Status, Shown) VALUES (@userid,@description, @status, @shown);";
                                await using var command = new SqlCommand(sql, connection);

                                command.Parameters.Add("@userid", SqlDbType.Int).Value = UserID;
                                command.Parameters.Add("@description", SqlDbType.VarChar).Value = task.Description;
                                command.Parameters.Add("@status", SqlDbType.VarChar).Value = task.Status.ToString();
                                command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                                command.ExecuteNonQuery();
                            }
                        }
                        if (dailyTaskQuery.Count() > 0)
                        {
                            foreach (Task task in dailyTaskQuery)
                            {
                                var sql = "INSERT INTO DailyTasks (AccountID, Description, Status, Date) VALUES (@userid, @description, @status, @date);";
                                await using var command = new SqlCommand(sql, connection);

                                command.Parameters.Add("@userid", SqlDbType.Int).Value = UserID;
                                command.Parameters.Add("@description", SqlDbType.VarChar).Value = task.Description;
                                command.Parameters.Add("@status", SqlDbType.VarChar).Value = task.Status.ToString();
                                command.Parameters.Add("@date", SqlDbType.VarChar).Value = task.Date;


                                command.ExecuteNonQuery();
                            }
                        }
                        if (notificationQuery.Count() > 0)
                        {
                            foreach (Notification notification in notificationQuery)
                            {
                                var sql = "INSERT INTO Notifications (AccountID, Description, Date, Time, TimeofNotification, WholeDate, Repeat, Frequency, Repeatmess, " +
                            "DayTime, TimeMess, RequestID) VALUES (@userid, @description, @date, @time, @timeofnotification, @wholedate, " +
                            "@repeat, @frequency, @repeatmess, @daytime, @timemess, @requestid);";
                                await using var command = new SqlCommand(sql, connection);

                                command.Parameters.Add("@userid", SqlDbType.Int).Value = UserID;
                                command.Parameters.Add("@description", SqlDbType.VarChar).Value = notification.Description;
                                command.Parameters.Add("@date", SqlDbType.VarChar).Value = notification.Date;
                                command.Parameters.Add("@time", SqlDbType.VarChar).Value = notification.Time;
                                command.Parameters.Add("@timeofnotification", SqlDbType.DateTime).Value = notification.Time_of_notification;
                                command.Parameters.Add("@wholedate", SqlDbType.DateTime).Value = notification.WholeDate;
                                command.Parameters.Add("@repeat", SqlDbType.VarChar).Value = notification.Repeat.ToString();
                                command.Parameters.Add("@repeatmess", SqlDbType.VarChar).Value = notification.Repeat_mess;
                                command.Parameters.Add("@daytime", SqlDbType.VarChar).Value = notification.Day_time;
                                command.Parameters.Add("@timemess", SqlDbType.VarChar).Value = notification.Time_mess;
                                command.Parameters.Add("@requestid", SqlDbType.Int).Value = notification.Request.NotificationId;
                                SqlParameter param = command.Parameters.AddWithValue("@frequency", notification.Frequency);
                                if (notification.Frequency == null)
                                {
                                    param.Value = DBNull.Value;
                                }

                                command.ExecuteNonQuery();
                            }
                        }

                        await connection.CloseAsync();

                    }
                    catch (SqlException e)
                    {
                        await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    }
                    catch (Exception e)
                    {
                        await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            await connection.DisposeAsync();
                        }

                        Lists.Daily_tasks.Clear();
                        Lists.Tasks.Clear();
                        Lists.Notifications.Clear();
                        Load(a);
                    }
                }
            }
        }

        public static async void ReloadTasks(Page a)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    int ID = 0;
                    if (Lists.Tasks.Count > 0)
                    {
                        ID = Lists.Tasks.Last().DatabaseID;
                    }
                    await connection.OpenAsync();

                    var sql = "SELECT * FROM Tasks WHERE AccountID = @accountID AND TaskID > @databaseID AND Shown LIKE @shown";

                    await using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command.Parameters.Add("@databaseID", SqlDbType.Int).Value = ID;
                    command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Task t = new Task(reader["Description"].ToString(), bool.Parse(reader["Status"].ToString()));
                                t.DatabaseID = int.Parse(reader["TaskID"].ToString());
                                Lists.Tasks.Add(t);
                            }                                                   
                        }
                    }

                    sql = "SELECT * FROM Tasks WHERE AccountID = @accountID AND Shown LIKE @shown";

                    await using var command2 = new SqlCommand(sql, connection);

                    command2.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command2.Parameters.Add("@shown", SqlDbType.VarChar).Value = "False";

                    using (SqlDataReader reader = await command2.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var taskQuery =
                                from task in Lists.Tasks
                                where task.DatabaseID == int.Parse(reader["TaskID"].ToString())
                                select task;
                                if (taskQuery != null)
                                {
                                    foreach (Task t in taskQuery.ToList())
                                    {
                                        Lists.Tasks.Remove(t);
                                    }
                                }
                            }
                        }
                    }

                    sql = "SELECT * FROM Tasks WHERE AccountID = @accountID AND Shown LIKE @shown";

                    await using var command3 = new SqlCommand(sql, connection);

                    command3.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command3.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                    using (SqlDataReader reader = await command3.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            
                            while (reader.Read())
                            {
                                if (Lists.Tasks.Count != 0)
                                {

                                    var taskQuery =
                                    from task in Lists.Tasks
                                    where task.DatabaseID == int.Parse(reader["TaskID"].ToString())
                                    where task.Status != bool.Parse(reader["Status"].ToString())
                                    select task;

                                    if (taskQuery != null)
                                    {
                                        foreach (Task t in taskQuery.ToList())
                                        {
                                            Lists.Tasks.Remove(t);
                                            t.Status = bool.Parse(reader["Status"].ToString());
                                            Lists.Tasks.Add(t);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await connection.CloseAsync();

                }
                catch (SqlException e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                        
                    }
                }
            }
        }

        public static async void ReloadDailyTasks(Page a)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    int ID = 0;
                    if (Lists.Daily_tasks.Count > 0)
                    {
                        ID = Lists.Daily_tasks.Last().DatabaseID;
                    }
                    await connection.OpenAsync();

                    var sql = "SELECT * FROM DailyTasks WHERE AccountID = @accountID AND DailyTaskID > @databaseID AND Shown LIKE @shown";

                    await using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command.Parameters.Add("@databaseID", SqlDbType.Int).Value = ID;
                    command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Task t = new Task(reader["Description"].ToString(), bool.Parse(reader["Status"].ToString()), DateOnly.Parse(reader["Date"].ToString()));
                                t.DatabaseID = int.Parse(reader["DailyTaskID"].ToString());
                                Lists.Daily_tasks.Add(t);
                            }
                        }
                    }

                    sql = "SELECT * FROM DailyTasks WHERE AccountID = @accountID AND Shown LIKE @shown";

                    await using var command2 = new SqlCommand(sql, connection);

                    command2.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command2.Parameters.Add("@shown", SqlDbType.VarChar).Value = "False";

                    using (SqlDataReader reader = await command2.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var taskQuery =
                                from task in Lists.Daily_tasks
                                where task.DatabaseID == int.Parse(reader["DailyTaskID"].ToString())
                                select task;
                                if (taskQuery != null)
                                {
                                    foreach (Task t in taskQuery.ToList())
                                    {
                                        Lists.Daily_tasks.Remove(t);
                                    }
                                }
                            }
                        }
                    }

                    sql = "SELECT * FROM DailyTasks WHERE AccountID = @accountID AND Shown LIKE @shown";

                    await using var command3 = new SqlCommand(sql, connection);

                    command3.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command3.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                    using (SqlDataReader reader = await command3.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                if (Lists.Tasks.Count != 0)
                                {

                                    var taskQuery =
                                    from task in Lists.Daily_tasks
                                    where task.DatabaseID == int.Parse(reader["DailyTaskID"].ToString())
                                    where task.Status != bool.Parse(reader["Status"].ToString())
                                    select task;

                                    if (taskQuery != null)
                                    {
                                        foreach (Task t in taskQuery.ToList())
                                        {
                                            
                                            Lists.Daily_tasks.Remove(t);
                                            t.Status = bool.Parse(reader["Status"].ToString());
                                            Lists.Daily_tasks.Add(t);

                                        }
                                    }
                                }
                            }
                        }
                    }

                    await connection.CloseAsync();

                }
                catch (SqlException e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();

                    }
                }
            }
        }

        public static async void ReloadNotifications(Page a)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    int ID = 0;
                    if (Lists.Notifications.Count > 0)
                    {
                        ID = Lists.Notifications.Last().DatabaseID;
                    }
                    await connection.OpenAsync();

                    var sql = "SELECT * FROM Notifications WHERE AccountID = @accountID AND NotificationID > @databaseID AND Shown LIKE @shown";

                    await using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command.Parameters.Add("@databaseID", SqlDbType.Int).Value = ID;
                    command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (!bool.Parse(reader["Repeat"].ToString()))
                                {
                                    var request = new NotificationRequest
                                    {
                                        NotificationId = int.Parse(reader["RequestID"].ToString()),
                                        Title = reader["Description"].ToString(),
                                        Subtitle = "Notification",
                                        Description = reader["WholeDate"].ToString(),
                                        BadgeNumber = 42,
                                        Schedule = new NotificationRequestSchedule
                                        {
                                            NotifyTime = DateTime.Parse(reader["WholeDate"].ToString())
                                        }
                                    };

                                    Notification notification = new Notification(reader["Description"].ToString(), DateOnly.Parse(reader["Date"].ToString())
                                        , TimeOnly.Parse(reader["Time"].ToString()), "00:00", bool.Parse(reader["Repeat"].ToString()), request);
                                    notification.DatabaseID = int.Parse(reader["NotificationID"].ToString());
                                    LocalNotificationCenter.Current.Show(notification.Request);
                                    Lists.Notifications.Add(notification);
                                }
                                else
                                {
                                    TimeSpan repeat_frequency = TimeSpan.Parse("0");
                                    DateTime date1;
                                    DateTime date2;
                                    switch (reader["Frequency"].ToString())
                                    {
                                        case "Everyday":
                                            repeat_frequency = TimeSpan.FromDays(1);
                                            break;
                                        case "Weekly":
                                            repeat_frequency = TimeSpan.FromDays(7);
                                            break;
                                        case "Monthly":
                                            date1 = DateTime.Parse(reader["Date"].ToString());
                                            date2 = DateTime.Parse(reader["Date"].ToString()).AddMonths(1);
                                            repeat_frequency = TimeSpan.FromDays((date2 - date1).TotalDays);
                                            break;
                                        case "Yearly":
                                            date1 = DateTime.Parse(reader["Date"].ToString());
                                            date2 = DateTime.Parse(reader["Date"].ToString()).AddYears(1);
                                            repeat_frequency = TimeSpan.FromDays((date2 - date1).TotalDays);
                                            break;
                                    }
                                    var request = new NotificationRequest
                                    {
                                        NotificationId = int.Parse(reader["RequestID"].ToString()),
                                        Title = reader["Description"].ToString(),
                                        Subtitle = "Notification",
                                        Description = reader["WholeDate"].ToString(),
                                        BadgeNumber = 42,
                                        Schedule = new NotificationRequestSchedule
                                        {
                                            NotifyTime = DateTime.Parse(reader["WholeDate"].ToString()),
                                            NotifyRepeatInterval = repeat_frequency
                                        }
                                    };

                                    Notification notification = new Notification(reader["Description"].ToString(), DateOnly.Parse(reader["Date"].ToString())
                                        , TimeOnly.Parse(reader["Time"].ToString()), "00:00", bool.Parse(reader["Repeat"].ToString()), reader["Frequency"].ToString(), request);
                                    notification.DatabaseID = int.Parse(reader["NotificationID"].ToString());
                                    Lists.Notifications.Add(notification);
                                }
                            }
                        }
                    }

                    sql = "SELECT * FROM Notifications WHERE AccountID = @accountID AND Shown LIKE @shown";

                    await using var command2 = new SqlCommand(sql, connection);

                    command2.Parameters.Add("@accountID", SqlDbType.Int).Value = UserID;
                    command2.Parameters.Add("@shown", SqlDbType.VarChar).Value = "False";

                    using (SqlDataReader reader = await command2.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var taskQuery =
                                from notification in Lists.Notifications
                                where notification.DatabaseID == int.Parse(reader["NotificationID"].ToString())
                                select notification;
                                if (taskQuery != null)
                                {
                                    foreach (Notification n in taskQuery.ToList())
                                    {
                                        Lists.Notifications.Remove(n);
                                    }
                                }
                            }
                        }
                    }

                    await connection.CloseAsync();

                }
                catch (SqlException e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();

                    }
                }
            }
        }
        public static async void Login(string login, string password, Page a)
        {
            password = sha256_hash(password);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    await connection.OpenAsync();

                    var sql = "SELECT AccountID FROM Accounts WHERE Login =@login AND Password =@password;";
                    await using var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
                    command.Parameters.Add("@password", SqlDbType.VarChar).Value = password;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                userID = reader.GetInt32(0);
                            }
                        }
                        else
                        {
                            await a.DisplayAlert("Error", "Wrong Login Credentials", "OK");
                            await connection.CloseAsync();
                            await connection.DisposeAsync();
                            return;
                        }
                    }
                    await connection.CloseAsync();
                    
                }
                catch (SqlException e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await a.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                    if (userID != 0)
                    {
                        Save_User();
                        bool answer = await a.DisplayAlert("Merge?", "Would you like to merge local data with database?", "Yes", "No");
                        if (answer)
                        {
                            MergeData(a);
                        }
                        else
                        {
                            Lists.Daily_tasks.Clear();
                            Lists.Tasks.Clear();
                            Lists.Notifications.Clear();
                            Load(a);
                        }
                        Device.StartTimer(TimeSpan.FromSeconds(30), () =>
                        {

                            ReloadTasks(a);

                            ReloadDailyTasks(a);

                            ReloadNotifications(a);

                            if (UserID != 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }

                           
                        });

                        await a.Navigation.PopModalAsync();
                    }
                }
            }
        }
        public static async void RegisterAccount(string login, string password, Page r)
        {
            password = sha256_hash(password);
            int user = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var sql = "INSERT INTO Accounts (Login, Password) VALUES (@login, @password);";
                    await using var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
                    command.Parameters.Add("@password", SqlDbType.VarChar).Value = password;
                    command.ExecuteNonQuery();

                    
                }
                catch (SqlException e) when (e.Number == 2627)
                {
                    await r.DisplayAlert("Error", "That Login is already used", "OK");
                }
                catch (Exception e)
                {
                    await r.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }

                    await r.Navigation.PopModalAsync();
                }
            }
        }

        public static async void InsertTask(Page t, Task task)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    if (task.Date == null)
                    {
                        var sql = "INSERT INTO Tasks (AccountID, Description, Status, Shown) VALUES (@userid,@description, @status, @shown);";
                        await using var command = new SqlCommand(sql, connection);

                        command.Parameters.Add("@userid", SqlDbType.Int).Value = UserID;
                        command.Parameters.Add("@description", SqlDbType.VarChar).Value = task.Description;
                        command.Parameters.Add("@status", SqlDbType.VarChar).Value = task.Status.ToString();
                        command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";

                        command.ExecuteNonQuery();

                        sql = "SELECT TOP 1 TaskID FROM Tasks WHERE AccountID = @userid ORDER BY TaskID DESC";

                        await using var command2 = new SqlCommand(sql, connection);

                        command2.Parameters.Add("userid", SqlDbType.Int).Value = UserID;

                        using (SqlDataReader reader = await command2.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    task.DatabaseID = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                    else
                    {
                        var sql = "INSERT INTO DailyTasks (AccountID, Description, Status, Date, Shown) VALUES (@userid, @description, @status, @date, @shown);";
                        await using var command = new SqlCommand(sql, connection);

                        command.Parameters.Add("@userid", SqlDbType.Int).Value = UserID;
                        command.Parameters.Add("@description", SqlDbType.VarChar).Value = task.Description;
                        command.Parameters.Add("@status", SqlDbType.VarChar).Value = task.Status.ToString();
                        command.Parameters.Add("@date", SqlDbType.VarChar).Value = task.Date;
                        command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "True";


                        command.ExecuteNonQuery();

                        sql = "SELECT TOP 1 DailyTaskID FROM DailyTasks WHERE AccountID = @userid ORDER BY DailyTaskID DESC";

                        await using var command2 = new SqlCommand(sql, connection);

                        command2.Parameters.Add("userid", SqlDbType.Int).Value = UserID;

                        using (SqlDataReader reader = await command2.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    task.DatabaseID = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                    await connection.CloseAsync();

                }
                catch (SqlException e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }

        public static async void UpdateTaskStatus(Page t, Task task)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    if (task.Date == null)
                    {
                        var sql = "UPDATE Tasks SET Status = @status WHERE TaskID = @taskID;";
                        await using var command = new SqlCommand(sql, connection);

                        command.Parameters.Add("@taskID", SqlDbType.Int).Value = task.DatabaseID;

                        command.Parameters.Add("@status", SqlDbType.VarChar).Value = task.Status.ToString();

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        var sql = "UPDATE DailyTasks SET Status = @status WHERE DailyTaskID = @taskID;";
                        await using var command = new SqlCommand(sql, connection);

                        command.Parameters.Add("@taskID", SqlDbType.Int).Value = task.DatabaseID;

                        command.Parameters.Add("@status", SqlDbType.VarChar).Value = task.Status.ToString();

                        command.ExecuteNonQuery();
                    }
                    await connection.CloseAsync();
                }
                catch (SqlException e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }

        public static async void UpdateTaskDate(Page t, Task task)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var sql = "UPDATE DailyTasks SET Date = @date WHERE DailyTaskID = @taskID;";
                    await using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@taskID", SqlDbType.Int).Value = task.DatabaseID;

                    command.Parameters.Add("@date", SqlDbType.VarChar).Value = task.Date;

                    command.ExecuteNonQuery();

                    await connection.CloseAsync();
                }
                catch (SqlException e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }

        public static async void DeleteTask(Page t, Task task)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    if (task.Date == null)
                    {
                        var sql = "UPDATE Tasks SET Shown = @shown WHERE TaskID = @taskID;";
                        await using var command = new SqlCommand(sql, connection);

                        command.Parameters.Add("@taskID", SqlDbType.Int).Value = task.DatabaseID;
                        command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "False";

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        var sql = "UPDATE DailyTasks SET Shown = @shown WHERE DailyTaskID = @taskID;";
                        await using var command = new SqlCommand(sql, connection);

                        command.Parameters.Add("@taskID", SqlDbType.Int).Value = task.DatabaseID;
                        command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "False";

                        command.ExecuteNonQuery();
                    }
                    await connection.CloseAsync();
                }
                catch (SqlException e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }
        

        public static async void InsertNotification(Page t, Notification notification)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var sql = "INSERT INTO Notifications (AccountID, Description, Date, Time, TimeofNotification, WholeDate, Repeat, Frequency, Repeatmess, " +
                        "DayTime, TimeMess, RequestID) VALUES (@userid, @description, @date, @time, @timeofnotification, @wholedate, " +
                        "@repeat, @frequency, @repeatmess, @daytime, @timemess, @requestid, true);";
                    await using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@userid", SqlDbType.Int).Value = UserID;
                    command.Parameters.Add("@description", SqlDbType.VarChar).Value = notification.Description;
                    command.Parameters.Add("@date", SqlDbType.VarChar).Value = notification.Date;
                    command.Parameters.Add("@time", SqlDbType.VarChar).Value = notification.Time;
                    command.Parameters.Add("@timeofnotification", SqlDbType.DateTime).Value = notification.Time_of_notification;
                    command.Parameters.Add("@wholedate", SqlDbType.DateTime).Value = notification.WholeDate;
                    command.Parameters.Add("@repeat", SqlDbType.VarChar).Value = notification.Repeat.ToString();
                    command.Parameters.Add("@repeatmess", SqlDbType.VarChar).Value = notification.Repeat_mess;
                    command.Parameters.Add("@daytime", SqlDbType.VarChar).Value = notification.Day_time;
                    command.Parameters.Add("@timemess", SqlDbType.VarChar).Value = notification.Time_mess;
                    command.Parameters.Add("@requestid", SqlDbType.Int).Value = notification.Request.NotificationId;
                    SqlParameter param = command.Parameters.AddWithValue("@frequency", notification.Frequency);
                    if (notification.Frequency == null)
                    {
                        param.Value = DBNull.Value;
                    }

                    command.ExecuteNonQuery();

                    sql = "SELECT TOP 1 NotificationID FROM Notifications WHERE AccountID = @userid ORDER BY NotificationID DESC";

                    await using var command2 = new SqlCommand(sql, connection);

                    command2.Parameters.Add("userid", SqlDbType.Int).Value = UserID;

                    using (SqlDataReader reader = await command2.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                notification.DatabaseID = reader.GetInt32(0);
                            }
                        }
                    }


                    await connection.CloseAsync();
                }
                catch (SqlException e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }

        public static async void UpdateNotification(Page t, Notification notification)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var sql = "UPDATE Notifications SET Date = @date, WholeDate = @wholedate, TimeOfNotification = @timeofnotification" +
                        ", TimeMess = @timemess, DayTime = @daytime WHERE NotificationID = @notificationID;";
                    await using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@notificationID", SqlDbType.Int).Value = notification.DatabaseID;

                    command.Parameters.Add("@date", SqlDbType.VarChar).Value = notification.Date;

                    command.Parameters.Add("@wholedate", SqlDbType.DateTime).Value = notification.WholeDate;

                    command.Parameters.Add("@timeofnotification", SqlDbType.DateTime).Value = notification.Time_of_notification;

                    command.Parameters.Add("@timemess", SqlDbType.VarChar).Value = notification.Time_mess;

                    command.Parameters.Add("@daytime", SqlDbType.VarChar).Value = notification.Day_time;

                    command.ExecuteNonQuery();

                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
                catch (SqlException e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }

        public static async void DeleteNotification(Page t,Notification notification)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();


                    var sql = "UPDATE Notifications SET Shown = @shown WHERE NotificationID = @notificationID;";
                    await using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@notificationID", SqlDbType.Int).Value = notification.DatabaseID;
                    command.Parameters.Add("@shown", SqlDbType.VarChar).Value = "False";

                    command.ExecuteNonQuery();


                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
                catch (SqlException e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                }
                catch (Exception e)
                {
                    await t.DisplayAlert("Error", $"SQL Error: {e.Message}", "OK");
                    UserID = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.DisposeAsync();
                    }
                }
            }
        }


        public static void Save_User()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(int));
            TextWriter writer = new StreamWriter(FileSystem.Current.AppDataDirectory + "/User.xml");
            serializer.Serialize(writer, UserID);
            writer.Close();
        }

        public static void Load_User()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(int));
            FileStream fs = new FileStream(FileSystem.Current.AppDataDirectory + "/User.xml", FileMode.Open);
            UserID = (int)serializer.Deserialize(fs);
            fs.Close();
        }
        public static String sha256_hash(String value)
        {
            using (SHA256 hash = SHA256.Create())
            {
                return String.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }
    }
}
