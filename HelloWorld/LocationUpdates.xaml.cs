using HelloWorld.Classes;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
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
    public sealed partial class LocationUpdates : Page
    {

        string postData = string.Empty;

        public LocationUpdates()
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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                // Geolocator is in the Windows.Devices.Geolocation namespace
                Geolocator geo = new Geolocator();
                // await this because we don't know hpw long it will take to complete and we don't want to block the UI
                Geoposition pos = await geo.GetGeopositionAsync(); // get the raw geoposition data
                double lat = pos.Coordinate.Point.Position.Latitude; // current latitude
                double longt = pos.Coordinate.Point.Position.Longitude; // current longitude

                txtRequest.Text = txtRequest.Text.Replace("#Lat#", lat.ToString());
                txtRequest.Text = txtRequest.Text.Replace("#Long#", longt.ToString());
            }
            catch (Exception ex)
            {
                txtRequest.Text = txtRequest.Text.Replace("#Lat#", "52.516255");
                txtRequest.Text = txtRequest.Text.Replace("#Long#", "13.377763");
                Debug.WriteLine(ex.Message);
            }
        }


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

            // HTTP web request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/locations");

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
