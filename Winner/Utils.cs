using Microsoft.Win32;
using Naver.SearchAd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winner
{

    //EJH - 개발자가 될 수 있을까? 
    //EJH_수정테스트
    class CommonUtils
    {
        public static char[] delimiterChars = { ' ', ',', '.', ':', '\t', '/' };
        public static char[] delimiterComma = { ','};
        public static char[] delimiterSlash = { '/' };

        private static string GenerateMACAddress()
        {
            var sBuilder = new StringBuilder();
            var r = new Random();
            int number;
            byte b;
            for (int i = 0; i < 6; i++)
            {
                number = r.Next(0, 255);
                b = Convert.ToByte(number);
                if (i == 0)
                {
                    b = setBit(b, 6); //--> set locally administered
                    b = unsetBit(b, 7); // --> set unicast 
                }
                sBuilder.Append(number.ToString("X2"));
            }
            return sBuilder.ToString().ToUpper();
        }

        private static byte setBit(byte b, int BitNumber)
        {
            if (BitNumber < 8 && BitNumber > -1)
            {
                return (byte)(b | (byte)(0x01 << BitNumber));
            }
            else
            {
                throw new InvalidOperationException(
                "Der Wert für BitNumber " + BitNumber.ToString() + " war nicht im zulässigen Bereich! (BitNumber = (min)0 - (max)7)");
            }
        }

        private static byte unsetBit(byte b, int BitNumber)
        {
            if (BitNumber < 8 && BitNumber > -1)
            {
                return (byte)(b | (byte)(0x00 << BitNumber));
            }
            else
            {
                throw new InvalidOperationException(
                "Der Wert für BitNumber " + BitNumber.ToString() + " war nicht im zulässigen Bereich! (BitNumber = (min)0 - (max)7)");
            }
        }

        public static bool SetMAC(string nicid, string newmac)
        {
            bool ret = false;
            string baseReg = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\";

            using (RegistryKey bkey = Registry.LocalMachine)
            using (RegistryKey key = bkey.OpenSubKey(baseReg + nicid))
            {
                if (key != null)
                {

                    RegistryKey rk = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002bE10318}\\" + nicid, RegistryKeyPermissionCheck.ReadWriteSubTree);

                    if (rk != null)
                    {
                        if (newmac != null)
                        {
                            rk.SetValue("NetworkAddress", newmac);
                        }
                        else
                        {
                            rk.DeleteValue("NetworkAddress");
                        }

                        rk.Close();
                    }


                    ManagementObjectSearcher mos = new ManagementObjectSearcher(new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE Index = " + nicid));

                    foreach (ManagementObject o in mos.Get().OfType<ManagementObject>())
                    {
                        o.InvokeMethod("Disable", null);
                        o.InvokeMethod("Enable", null);
                        ret = true;
                    }
                }
            }

            return ret;
        }

        

        public static string Base64Encoding(string EncodingText, System.Text.Encoding oEncoding = null)
        {
            if (oEncoding == null)
                oEncoding = System.Text.Encoding.UTF8;

            byte[] arr = oEncoding.GetBytes(EncodingText);
            return System.Convert.ToBase64String(arr);
        }

        public static string Base64Decoding(string DecodingText, System.Text.Encoding oEncoding = null)
        {
            if (oEncoding == null)
                oEncoding = System.Text.Encoding.UTF8;

            byte[] arr = System.Convert.FromBase64String(DecodingText);
            return oEncoding.GetString(arr);
        }
        

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

        
        public static object[] MakeArray(Object o)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object[] retArray = new object[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                retArray[i] = fields[i].GetValue(o);
            }

            return retArray;
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

        private static Random random = new Random();
        public  static int GetRandomValue(int min, int max)
        {            
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

        public static string MakeDelimeterItem(params string[] list)
        {            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                sb.Append(list[i]);
                if (i != list.Length - 1)
                {
                    sb.Append("/");
                }                
            }

            return sb.ToString();            
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

    class UIUtils {

        // 그리드뷰에서 현재 선택된 단일 로우를 반환
        public static int GetSelectedRow(Object sender)
        {
            DataGridView dataGridView = (DataGridView)sender;
            int _index = 0;
            if (dataGridView.SelectedRows.Count == 0 && dataGridView.SelectedCells.Count == 0)
            {
                // 선택된 그리드가 없을 경우
                throw new UIException(1000,"선택된 로우가 없습니다.");
                //MessageBox.Show("선택된 슬롯이 없습니다.");
                //return null;
            }
            else if (dataGridView.SelectedRows.Count > 1)
            {
                //MessageBox.Show("수정은 하나의 슬롯만 선택하여 할 수 있습니다.");
                //return null;

                // 여러개의 그리드를 선택 했을 경우
                throw new UIException(1001, "선택된 로우가 여러개 입니다.");
            }
            else {
                
                DataGridViewRow row = null;

                if (dataGridView.SelectedRows.Count == 0)
                {

                    DataGridViewSelectedCellCollection dataGridViewSelectedCells = dataGridView.SelectedCells;
                    int rowIndex = -1;
                    int index = 0;
                    foreach (DataGridViewCell cell in dataGridViewSelectedCells)
                    {

                        if (index > 0)
                        {
                            if (rowIndex != cell.RowIndex)
                            {
                                // 여러개의 그리드를 선택 했을 경우
                                throw new UIException(1001, "선택된 로우가 여러개 입니다.");
                            }
                        }

                        rowIndex = cell.RowIndex;
                        index++;
                    }

                    _index = rowIndex;
                }
                else
                {
                    _index = dataGridView.SelectedRows[0].Index;
                }

                return _index;
            }
        }

        public static List<int> GetSelectedRowIndexs(object sender)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (dataGridView.SelectedRows.Count == 0 && dataGridView.SelectedCells.Count == 0)
            {
                throw new UIException(1000, "선택된 로우가 없습니다.");                
            }           
            else
            {
                DataGridViewRow row = null;
                DataGridViewSelectedCellCollection dataGridViewSelectedCells = dataGridView.SelectedCells;
                List<int> list = new List<int>();
                
                foreach (DataGridViewCell cell in dataGridViewSelectedCells)
                {                        
                    if ( !list.Contains(cell.RowIndex)) {
                        list.Add(cell.RowIndex);
                    }                                                                                                
                }       
                return list;
            }
        }

    }

    class ObjectUtils
    {
        public static bool isNull( string Object)
        {
            if (Object == null || Object.Length == 0)
            {
                return true;
            }

            return false;
        }
    }

    class HanGulUtils
    {

            public struct HANGUL_INFO
            {
                /// <summary>
                ///  한글여부(H, NH)
                /// </summary>
                public string isHangul;
                /// <summary>
                /// 분석 한글
                /// </summary>
                public char originalChar;
                /// <summary>
                /// 분리 된 한글(강 -> ㄱ,ㅏ,ㅇ)
                /// </summary>
                public char[] chars;
            }

            /// <summary>
            /// 초성 리스트
            /// </summary>
            public static readonly string HTable_ChoSung = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
            /// <summary>
            /// 중성 리스트
            /// </summary>
            public static readonly string HTable_JungSung = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
            /// <summary>
            /// 종성 리스트
            /// </summary>
            public static readonly string HTable_JongSung = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
            private static readonly ushort m_UniCodeHangulBase = 0xAC00;
            private static readonly ushort m_UniCodeHangulLast = 0xD79F;

            /// <summary>
            /// 생성자
            /// </summary>
            public HanGulUtils() { }

            /// <summary>
            /// 초성, 충성, 종성으로 이루어진 한글을 한글자의 한글로 만든다.
            /// </summary>
            /// <param name="choSung">초성</param>
            /// <param name="jungSung">중성</param>
            /// <param name="jongSung">종성</param>
            /// <returns>합쳐진 글자</returns>
            /// <remarks>
            /// <para>
            /// 초성, 충성, 종성으로 이루어진 한글을 한글자의 한글로 만든다.
            /// </para>
            /// <example>
            /// <code>
            /// string choSung = "ㄱ", jungSung = "ㅏ", jongSung = "ㅇ";
            /// char hangul = MergeJaso(choSung, jungSung, jongSung);
            /// // 결과 -> 강
            /// </code>
            /// </example>
            /// </remarks>
            public static char MergeJaso(string choSung, string jungSung, string jongSung)
            {
                int ChoSungPos, JungSungPos, JongSungPos;
                int nUniCode;

                ChoSungPos = HTable_ChoSung.IndexOf(choSung);    // 초성 위치
                JungSungPos = HTable_JungSung.IndexOf(jungSung);   // 중성 위치
                JongSungPos = HTable_JongSung.IndexOf(jongSung);   // 종성 위치

                // 앞서 만들어 낸 계산식
                nUniCode = m_UniCodeHangulBase + (ChoSungPos * 21 + JungSungPos) * 28 + JongSungPos;

                // 코드값을 문자로 변환
                char temp = Convert.ToChar(nUniCode);

                return temp;
            }

            /// <summary>
            /// 한글자의 한글을 초성, 중성, 종성으로 나눈다.
            /// </summary>
            /// <param name="hanChar">한글</param>
            /// <returns>분리된 한글에 대한 정보</returns>
            /// <seealso cref="HANGUL_INFO"/>
            /// <remarks>
            /// <para>
            /// 한글자의 한글을 초성, 중성, 종성으로 나눈다.
            /// </para>
            /// <example>
            /// <code>
            /// HANGUL_INFO hinfo = DevideJaso('강');
            /// // hinfo.isHangul -> "H" (한글)
            /// // hinfo.originalChar -> 강
            /// // hinfo.chars[0] -> ㄱ, hinfo.chars[1] -> ㄴ, hinfo.chars[2] = ㅇ
            /// </code>
            /// </example>
            /// </remarks>
            public static HANGUL_INFO DevideJaso(char hanChar)
            {
                int ChoSung, JungSung, JongSung;    // 초성,중성,종성의 인덱스
                ushort temp = 0x0000;                // 임시로 코드값을 담을 변수
                HANGUL_INFO hi = new HANGUL_INFO();

                //Char을 16비트 부호없는 정수형 형태로 변환 - Unicode
                temp = Convert.ToUInt16(hanChar);

                // 캐릭터가 한글이 아닐 경우 처리
                if ((temp < m_UniCodeHangulBase) || (temp > m_UniCodeHangulLast))
                {
                    hi.isHangul = "NH";
                    hi.originalChar = hanChar;
                    hi.chars = null;
                }
                else
                {
                    // nUniCode에 한글코드에 대한 유니코드 위치를 담고 이를 이용해 인덱스 계산.
                    int nUniCode = temp - m_UniCodeHangulBase;
                    ChoSung = nUniCode / (21 * 28);
                    nUniCode = nUniCode % (21 * 28);
                    JungSung = nUniCode / 28;
                    nUniCode = nUniCode % 28;
                    JongSung = nUniCode;

                    hi.isHangul = "H";
                    hi.originalChar = hanChar;
                    hi.chars = new char[] { HTable_ChoSung[ChoSung], HTable_JungSung[JungSung], HTable_JongSung[JongSung] };
                }

                return hi;
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
