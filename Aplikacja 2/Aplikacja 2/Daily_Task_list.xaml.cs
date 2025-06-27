namespace Aplikacja_2;

public partial class Daily_Task_list : ContentPage
{
    public Daily_Task_list()
	{
		if (Lists.Tasks.Count == 0 && File.Exists(FileSystem.Current.AppDataDirectory + "/DailyTasks.xml") && DBConnect.UserID == 0)
		{
			Lists.LoadDailyTasks();
		}
		
		


		InitializeComponent();
        dailyTaskList.ItemsSource = Lists.Daily_tasks;

        Device.StartTimer(TimeSpan.FromMinutes(1), () =>
        {
            var taskQuery =
            from task in Lists.Daily_tasks
            where DateOnly.Parse(task.Date) != DateOnly.FromDateTime(DateTime.Now)
            select task;

            if (taskQuery != null)
            {
                foreach (Task t in taskQuery.ToList())
                {
                    t.Date = DateOnly.FromDateTime(DateTime.Now).ToString();
                    t.Status = false;
                    DBConnect.UpdateTaskDate(this, t);
                    
                }
                Lists.SaveDailyTasks();
            }
			dailyTaskList.ItemsSource = Lists.Daily_tasks;
            return true;
        });
        delete_But.IsEnabled = false;
		

    }

	private async void Add_Task(object sender, EventArgs e)
	{
        string result = await DisplayPromptAsync("Add Task", "Task Description: ");
		if (!string.IsNullOrWhiteSpace(result))
		{	
			Task task = new Task(result, false, DateOnly.FromDateTime(DateTime.Now));
			Lists.Daily_tasks.Add(task);
			Lists.SaveDailyTasks();
            if (DBConnect.UserID != 0)
            {
               DBConnect.InsertTask(this, task);
            }
		}
    }

	private void Delete_Task(object sender, EventArgs e)
	{
		int index = Lists.Daily_tasks.IndexOf(dailyTaskList.SelectedItem as Task);
		Lists.Daily_tasks.RemoveAt(index);
		delete_But.IsEnabled=false;
		Lists.SaveDailyTasks();

        if (DBConnect.UserID != 0)
        {
            DBConnect.DeleteTask(this, dailyTaskList.SelectedItem as Task);
        }
    }

	private async void Status_Change(object sender, EventArgs e)
	{

        var selectedItem = ((SwitchCell)sender).BindingContext as Task;

        if (((SwitchCell)sender).On)
        {
            if (selectedItem != null)
            {
                Lists.SaveDailyTasks();
                if (DBConnect.UserID != 0)
                {
                    DBConnect.UpdateTaskStatus(this, selectedItem);
                }
            }
        }
        else
        {
            if (selectedItem != null)
            {
                Lists.SaveDailyTasks();
                if (DBConnect.UserID != 0)
                {
                    DBConnect.UpdateTaskStatus(this, selectedItem);
                }
            }
        }				
	}
	private void Delete(object sender, EventArgs e) 
	{ 
		delete_But.IsEnabled = true;
	}

    protected override void OnAppearing()
    {
        if (DBConnect.UserID != 0)
        {
            DBConnect.ReloadDailyTasks(this);
        }

        var taskQuery =
           from task in Lists.Daily_tasks
           where DateOnly.Parse(task.Date) != DateOnly.FromDateTime(DateTime.Now)
           select task;

        if (taskQuery != null)
        {
            foreach (Task t in taskQuery.ToList())
            {
                t.Date = DateOnly.FromDateTime(DateTime.Now).ToString();
                t.Status = false;
                DBConnect.UpdateTaskDate(this, t);
                DBConnect.UpdateTaskStatus(this, t);

            }
            Lists.SaveDailyTasks();
        }
        dailyTaskList.ItemsSource = Lists.Daily_tasks;

        base.OnAppearing();
    }
}