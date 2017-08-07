using Naver.SearchAd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winner
{
    class CommonUtils
    {
        public  static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        public static string GetExternalIPAddress()
        {
            string url = "http://ip.url.kr/";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            
            string resResult = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                StreamReader readerPost = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8, true);
                resResult = readerPost.ReadToEnd();
            }

            Regex regexp = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
            string IP = regexp.Matches(resResult)[0].ToString();


            //int ingNO = resResult.IndexOf("조회 IP");
            //string varTemp = resResult.Substring(ingNO, 50);
            //string realIP = Parsing(Parsing(varTemp, "Current IP Address: ", 1), "</body>", 0).Trim();
            return IP;
    
        }

        public static string Parsing(string _body, string _parseString, int no)
        {
            string varTemp = _body.Split(new string[] { _parseString }, StringSplitOptions.None)[no];
            return varTemp;
        }

        public  static long GetUUID()
        {
            byte[] gb = Guid.NewGuid().ToByteArray();
            int i = BitConverter.ToInt32(gb, 0);
            return BitConverter.ToInt64(gb, 0);
        }

        public  static int GetRandomValue(int min, int max)
        {
            Random random = new Random();
            return random.Next( min, max);            
        }

        public static int GetRandomValue(string min, string max)
        {
            return GetRandomValue(int.Parse(min), int.Parse(max));
        }

        public static void ProcessKillByName(string name)
        {
            Process[] processList = Process.GetProcessesByName(name);
            if (processList.Length > 0)
            {
                for (int i = 0; i < processList.Length; i++)
                {
                    processList[i].Kill();
                }
            }
        }

        public static int GetRandomValue(string v)
        {
            string[] list = v.Split(Configuration.DELIMITER_CHARS);
            return GetRandomValue(list[0], list[1]);
        }
    }

    class DateUtils
    {
        public static long GetCurrentTimeStamp()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - epoch).TotalMilliseconds;            
        }
    }

    class LicenseUtils
    {
        public static string Generate()
        {
            //return Cryptor.Hash.encryptSHA512("ddddddd");
            return null;
        }

    }

    class MobileUtils
    {
        private  static string cmd = Application.StartupPath + @"\adb\adb.exe";

        public static void EnAbleAirPlainMode()
        {            
            // String cmd = "C:\\Users\\JHJ\\Downloads\\adb\\adb.exe";            
            Process process = new Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.FileName = cmd;//设定程序名  
            process.StartInfo.Arguments = " shell settings put global airplane_mode_on 1";
            process.StartInfo.UseShellExecute = false; //关闭shell的使用  
            process.StartInfo.RedirectStandardInput = true; //重定向标准输入  
            process.StartInfo.RedirectStandardOutput = true; //重定向标准输出  
            process.StartInfo.RedirectStandardError = true; //重定向错误输出  
            process.StartInfo.CreateNoWindow = true;//设置不显示窗口  
            process.Start();
            process.WaitForExit();

            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.FileName = cmd;//设定程序名  
            process.StartInfo.Arguments = " shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state true";
            process.StartInfo.UseShellExecute = false; //关闭shell的使用  
            process.StartInfo.RedirectStandardInput = true; //重定向标准输入  
            process.StartInfo.RedirectStandardOutput = true; //重定向标准输出  
            process.StartInfo.RedirectStandardError = true; //重定向错误输出  
            process.StartInfo.CreateNoWindow = true;//设置不显示窗口  
            process.Start();
            process.WaitForExit();
        }

        public static void DisAbleAirPlainMode()
        {
            Process process = new Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.FileName = cmd;//设定程序名  
            process.StartInfo.Arguments = " shell settings put global airplane_mode_on 0";
            process.StartInfo.UseShellExecute = false; //关闭shell的使用  
            process.StartInfo.RedirectStandardInput = true; //重定向标准输入  
            process.StartInfo.RedirectStandardOutput = true; //重定向标准输出  
            process.StartInfo.RedirectStandardError = true; //重定向错误输出  
            process.StartInfo.CreateNoWindow = true;//设置不显示窗口  
            process.Start();
            process.WaitForExit();

            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.FileName = cmd;//设定程序名  
            process.StartInfo.Arguments = " shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state false";
            process.StartInfo.UseShellExecute = false; //关闭shell的使用  
            process.StartInfo.RedirectStandardInput = true; //重定向标准输入  
            process.StartInfo.RedirectStandardOutput = true; //重定向标准输出  
            process.StartInfo.RedirectStandardError = true; //重定向错误输出  
            process.StartInfo.CreateNoWindow = true;//设置不显示窗口  
            process.Start();
            process.WaitForExit();
        }

    }
}
