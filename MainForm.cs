using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
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
            client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
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
            Try_Test_Site();

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

        private void Timer_Tick(object sender, EventArgs e)
        {
            Try_Test_Site();
        }

        private async void Try_Test_Site()
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
                    try
                    {
                        DoWork(url, false, ex.Message);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Send Message ---- ERROR" + ":" + ex2.Message);

                    }

                    //AppendNewLine(url + "---- ERROR:" + ex.Message);

                }

            }
            Console.WriteLine("");
        }


        Dictionary<string, bool> siteStatus = new Dictionary<string, bool>();
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        private async void DoWork(string url, bool isOk, string msgStr)

        {
            Console.WriteLine(url + "---- " + (isOk ? "OK" : "ERROR") + ":" + msgStr);

            if (!siteStatus.ContainsKey(url))
            {
                siteStatus.Add(url, true);
            }

            if (siteStatus[url] != isOk)
            {
                siteStatus[url] = isOk;
                string title = isOk ? "站点恢复访问" : "站点无法访问";
                string msg = title + "\r\n\r\n地址 " + url + "\r\n\r\n" + (isOk ? "" : msgStr);
                //string sendUrl = "https://sctapi.ftqq.com/" + appConfig.SendUrl + ".send?title=" + title + "&desp=" + msg;
                string sendUrl = string.Format(appConfig.SendUrl, title, msg); ;
                //Console.WriteLine("send_url :" + send_url);

                //HttpResponseMessage res = await GetAsync(sendUrl);
#if !DEBUG
                HttpResponseMessage res = await GetAsync(sendUrl);
                Console.WriteLine("发送通知 :" + title);
#else
                Console.WriteLine("DEBUG : 不发送通知");
                Console.WriteLine("DEBUG : " + sendUrl);
#endif

            }

        }
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
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
                    Console.WriteLine("开机自启动设置失败 -- " + ee.Message);

                }
            }
            else
            {
                // 取消开机自启动
                Console.WriteLine("取消开机自启动");

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
                    Console.WriteLine("取消开机自启动失败 -- " + ee.Message);
                }
            }
        }

        private void checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            cbx_startup();

            appConfig.AutoStart = checkBoxAutoStart.Checked ? "1" : "0";
            AppConfig.SetVal(appConfig);
        }

        #region fields(菜单栏)
        private const int WM_SYSCOMMAND = 0X112;
        private const int MF_STRING = 0X0;
        private enum SystemMenuItem : int
        {
            Version,
            Setting,
        }
        #endregion

        #region GetSystemMenu 获取系统菜单
        /// <summary>
        /// 获取系统菜单
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="bRevert"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        #endregion

        #region AppendMenu 追加菜单项
        /// <summary>
        /// 追加菜单项
        /// </summary>
        /// <param name="hMenu">菜单指针</param>
        /// <param name="uFlags"></param>
        /// <param name="uIDNewItem"></param>
        /// <param name="lpNewItem"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);
        #endregion

        #region InsertMenu
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hMenu"></param>
        /// <param name="uPosition"></param>
        /// <param name="uFlags"></param>
        /// <param name="uIDNewItem"></param>
        /// <param name="lpNewItem"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);
        #endregion

        #region OnHandleCreated 创建控件
        /// <summary>
        /// 创建控件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var hSysMenu = GetSystemMenu(this.Handle, false);
            //加分割线
            //AppendMenu(hSysMenu, MF_SEPARATOR, 0, String.Empty);

            //加菜单项
            InsertMenu(hSysMenu, 0, MF_STRING, (int)SystemMenuItem.Version, "版本信息");

            InsertMenu(hSysMenu, 0, MF_STRING, (int)SystemMenuItem.Setting, "配置");
            //加分割线
            //AppendMenu(hSysMenu, MF_SEPARATOR, 0, String.Empty);

            //加菜单项
            //AppendMenu(hSysMenu, MF_STRING, (int)SystemMenuItem.Setting, "配置");
        }
        #endregion

        #region WndProc 处理 Windows 消息
        /// <summary>
        /// 处理 Windows 消息
        /// </summary>
        /// <param name="e"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_SYSCOMMAND)
            {
                switch ((SystemMenuItem)m.WParam)
                {
                    case SystemMenuItem.Version:
                        Assembly thisAssem = typeof(MainForm).Assembly;
                        AssemblyName thisAssemName = thisAssem.GetName();
                        Version ver = thisAssemName.Version;
                        MessageBox.Show($"This is version {ver} of {thisAssemName.Name}.", "Version", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case SystemMenuItem.Setting:
                        if (new ConfigKey().ShowDialog() == DialogResult.OK)
                        {
                            appConfig.SendUrl = AppConfig.GetVal().SendUrl;
                        }
                        // DialogHelper.ShowConfirm("你好呀=^_^= ");
                        //MessageBox.Show("","",MessageBoxButtons.OKCancel,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly)
                        //Microsoft.VisualBasic.Interaction.InputBox("Question?", "Title", "Default Text");

                        break;
                }
            }
        }
        #endregion





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
                ostrm = new FileStream(AppDomain.CurrentDomain.BaseDirectory + (DateTime.Now.ToString("yyyyMMdd")) + ".log", FileMode.Append, FileAccess.Write);
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
            streamWriter.WriteLine(dateStr + value);
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
