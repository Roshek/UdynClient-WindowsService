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
using Newtonsoft.Json;

namespace UdynWindowsService
{
    public partial class UdynWindowsService : ServiceBase
    {
        private const string configFileName = "udynconfig.json";
        private const string logFileName = "udyn.log";
        private HttpClient httpClient;
        private Config currentConfig;
        private string currentConfigFile;
        private System.Timers.Timer timer;

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
            timer = new System.Timers.Timer();
            timer.Elapsed += UpdateDyname;

            string installDir = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UdynWindowsService", "ImagePath", "noPath");
            if (installDir.Equals("noPath"))
                return;
            installDir = installDir.Replace("UdynWindowsService.exe", "").Trim("\"".ToCharArray());
            Logger.LogPath = installDir + logFileName;
            Logger.Log("Initializing service...", LogLevel.DEBUG);
            currentConfigFile = installDir + configFileName;

            LoadConfig();

        }

        private void StartUpdate()
        {
            timer.Start();
            Logger.Log("Udyn DDNS service started!\n  Update interval = " + timer.Interval + "ms -- Log Level = " + Logger.LogLevel);
        }

        private void LoadConfig()
        {
            Logger.Log("Loading config file " + currentConfigFile, LogLevel.INFO);
            if (!File.Exists(currentConfigFile))
            {
                Logger.Log("Config file not found! Creating default config.", LogLevel.FATAL);
                currentConfig = new Config
                {
                    Interval = 300000,
                    Prefix = "yourPrefix",
                    Token = "yourToken",
                    LogLevel = LogLevel.INFO
                };
                SaveConfig();
            }
            else
            {
                currentConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(currentConfigFile));
                if (currentConfig != null)
                {
                    timer.Interval = currentConfig.Interval;
                    Logger.LogLevel = currentConfig.LogLevel;
                    Logger.Log("Load successfull!", LogLevel.INFO);
                    StartUpdate();
                    return;
                }
                Logger.Log("Config file syntax error!", LogLevel.FATAL);
            }
            Logger.Log("Please edit the config and restart the service!", LogLevel.ERROR);
            this.Stop();
        }

        private void SaveConfig()
        {
            Logger.Log("Saving config to " + currentConfigFile);
            File.WriteAllText(currentConfigFile, JsonConvert.SerializeObject(currentConfig));
            Logger.Log("Save successful!", LogLevel.DEBUG);
        }

        private async void UpdateDyname(object sender, System.Timers.ElapsedEventArgs e)
        {
            var contentDict = new Dictionary<string, string>
            {
                {"prefix", currentConfig.Prefix },
                {"token", currentConfig.Token}
            };
            HttpResponseMessage response = null;
            string responseData = "";
            try
            {
                Logger.Log("Updating dynamic hostname...", LogLevel.DEBUG);
                response = await httpClient.PostAsync("update/", new FormUrlEncodedContent(contentDict)).ConfigureAwait(false);
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                Logger.Log("Update successful! Response from server: " + responseData, LogLevel.DEBUG);
            }
            catch (HttpRequestException ex)
            {
                Logger.Log("Update failed: HttpRequest Exception " + ex.Message + "\r\n  Server message: " + responseData, LogLevel.ERROR);
            }
        }

        protected override void OnStop()
        {
            Logger.Log("Udyn DDNS service stopped");
            Logger.Separator();
        }
    }
}
