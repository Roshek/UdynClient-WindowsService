using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;

using Newtonsoft.Json;
using UdynWindowsService;

//source https://blogs.msdn.microsoft.com/jschaffe/2012/10/23/creating-wix-custom-actions-in-c-and-passing-parameters/
namespace UdynWixInstaller_CreateConfigCA
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CreateConfigFile(Session session)
        {
            try
            {
                session.Log("Begin CreateConfigFile custom action");
                string customActionData = session["CustomActionData"];

                string[] dataArray = customActionData.Split(';');

                string configFile = dataArray[0];
                Config config = new Config
                {
                    Prefix = dataArray[1],
                    Token = dataArray[2],
                    Interval = Int32.Parse(dataArray[3]),
                    LogLevel = (LogLevel)Int32.Parse(dataArray[4])
                };
                string json = JsonConvert.SerializeObject(config);
                File.WriteAllText(configFile, json);
                
                session.Log("End CreateConfigFile custom action");
            }
            catch (Exception ex)
            {
                session.Log("ERROR in custom action CreateConfigFile {0}", 
                    ex.ToString());
                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }
    }
}
