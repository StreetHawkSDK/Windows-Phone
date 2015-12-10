using HelloWorld.Classes;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
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
    public sealed partial class Update : Page
    {
        string postData = string.Empty;

        public Update()
        {
            this.InitializeComponent();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            //Get device details - OS, Version, Network
            GetDeviceDetails();
        }

        /// <summary>
        /// Get Device Details
        /// </summary>
        private void GetDeviceDetails()
        {

            var d = Application.Current.Resources.Values;

            Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation devideInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();

            //OS Name
            txtRequest.Text = txtRequest.Text.Replace("#operating_system#", "Windows");

            //OS Version
            txtRequest.Text = txtRequest.Text.Replace("#os_version#", "8.1");

            //Model
            txtRequest.Text = txtRequest.Text.Replace("#model#", devideInfo.SystemSku.ToString());

            try
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();

                //Career Name
                txtRequest.Text = txtRequest.Text.Replace("#carrier_name#",
                    profile.WwanConnectionProfileDetails.HomeProviderId.ToString());
            }
            catch (Exception)
            {
                //Career Name
                txtRequest.Text = txtRequest.Text.Replace("#carrier_name#", "");
            }
        }

        #region "VIEW - Enter/Exit"

        Frame rootFrame = null;

        /// <summary>
        /// View Exit Call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
            {
                e.Handled = true;

                // HTTP web request
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/exit");

                // HTTP Headers
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Headers["X-Event-Key"] = App.StreetHawkEventKey;
                httpWebRequest.Headers["X-App-Key"] = App.StreetHawkAppKey;
                httpWebRequest.Headers["X-Installid"] = App.StreetHawkInstallID;

                // Method
                httpWebRequest.Method = "POST";

                // Request now
                httpWebRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallbackForView), httpWebRequest);
            }
        }

        /// <summary>
        /// View Enter Call
        /// </summary>        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // HTTP web request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/enter");

            // HTTP Headers
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Headers["X-Event-Key"] = App.StreetHawkEventKey;
            httpWebRequest.Headers["X-App-Key"] = App.StreetHawkAppKey;
            httpWebRequest.Headers["X-Installid"] = App.StreetHawkInstallID;

            // Method
            httpWebRequest.Method = "POST";

            // Request now
            httpWebRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallbackForView), httpWebRequest);
        }


        /// <summary>
        /// HTTP Request Callback for View
        /// </summary>
        /// <param name="asynchronousResult"></param>
        private void GetRequestStreamCallbackForView(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;

                string viewEnterPostData = "{";
                viewEnterPostData += "\"page\": \"update\"";
                viewEnterPostData += "}";

                // Post
                using (var stream = webRequest.EndGetRequestStream(asynchronousResult))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(viewEnterPostData);
                    stream.Write(byteArray, 0, byteArray.Length);
                }

                webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallbackForView), webRequest);
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
        /// HTTP Web Request Callback for View
        /// </summary>
        /// <param name="asynchronousResult"></param>
        void GetResponseCallbackForView(IAsyncResult asynchronousResult)
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
        #endregion

        #region "   UPDATE"

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
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/update");

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

        #endregion
    }
}
