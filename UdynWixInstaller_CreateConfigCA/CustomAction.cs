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
                session.Log("Configfile: {0} - Prefix: {1} - Token: {2} - Interval: {3} - Loglevel: {4}",
                    session["CONFIGFILE"], session["PREFIX"], session["TOKEN"], session["INTERVAL"], session["LOGLEVEL"]);


                string configFile = session["CONFIGFILE"];
                Config config = new Config
                {
                    Prefix = session["PREFIX"],
                    Token = session["TOKEN"],
                    Interval = Int32.Parse(session["INTERVAL"]),
                    LogLevel = (LogLevel)Int32.Parse(session["LOGLEVEL"])
                };
                File.WriteAllText(configFile, JsonConvert.SerializeObject(config));

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
