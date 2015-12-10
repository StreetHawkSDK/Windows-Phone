using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace HelloWorld.Classes
{
    public class RegisterBackgroundTask
    {
        #region BackgroundTask

        public async void registerTimerTrigger(uint time, string entryPoint)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "TimerTriggeredTask")
                {
                    AttachProgressAndCompletedHandlers(task.Value);
                }
            }

            var _task = BackgroundTaskConfiguration.RegisterBackgroundTask(entryPoint, "TimerTriggeredTask", new TimeTrigger(time, false), null);

            await _task;

            AttachProgressAndCompletedHandlers(_task.Result);

            Debug.WriteLine(entryPoint + " Background task Registered");
        }


        private void AttachProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }


        private void OnProgress(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var progress = "Progress: " + args.Progress + "%";
            BackgroundTaskConfiguration.TimeTriggeredTaskProgress = progress;
        }


        /// <summary>
        /// Execute heartbeat
        /// </summary>
        /// <param name="task"></param>
        /// <param name="args"></param>
        private async void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            try
            {


                //Read Key
                var folder = ApplicationData.Current.LocalFolder;
                var files = await folder.GetFilesAsync();
                var installIdFile = files.FirstOrDefault(x => x.Name == "Installid.txt");

                //Not found - go back
                if (installIdFile == null)
                    return;

                //Found - Read InstallId
                App.StreetHawkInstallID = await FileIO.ReadTextAsync(installIdFile);

                // HTTP web request
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/heartbeat");

                // HTTP Headers
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Headers["X-Event-Key"] = App.StreetHawkEventKey;
                httpWebRequest.Headers["X-App-Key"] = App.StreetHawkAppKey;
                httpWebRequest.Headers["X-Installid"] = App.StreetHawkInstallID;

                // Method
                httpWebRequest.Method = "POST";

                // Request now
                httpWebRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), httpWebRequest);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// HTTP Request Callback
        /// </summary>
        /// <param name="asynchronousResult"></param>
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// HTTP Web Request Callback
        /// </summary>
        /// <param name="asynchronousResult"></param>
        void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest myrequest = (HttpWebRequest)asynchronousResult.AsyncState;
                using (HttpWebResponse response = (HttpWebResponse)myrequest.EndGetResponse(asynchronousResult))
                {
                    System.IO.Stream responseStream = response.GetResponseStream();
                    using (var reader = new System.IO.StreamReader(responseStream))
                    {
                        var result = reader.ReadToEnd();
                    }
                    responseStream.Dispose();
                }
            }
            catch (Exception)
            {

            }
        }


        #endregion

    }
}
