using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebMonitorAlarm
{
    public partial class MainForm : Form
    {
        public readonly HttpClient client;
        public readonly System.Windows.Forms.Timer timer;
        private readonly AppConfig appConfig;

        public MainForm()
        {
            InitializeComponent();
            timeUnit.SelectedIndex = 0;
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
           


            timer = new System.Windows.Forms.Timer();
            appConfig = AppConfig.GetVal();
            timer.Tick += Timer_Tick;
            new ConsoleHelper(textBoxLog);//重写Console的Write与WriteLine
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            textBoxUrls.Text = appConfig.Urls;
            timeNum.Value = int.Parse(appConfig.TimeNum);
            timeUnit.Text = appConfig.TimeUnit;
            checkBoxAutoStart.Checked = appConfig.AutoStart == "1";

            if (appConfig.Status == "1")
            {
                startNonitor();
            }
            //buttonStart.Text 
            // Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace);
        }

        #region Get请求
        /// <summary>
        /// 发送Get请求
        /// </summary>
        /// <param name="url">网站地址</param>
        /// <returns>返回获取的Html页面</returns>
        public virtual async Task<HttpResponseMessage> GetAsync(string url)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
            return await client.SendAsync(httpRequestMessage);
        }
        #endregion
        private void button1_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                stopNonitor();
            }
            else
            {
                startNonitor();
            }


        }

        private void startNonitor()
        {
            if (timer.Enabled)
            {
                timer.Stop();
            }
            appConfig.Urls = textBoxUrls.Text;
            appConfig.TimeNum = timeNum.Value.ToString();
            appConfig.TimeUnit = timeUnit.Text;
            appConfig.Status = "1";
            AppConfig.SetVal(appConfig);

            int numicVla = (int)timeNum.Value;
            switch (timeUnit.SelectedItem.ToString())
            {
                case "秒":
                    numicVla = numicVla * 1000;
                    break;
                case "分":
                    numicVla = numicVla * 1000 * 60;
                    break;
                case "小时":
                    numicVla = numicVla * 1000 * 60 * 60;
                    break;
                default:
                    numicVla = numicVla * 1000;
                    break;
            }
            timer.Interval = numicVla;
            Timer_Tick(null, null);

            timer.Start();
            buttonStart.Text = "停止";
        }
        private void stopNonitor()
        {
            if (timer.Enabled)
            {
                timer.Stop();

            }
            appConfig.Status = "0";
            AppConfig.SetVal(appConfig);
            buttonStart.Text = "开始";
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {

            string[] urls = textBoxUrls.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string url in urls)
            {
                try
                {
                    HttpResponseMessage res = await GetAsync(url);
                    if (res != null && res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        DoWork(url, true, "");
                        //AppendNewLine(url + "---- OK");
                    }
                    else
                    {
                        DoWork(url, false, res.ReasonPhrase);
                        //AppendNewLine(url + "---- ERROR:" + res.ReasonPhrase);

                    }
                }
                catch (Exception ex)
                {
                    DoWork(url, false, ex.Message);
                    //AppendNewLine(url + "---- ERROR:" + ex.Message);

                }

            }
            Console.WriteLine("");
        }




        Dictionary<string, bool> siteStatus = new Dictionary<string, bool>();
        private async void DoWork(string url, bool isOk, string msg)
        {
            Console.WriteLine(url + "---- " + (isOk ? "OK" : "ERROR") + ":" + msg);

            if (!siteStatus.ContainsKey(url))
            {
                siteStatus.Add(url, true);
            }

            if (siteStatus[url] != isOk)
            {
                siteStatus[url] = isOk;
                string messagetitle = isOk ? "站点恢复访问" : "站点无法访问";
                string messagecontent = messagetitle + "\r\n\r\n地址 " + url + "\r\n\r\n" + (isOk ? "" : msg);
                string sendUrl = "https://sctapi.ftqq.com/" + appConfig.SendKey + ".send?title=" + messagetitle + "&desp=" + messagecontent;
#if !DEBUG
                HttpResponseMessage res = await GetAsync(sendUrl);
                Console.WriteLine("发送通知 :" + messagetitle);
#else
                Console.WriteLine("DEBUG : 不发送通知");
#endif

            }

        }

        private void cbx_startup()
        {
            // 要设置软件名称，有唯一性要求，最好起特别一些
            string SoftWare = "WebMonitorAlarm";

            // 注意this.uiCheckBox1.Checked时针对Winfom程序的，如果是命令行程序要另外设置一个触发值
            if (this.checkBoxAutoStart.Checked)
            {

                Console.WriteLine("设置开机自启动");
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.CurrentUser; //
                // 添加到 当前登陆用户的 注册表启动项     
                try
                {
                    //  
                    //SetValue:存储值的名称   
                    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");

                    // 检测是否之前有设置自启动了，如果设置了，就看值是否一样
                    string old_path = (string)rk2.GetValue(SoftWare);
                    //Console.WriteLine("\r\n旧注册表值: {0}", old_path);

                    if (old_path == null || !path.Equals(old_path))
                    {
                        //Console.WriteLine("\r\n新注册表值: {0}", path);
                        rk2.SetValue(SoftWare, path);
                        Console.WriteLine("添加开机启动成功: {0}", path);
                    }
                    else
                    {
                        Console.WriteLine("开机启动项已存在: {0}", path);
                    }

                    rk2.Close();
                    rk.Close();

                }
                catch (Exception ee)
                {
                    Console.WriteLine("开机自启动设置失败");

                }
            }
            else
            {
                // 取消开机自启动
                Console.WriteLine("取消开机自启动");
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.CurrentUser;
                try
                {
                    // SetValue: 存储值的名称
                    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");

                    string old_path = (string)rk2.GetValue(SoftWare);
                    Console.WriteLine("\r\n注册表值: {0}", old_path);

                    rk2.DeleteValue(SoftWare, false);
                    Console.WriteLine("取消开机启动成功");
                    rk2.Close();
                    rk.Close();
                }
                catch (Exception ee)
                {
                    //MessageBox.Show(ee.Message.ToString(), "提 示", MessageBoxButtons.OK, MessageBoxIcon.Error);  // 提示
                    Console.WriteLine("取消开机自启动失败");
                }
            }
        }

        private void checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            cbx_startup();

            appConfig.AutoStart = checkBoxAutoStart.Checked ? "1" : "0";
            AppConfig.SetVal(appConfig);
        }
    }
    public class ConsoleHelper : TextWriter
    {
        private TextBox _textBox { set; get; }
        private int maxRowLenght = 2000;//textBox中显示的最大行数，若不限制，则置为0

        private FileStream ostrm;
        private StreamWriter streamWriter;

        public ConsoleHelper(TextBox textBox)
        {
            this._textBox = textBox;
            Console.SetOut(this);
            try
            {
                ostrm = new FileStream("./" + (DateTime.Now.ToString("yyyyMMdd")) + ".log", FileMode.Append, FileAccess.Write);
                streamWriter = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }

        }
        public override void Write(string value)
        {
            string dateStr = "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] ";



            streamWriter.Write(dateStr + value);
            streamWriter.Flush();
            if (_textBox.IsHandleCreated)
                _textBox.BeginInvoke(new ThreadStart(() =>
                {
                    if (maxRowLenght > 0 && _textBox.Lines.Length > maxRowLenght)
                    {
                        int strat = _textBox.GetFirstCharIndexFromLine(0);//获取第0行第一个字符的索引
                        int end = _textBox.GetFirstCharIndexFromLine(10);
                        _textBox.Select(strat, end);//选择文本框中的文本范围
                        _textBox.SelectedText = "";//将当前选定的文本内容置为“”
                        _textBox.AppendText(dateStr + value + " ");
                    }
                    else
                    {
                        _textBox.AppendText(dateStr + value + " ");
                    }
                }));


        }

        public override void WriteLine(string value)
        {
            string dateStr = "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] ";
            streamWriter.WriteLine(dateStr+value);
            streamWriter.Flush();

            if (_textBox.IsHandleCreated)
                _textBox.BeginInvoke(new ThreadStart(() =>
                {
                    if (maxRowLenght > 0 && _textBox.Lines.Length > maxRowLenght)
                    {
                        int strat = _textBox.GetFirstCharIndexFromLine(0);//获取第0行第一个字符的索引
                        int end = _textBox.GetFirstCharIndexFromLine(10);
                        _textBox.Select(strat, end);//选择文本框中的文本范围
                        _textBox.SelectedText = "";//将当前选定的文本内容置为“”
                        _textBox.AppendText(dateStr + value + "\r\n");
                    }
                    else
                    {
                        _textBox.AppendText(dateStr + value + "\r\n");
                    }
                }));

        }

        public override Encoding Encoding//这里要注意,重写wirte必须也要重写编码类型
        {
            get { return Encoding.UTF8; }
        }
    }
}
