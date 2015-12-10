using HelloWorld.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HelloWorld
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Register : Page
    {
        public Register()
        {
            this.InitializeComponent();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Go back to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        /// <summary>
        /// Send request and get response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GrdProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            // HTTP web request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/register");

            // HTTP Headers
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Headers["X-Event-Key"] = App.StreetHawkEventKey;
            httpWebRequest.Headers["X-App-Key"] = App.StreetHawkAppKey;

            // Method
            httpWebRequest.Method = "POST";

            // Request now
            httpWebRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), httpWebRequest);
        }

        /// <summary>
        /// HTTP Request Callback
        /// </summary>
        /// <param name="asynchronousResult"></param>
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
        }

        /// <summary>
        /// HTTP Web Request Callback
        /// </summary>
        /// <param name="asynchronousResult"></param>
        async void GetResponseCallback(IAsyncResult asynchronousResult)
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

                        RegisterResponse objRegisterResponse = JsonConvert.DeserializeObject<RegisterResponse>(result);

                        App.StreetHawkInstallID = objRegisterResponse.installid;

                        //Save InstallId to file for Heartbeat API call
                        // create storage file in local app storage
                        StorageFile fileSave = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                            "Installid.txt",
                            CreationCollisionOption.ReplaceExisting);

                        //Save now
                        if (fileSave != null)
                        {
                            await FileIO.WriteTextAsync(fileSave, App.StreetHawkInstallID);
                        }


                        // Show response in TextBox
                        IProgress<string> progress = new Progress<string>(UpdateProgress);
                        try
                        {
                            progress.Report(result);
                        }
                        catch (Exception) { }
                    }
                    responseStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                //Update result
                IProgress<string> progress = new Progress<string>(UpdateProgress);
                try
                {
                    progress.Report(ex.Message);
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Update Progress
        /// </summary>
        /// <param name="message"></param>
        private async void UpdateProgress(string message)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtResponse.Text = message;
                GrdProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            });
        }

    }
}
