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

        Session _session;
        Publisher _publisher;

        public static readonly string URL = @"https://pscagebot.herokuapp.com/session";

        public MainWindow()
        {
            InitializeComponent();
            _publisher = new Publisher(Context.Instance, renderer: PublisherVideo, name: "drmueller");

            JObject creds = getCredentials();
            System.Diagnostics.Debug.WriteLine(String.Format("Connecting to server with: {0} {1} {2}",creds.GetValue("apiKey").ToString(), creds.GetValue("sessionId").ToString(), creds.GetValue("token").ToString()));
            _session = new Session(Context.Instance, creds.GetValue("apiKey").ToString(), creds.GetValue("sessionId").ToString());

            _session.Connected += Session_Connected;
            _session.Disconnected += Session_Disconnected;
            _session.Error += Session_Error;
            _session.StreamReceived += Session_StreamReceived;

            _session.Connect(creds.GetValue("token").ToString());
        }


        private JObject getCredentials()
        {
            string html;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (System.IO.Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                //{"apiKey":"46088682","sessionId":"2_MX40NjA4ODY4Mn5-MTUyNTA4NDY0MjEzMX5mMzB3WTNuVHhReXFScngvSlFQNG1WcVd-fg","token":"T1==cGFydG5lcl9pZD00NjA4ODY4MiZzaWc9MTkyNTBiNWE2Mzc2MWQ2NWViYTA5MzExZmQxN2RiNWNmOTQxZWU3NTpzZXNzaW9uX2lkPTJfTVg0ME5qQTRPRFk0TW41LU1UVXlOVEE0TkRZME1qRXpNWDVtTXpCM1dUTnVWSGhSZVhGU2NuZ3ZTbEZRTkcxV2NWZC1mZyZjcmVhdGVfdGltZT0xNTI1MDg1NDgwJnJvbGU9cHVibGlzaGVyJm5vbmNlPTE1MjUwODU0ODAuOTczNjk2OTAwNjQw"}

                html = reader.ReadToEnd();
            }

            return JObject.Parse(html);
        }

        private void Session_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to session.");
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
            Subscriber subscriber = new Subscriber(Context.Instance, e.Stream, SubscriberVideo);
            _session.Subscribe(subscriber);
        }
    }
}
