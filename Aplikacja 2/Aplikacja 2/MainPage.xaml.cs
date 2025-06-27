using Plugin.DeviceSensors;
using Plugin.DeviceSensors.Shared;
using Plugin.Maui.Pedometer;
using System.Diagnostics;

namespace Aplikacja_2
{
    public partial class MainPage : ContentPage
    {
        private int stepCount = 0;
        
      

        
        
        public MainPage()
        {

            if (File.Exists(FileSystem.Current.AppDataDirectory + "/User.xml"))
            {
               DBConnect.Load_User();
            }
            if (DBConnect.UserID != 0)
            {              
                DBConnect.Load(this);

                Device.StartTimer(TimeSpan.FromSeconds(30), () =>
                {

                    DBConnect.ReloadTasks(this);

                    DBConnect.ReloadDailyTasks(this);

                    DBConnect.ReloadNotifications(this);

                    if (DBConnect.UserID != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                });
            }
            if (Lists.Steps.Count == 0 && File.Exists(FileSystem.Current.AppDataDirectory + "/Steps.xml") && DBConnect.UserID == 0)
            {
                Lists.LoadSteps();
            }
            InitializeComponent();
            Device.StartTimer(TimeSpan.FromMinutes(1), () => {
                if (Lists.Steps[0].Date != DateOnly.FromDateTime(DateTime.Now).ToString())
                {
                    ShiftList();
                    l1.Text = Lists.Steps[1].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[1].Counter.ToString();
                    l2.Text = Lists.Steps[2].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[2].Counter.ToString();
                    l3.Text = Lists.Steps[3].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[3].Counter.ToString();
                    l4.Text = Lists.Steps[4].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[4].Counter.ToString();
                    l5.Text = Lists.Steps[5].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[5].Counter.ToString();
                    l6.Text = Lists.Steps[6].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[6].Counter.ToString();

                }
                
                return true;
            });
            ShiftList();
            
            l1.Text = Lists.Steps[1].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[1].Counter.ToString();
            l2.Text = Lists.Steps[2].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[2].Counter.ToString();
            l3.Text = Lists.Steps[3].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[3].Counter.ToString(); 
            l4.Text = Lists.Steps[4].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[4].Counter.ToString(); 
            l5.Text = Lists.Steps[5].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[5].Counter.ToString(); 
            l6.Text = Lists.Steps[6].Date + "\n" + "Steps Taken" + "\n" + Lists.Steps[6].Counter.ToString();
#if WINDOWS
            Steps.Text = $"Step Counter not supported";

            CounterBtn.IsEnabled = false;
#endif
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            if (Pedometer.Default.IsSupported)
            {
                if (Pedometer.Default.IsMonitoring)
                {
                    Pedometer.Default.ReadingChanged -= Count;

                    Pedometer.Default.Stop();

                    CounterBtn.Text = "Start";    
                }
                else if (!Pedometer.Default.IsMonitoring)
                {
                    Pedometer.Default.Start();

                    Pedometer.Default.ReadingChanged += Count;

                    CounterBtn.Text = "Stop";
                }               
            }                                
        }
        private void ShiftList()
        {
            if (Lists.Steps.Count == 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    Step step = new Step(DateOnly.FromDateTime(DateTime.Now.AddDays(-i)), 0);
                    Lists.Steps.Add(step);
                }
                
                Lists.SaveSteps();
                Steps.Text = $"{stepCount}";
                stepCount = Lists.Steps[0].Counter;
            }
            else
            {
                if (DateOnly.Parse(Lists.Steps[0].Date) < DateOnly.FromDateTime(DateTime.Now))
                {
                    int span =  DateOnly.FromDateTime(DateTime.Now).DayNumber - DateOnly.Parse(Lists.Steps[0].Date).DayNumber;
                    if (span > 6)
                    {

                        for (int i = 0; i < 7; i++)
                        {
                            Step step = new Step(DateOnly.FromDateTime(DateTime.Now.AddDays(-i)), 0);
                            Lists.Steps[i] = step;
                        }

                        Lists.SaveSteps();
                        Steps.Text = $"{stepCount}";
                        stepCount = Lists.Steps[0].Counter;
                    }
                    else
                    {
                        for (int i = 0; i < span; i++)
                        {
                            for (int j = 6; j > 0+i; j--)
                            {
                                Lists.Steps[j] = Lists.Steps[j - 1];
                            }
                            Lists.Steps[i] = new Step(DateOnly.FromDateTime(DateTime.Now.AddDays(-i)), 0);
                        }

                        Lists.SaveSteps();
                        Steps.Text = $"{stepCount}";
                        stepCount = Lists.Steps[0].Counter;
                    }
                   
                }
                else
                {
                    stepCount = Lists.Steps[0].Counter;
                    Steps.Text = $"{stepCount}";
                }
            }
        }
        private void Count(object sender, PedometerData e)
        {
            stepCount++;
            Lists.Steps[0].Counter = stepCount;
            Steps.Text = $"{stepCount}";
            SemanticScreenReader.Announce(Steps.Text);
            Lists.SaveSteps();

        }
    }


}
