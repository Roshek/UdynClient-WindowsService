using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;
using System.IO;

namespace UdynWindowsService
{
    public enum LogLevel
    {
        MUTE = 0,
        FATAL,
        ERROR,
        WARNING,
        INFO,
        DEBUG,
        TOTAL
    }

    public partial class UdynWindowsService : ServiceBase
    {
        private HttpClient httpClient;
        private LogLevel currentLogLevel;
        public UdynWindowsService()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.BaseAddress = new Uri("https://ddns.aszabados.eu/");
            currentLogLevel = LogLevel.DEBUG;
        }

        protected override void OnStart(string[] args)
        {
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 10000             
            };
            timer.Elapsed += UpdateDyname;
            timer.Start();
            Log("Udyn DDNS service started!\n  Update interval = " + timer.Interval);
        }

        private async void UpdateDyname(object sender, System.Timers.ElapsedEventArgs e)
        {
            var contentDict = new Dictionary<string, string>
            {
                {"prefix", "asd" },
                {"token", "asd"}
            };
            HttpResponseMessage response = null;
            string responseData = "";
            try
            {
                Log("Updating dynamic hostname...", LogLevel.DEBUG);
                response = await httpClient.PostAsync("update/", new FormUrlEncodedContent(contentDict)).ConfigureAwait(false);
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                Log("Update successful! Response from server: " + responseData, LogLevel.INFO);
            }
            catch (HttpRequestException ex)
            {
                Log("Update failed: HttpRequest Exception " + ex.Message + "\r\n  Server message: " + responseData, LogLevel.ERROR);
            }
        }

        protected override void OnStop()
        {
            Log("Udyn DDNS service stopped");
        }

        public void Log(string logMessage, LogLevel level = LogLevel.INFO, string path = @"F:\asd\udynlog.txt")
        {
            if (currentLogLevel < level)
                return;
            using (StreamWriter w = File.AppendText(path))
            {
                w.Write("Log level : [" + level.ToString() + "] -- Time : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine(": {0}", logMessage);
                w.WriteLine("\r\n");
            }
        }
    }
}
