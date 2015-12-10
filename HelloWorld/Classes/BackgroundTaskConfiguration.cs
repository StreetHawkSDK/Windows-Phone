using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace HelloWorld.Classes
{
    public class BackgroundTaskConfiguration
    {
        public const string TimeTriggeredTaskName = "TimerTriggeredTask";
        public static string TimeTriggeredTaskProgress = "";
        public static bool TimeTriggeredTaskRegistered = false;

        //register the Timer Trigger Task

        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            if (TaskRequiresBackgroundAccess(name))
            {
                await BackgroundExecutionManager.RequestAccessAsync();
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);

                //
                // If the condition changes while the background task is executing then it will
                // be canceled.
                //
                builder.CancelOnConditionLoss = true;
            }

            BackgroundTaskRegistration task = builder.Register();

            UpdateBackgroundTaskStatus(name, true);

            //
            // Remove previous completion status from local settings.
            //
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(name);

            return task;
        }

        //unregister the background task

        public static void UnregisterBackgroundTasks(string name)
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                    Debug.WriteLine("Unregistered: " + name);
                }
            }

            UpdateBackgroundTaskStatus(name, false);


        }

        //update the background task status

        public static void UpdateBackgroundTaskStatus(String name, bool registered)
        {
            if (name == TimeTriggeredTaskName)
            {
                TimeTriggeredTaskRegistered = registered;
            }
        }

        //get the background task status

        public static String GetBackgroundTaskStatus(String name)
        {
            var registered = false;

            if (name == TimeTriggeredTaskName)
            {
                registered = TimeTriggeredTaskRegistered;
            }

            var status = registered ? "Registered" : "Unregistered";

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(name))
            {
                status += " - " + settings.Values[name].ToString();
            }

            return status;
        }


        public static bool TaskRequiresBackgroundAccess(String name)
        {
            return true;
        }
    }
}
