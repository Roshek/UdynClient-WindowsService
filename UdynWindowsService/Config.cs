using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdynWindowsService
{
    public class Config
    {
        public string Prefix { get; set; }
        public string Token { get; set; }
        public int Interval { get; set; }
        public LogLevel LogLevel { get; set; }
    }
}
