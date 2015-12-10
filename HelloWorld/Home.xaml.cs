using HelloWorld.Classes;
using HelloWorld.Common;
using System;
using System.Linq;
using System.Diagnostics;
using System.Net;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.ApplicationModel;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HelloWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private SuspendingDeferral def = null;
        public Home()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // Register BackgroundTask for - HeartBeat API
            GetRegiserForHeartbeat();

            //
            App.Current.Suspending += (sender, e) =>
            {
                def = e.SuspendingOperation.GetDeferral();
                ApplicationData.Current.LocalSettings.Values["SuspInfo"] = true;
                SessionBackground();
            };


            App.Current.Resuming += (sender, e) =>
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SuspInfo"))
                {
                    SessionForeground();
                }
            };
        }

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
                return true;

            //Found - Read InstallId
            App.StreetHawkInstallID = await FileIO.ReadTextAsync(installIdFile);

            return true;
        }



        #region "   Session Foreground-Background"


        /// <summary>
        /// Session Background
        /// </summary>
        private async void SessionBackground()
        {
            // Get InstallId
            var isInstallIdFound = await GetInstallId();

            //Return - If not registered
            if (!isInstallIdFound)
                return;

            // HTTP web request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/background");

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
        /// Session Foreground
        /// </summary>
        private async void SessionForeground()
        {
            // Get InstallId
            var isInstallIdFound = await GetInstallId();

            //Return - If not registered
            if (!isInstallIdFound)
                return;

            // HTTP web request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(App.StreetHawkUrl + "/v2/events/foreground");

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

                webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

                if (def != null)
                    def.Complete();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // Complete the Deferral
                if (def != null)
                {
                    try
                    {
                        def.Complete();
                    }
                    catch (Exception) { }
                }
            }
        }

        #endregion


        /// <summary>
        /// Register background
        /// </summary>
        private void GetRegiserForHeartbeat()
        {
            var isTriggerRegistered = false;
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == "TimerTriggeredTask")
                {
                    // To Unregister use next line of code
                    //cur.Value.Unregister(true);

                    isTriggerRegistered = true;
                }
            }

            if (!isTriggerRegistered)
            {
                RegisterBackgroundTask _rct = new RegisterBackgroundTask();

                // 360 minutes (6 hours)
                _rct.registerTimerTrigger(360, "Tasks.TimerTrigger");
            }
        }



        #region NavigationHelper registration

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }



        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        //Options list - Tap
        private void lstOptions_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                switch (lstOptions.SelectedIndex)
                {
                    // Register
                    case 0:
                        Frame rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(Register));
                        break;

                    // Update
                    case 1:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(Update));
                        break;

                    // Heartbeat
                    case 2:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(Heartbeat));
                        break;

                    //View Enter
                    case 3:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(ViewEnter));
                        break;

                    //View Exit
                    case 4:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(ViewExit));
                        break;

                    //Sessions Foreground
                    case 5:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(SessionsForeground));
                        break;

                    //Sessions Background
                    case 6:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(SessionsBackground));
                        break;

                    //Location Updates
                    case 7:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(LocationUpdates));
                        break;

                    //Sharelink
                    case 8:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(Sharelink));
                        break;

                    //Deeplinking
                    case 9:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(Deeplinking));
                        break;

                    //Tagging
                    case 10:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(Tagging));
                        break;

                    //Custom Tagging
                    case 11:
                        rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(CustomTagging));
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
