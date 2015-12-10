using HelloWorld.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace HelloWorld
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;

        /// <summary>
        /// Streethawk Area
        /// </summary>
        //public static string StreetHawkUrl = "https://api.streethawk.com";
        public static string StreetHawkUrl = "https://dev.streethawk.com";
        //public static string StreetHawkAppKey = "HelloWorld";
        //public static string StreetHawkEventKey = "evtoxUsaFA1q7q6jBORv4U0ELU2VaerDQ";

        public static string StreetHawkAppKey = "anurag_dev_test";
        public static string StreetHawkEventKey = "evt5SiE9XVgIovyiS9sjaxv51Khr7UBGX";

        public static string StreetHawkAuthToken = "TgGgyBr0E4HL4td4HRIyzqMVXXsvmn";
        public static string StreetHawkSH_Version = string.Empty;
        public static string StreetHawkInstallID = string.Empty;
        public static string StreetHawkSharelink = string.Empty;
        //{"installid": "7D4GHRIZQ6OL636L"}



        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            //this.Suspending += this.OnSuspending;            
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Handles back button press.  If app is at the root page of app, don't go back and the
        /// system will suspend the app.
        /// </summary>
        /// <param name="sender">The source of the BackPressed event.</param>
        /// <param name="e">Details for the BackPressed event.</param>
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(Home), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Get InstallId
            var isInstallIdFound = await GetInstallId();

            if(!isInstallIdFound)
            {
                GetRegister();
            }

            
            // Ensure the current window is active
            Window.Current.Activate();
        }

        #region Register

        /// <summary>
        /// Get InstallId, if already registered
        /// </summary>
        private async Task<bool> GetInstallId()
        {
            //Read Key
            var folder = ApplicationData.Current.LocalFolder;
            var files = await folder.GetFilesAsync();
            var installIdFile = files.FirstOrDefault(x => x.Name == "Installid.txt");

            //Not found - go back
            if (installIdFile == null)
                return false;

            //Found - Read InstallId
            App.StreetHawkInstallID = await FileIO.ReadTextAsync(installIdFile);

            return true;
        }


        /// <summary>
        /// Send request and get response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetRegister()
        {
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
                    }
                    responseStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                
            }
        }
        #endregion

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
    }
}