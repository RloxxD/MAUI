namespace Aplikacja_2;

public partial class Task_list : ContentPage
{
	public Task_list()
	{
		if (Lists.Tasks.Count == 0 && File.Exists(FileSystem.Current.AppDataDirectory + "/Tasks.xml") && DBConnect.UserID == 0)
		{
			Lists.LoadTasks();
		}

        InitializeComponent();

        taskList.ItemsSource = Lists.Tasks;

        delete_But.IsEnabled = false;
		
		

    }

	private async void Add_Task(object sender, EventArgs e)
	{

        string result = await DisplayPromptAsync("Add Task", "Task Description: ");
		if (!string.IsNullOrWhiteSpace(result))
		{
			Task task = new Task(result, false);
			Lists.Tasks.Add(task);
			Lists.SaveTasks();
			if(DBConnect.UserID != 0)
			{
				DBConnect.InsertTask(this, task);
			}
		}
    }

	private void Delete_Task(object sender, EventArgs e)
	{
		int index = Lists.Tasks.IndexOf(taskList.SelectedItem as Task);
		Lists.Tasks.RemoveAt(index);
		delete_But.IsEnabled=false;
		Lists.SaveTasks();

        if (DBConnect.UserID != 0)
        {
            DBConnect.DeleteTask(this, taskList.SelectedItem as Task);
        }
    }

	private async void Status_Change(object sender, EventArgs e)
	{
        var selectedItem = ((SwitchCell)sender).BindingContext as Task;


		if (((SwitchCell)sender).On)
		{
			if (selectedItem != null)
			{
				if (selectedItem.Status == true)
				{
					Lists.SaveTasks();
					if (DBConnect.UserID != 0)
					{
						DBConnect.UpdateTaskStatus(this, selectedItem);
					}
				}
			}

		}
		else
		{
			if (selectedItem != null)
			{
				Lists.SaveTasks();
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
			DBConnect.ReloadTasks(this);
		}	

        base.OnAppearing();

		Task t = Lists.Tasks.FirstOrDefault();
		
    }
}