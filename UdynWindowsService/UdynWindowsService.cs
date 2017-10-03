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
using Microsoft.Win32;

namespace UdynWindowsService
{
    public partial class UdynWindowsService : ServiceBase
    {
        private HttpClient httpClient;
        public UdynWindowsService()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.BaseAddress = new Uri("https://ddns.aszabados.eu/");
            Logger.LogLevel = LogLevel.DEBUG;
        }

        protected override void OnStart(string[] args)
        {
            string installDir = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UdynWindowsService", "ImagePath", "noPath");
            if (installDir.Equals("noPath"))
                return;
            Logger.LogPath = installDir.Replace("UdynWindowsService.exe", "").Trim("\"".ToCharArray()) + "udyn.log";

            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 10000             
            };
            timer.Elapsed += UpdateDyname;
            timer.Start();
            Logger.Log("Udyn DDNS service started!\n  Update interval = " + timer.Interval);
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
                Logger.Log("Updating dynamic hostname...", LogLevel.DEBUG);
                response = await httpClient.PostAsync("update/", new FormUrlEncodedContent(contentDict)).ConfigureAwait(false);
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                Logger.Log("Update successful! Response from server: " + responseData, LogLevel.INFO);
            }
            catch (HttpRequestException ex)
            {
                Logger.Log("Update failed: HttpRequest Exception " + ex.Message + "\r\n  Server message: " + responseData, LogLevel.ERROR);
            }
        }

        protected override void OnStop()
        {
            Logger.Log("Udyn DDNS service stopped");
        }
    }
}
