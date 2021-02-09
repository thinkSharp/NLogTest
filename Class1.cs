using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDna.Integration;
using NLog;
using Newtonsoft.Json;
using System.Net.Http;
using System.Configuration;
namespace NLogTest
{
    public static class Class1
    {
        //    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        //    [ExcelFunction(Description = "My first .NET function", Name ="Test.Log")] 
        //    public static string HelloDna(string name)
        //    {
        //        //var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //        //var settings = configFile.AppSettings.Settings;
        //        //var test = settings["url"];
        //        logger.Info("HelloDna function called: {0} ", name);
        //        return "Hello " + name;
        //    }


        [ExcelFunction(Description = "My first .NET function", Name = "Test.ReadSettings")]
        public static string ReadSettings(string name)
        {
            string test_url = ConfigurationManager.AppSettings["test-url"];
            //logger.Info("HelloDna function called: {0} = {1}", name, test_url);
            return "Hello " + name + " setting value: " + test_url;
        }


    }
}

