using System;
using System.Windows;
using Newtonsoft.Json.Linq;
using OpenTok;
using System.IO;
using System.Net;


namespace SurrogateClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 

        private Session _session;
        private Publisher _publisher;
        private Subscriber _subscriber;

        public static readonly string URL = @"https://pscagebot.herokuapp.com/session";

        public MainWindow()
        {
            InitializeComponent();           
        }

        #region getCredentials
        private JObject GetCredentials()
        {
            string html;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (System.IO.Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                

                html = reader.ReadToEnd();
            }

            return JObject.Parse(html);
        }
        #endregion getCredentials

        private void Session_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to session.");
            _session.Publish(_publisher);
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from session.");
        }

        private void Session_Error(object sender, Session.ErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Session error:" + e.ErrorCode);
        }


        /// <summary>
        /// When another client publishes a stream to a session, the Session.StreamReceived message is sent and this method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            if(_subscriber != null)
            {
                _session.Unsubscribe(_subscriber);
                _subscriber.Dispose();
            }

            System.Diagnostics.Debug.WriteLine("Stream received: "+e.Stream.Name);
            _subscriber = new Subscriber(Context.Instance, e.Stream, SubscriberVideo);
            _session.Subscribe(_subscriber);
        }

        /// <summary>
        /// Called if the user presses the connect button. Creates a session and connects to the opentok cloud
        /// </summary>
        /// <param name="sender">not needed</param>
        /// <param name="e">not needed</param>
        private void ConnectClicked(object sender, RoutedEventArgs e)
        {
            _publisher = new Publisher(Context.Instance, renderer: PublisherVideo, name: "drmueller");

            JObject creds = GetCredentials();
            System.Diagnostics.Debug.WriteLine(String.Format("Connecting to server with: {0} {1} {2}", creds.GetValue("apiKey").ToString(), creds.GetValue("sessionId").ToString(), creds.GetValue("token").ToString()));
            _session = new Session(Context.Instance, creds.GetValue("apiKey").ToString(), creds.GetValue("sessionId").ToString());

            _session.Connected += Session_Connected;
            _session.Disconnected += Session_Disconnected;
            _session.Error += Session_Error;
            _session.StreamReceived += Session_StreamReceived;

            _session.Connect(creds.GetValue("token").ToString());
        }

        /// <summary>
        /// Disconnects from the session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectClicked(object sender, RoutedEventArgs e)
        {
            _session.Disconnect();
            _subscriber = null;
        }
    }
}
