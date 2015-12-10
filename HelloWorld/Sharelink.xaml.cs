using HelloWorld.Classes;
using System;
using System.Net;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HelloWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Sharelink : Page
    {

        string postData = string.Empty;

        public Sharelink()
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

        #region Sharelink

        /// <summary>
        /// Send request and get response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSend_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Check for InstallID
            if (App.StreetHawkInstallID == string.Empty)
            {
                MessageDialog msgbox = new MessageDialog("Please register.", GlobalConst.AppMessageTitle);
                await msgbox.ShowAsync();
                return;
            }

            GrdProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            postData = txtRequest.Text.Trim();
            App.StreetHawkSharelink = string.Empty;

            // HTTP web request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/sharelink");

            // HTTP Headers            
            httpWebRequest.Headers["X-Event-Key"] = App.StreetHawkEventKey;
            httpWebRequest.Headers["X-App-Key"] = App.StreetHawkAppKey;
            httpWebRequest.Headers["X-Installid"] = App.StreetHawkInstallID;

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
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;

                // Post
                using (var stream = webRequest.EndGetRequestStream(asynchronousResult))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    stream.Write(byteArray, 0, byteArray.Length);
                }

                webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
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

                        App.StreetHawkSharelink = result.Replace("{", "").Replace("}", "");

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

        #endregion
    }
}