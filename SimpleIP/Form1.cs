using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace webbrowser代理ip
{
    public partial class Form1 : Form
    {
        private static string SQLCONNECTION = ConfigurationManager.ConnectionStrings["SqlCon"].ConnectionString;


        private DataTable dt = null;

        private async void InitData()
        {
            await Task.Run(() =>
            {
                using (SqlConnection con = new SqlConnection(SQLCONNECTION))
                {
                    SqlDataAdapter adp = new SqlDataAdapter("SELECT IP,Port FROM dbo.p_IPProxy WHERE Type='HTTP'", con);
                    dt = new DataTable();
                    adp.Fill(dt);
                    dataGridView1.BeginInvoke(new Action(() =>
                    {
                        dataGridView1.DataSource = dt;
                    }));
                }
            });
        }

        public Form1()
        {
            InitializeComponent();
            InitData();
            //屏蔽脚本报错提示
            webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebLoadCompleted);
        }

        #region 设置代理IP
        private void button2_Click(object sender, EventArgs e)
        {
            string proxy = this.textBox1.Text;
            RefreshIESettings(proxy);
            IEProxy ie = new IEProxy(proxy);
            ie.RefreshIESettings();
            //MessageBox.Show(ie.RefreshIESettings().ToString());
        }
        #endregion
        #region 取消代理IP
        private void button3_Click(object sender, EventArgs e)
        {
            IEProxy ie = new IEProxy(null);
            ie.DisableIEProxy();
        }
        #endregion
        #region 打开网页
        private void button1_Click(object sender, EventArgs e)
        {
            string url = txt_url.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请输入要打开的网址");
                return;
            }
            this.webBrowser1.Navigate(url, null, null, null);
        }
        #endregion
        #region 代理IP
        public struct Struct_INTERNET_PROXY_INFO
        {
            public int dwAccessType;
            public IntPtr proxy;
            public IntPtr proxyBypass;
        };
        //strProxy为代理IP:端口
        private void RefreshIESettings(string strProxy)
        {
            const int INTERNET_OPTION_PROXY = 38;
            const int INTERNET_OPEN_TYPE_PROXY = 3;
            const int INTERNET_OPEN_TYPE_DIRECT = 1;

            Struct_INTERNET_PROXY_INFO struct_IPI;
            // Filling in structure
            struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_PROXY;
            struct_IPI.proxy = Marshal.StringToHGlobalAnsi(strProxy);
            struct_IPI.proxyBypass = Marshal.StringToHGlobalAnsi("local");

            // Allocating memory
            IntPtr intptrStruct = Marshal.AllocCoTaskMem(Marshal.SizeOf(struct_IPI));
            if (string.IsNullOrEmpty(strProxy) || strProxy.Trim().Length == 0)
            {
                strProxy = string.Empty;
                struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_DIRECT;

            }
            // Converting structure to IntPtr
            Marshal.StructureToPtr(struct_IPI, intptrStruct, true);

            bool iReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, Marshal.SizeOf(struct_IPI));
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
        public class IEProxy
        {
            private const int INTERNET_OPTION_PROXY = 38;
            private const int INTERNET_OPEN_TYPE_PROXY = 3;
            private const int INTERNET_OPEN_TYPE_DIRECT = 1;

            private string ProxyStr;


            [DllImport("wininet.dll", SetLastError = true)]

            private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

            public struct Struct_INTERNET_PROXY_INFO
            {
                public int dwAccessType;
                public IntPtr proxy;
                public IntPtr proxyBypass;
            }

            private bool InternetSetOption(string strProxy)
            {
                int bufferLength;
                IntPtr intptrStruct;
                Struct_INTERNET_PROXY_INFO struct_IPI;

                if (string.IsNullOrEmpty(strProxy) || strProxy.Trim().Length == 0)
                {
                    strProxy = string.Empty;
                    struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_DIRECT;
                }
                else
                {
                    struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_PROXY;
                }
                struct_IPI.proxy = Marshal.StringToHGlobalAnsi(strProxy);
                struct_IPI.proxyBypass = Marshal.StringToHGlobalAnsi("local");
                bufferLength = Marshal.SizeOf(struct_IPI);
                intptrStruct = Marshal.AllocCoTaskMem(bufferLength);
                Marshal.StructureToPtr(struct_IPI, intptrStruct, true);
                return InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, bufferLength);

            }
            public IEProxy(string strProxy)
            {
                this.ProxyStr = strProxy;
            }
            //设置代理
            public bool RefreshIESettings()
            {
                return InternetSetOption(this.ProxyStr);
            }
            //取消代理
            public bool DisableIEProxy()
            {
                return InternetSetOption(string.Empty);
            }
        }
        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            string url = txt_url.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请输入要打开的网址");
                return;
            }
            //让主线程去访问设置提示信息
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("当前进程正在批量设置，请等待本次操作完成！");
                return;
            }
            backgroundWorker1.RunWorkerAsync(dt);
        }

        /// <summary>  
        /// 是否能 Ping 通指定的主机  
        /// </summary>  
        /// <param name="ip">ip 地址或主机名或域名</param>  
        /// <returns>true 通，false 不通</returns>  
        private static bool Ping(string ip)
        {
            Ping p = new Ping();
            int timeout = 1000;
            PingReply reply = p.Send(ip, timeout);
            return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                this.textBox1.Text = string.Format("{0}:{1}", row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString());
                button2_Click(null, null);
                button1_Click(null, null);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("批量代理执行完成");
        }

        private void WebLoadCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            DataTable dt = (DataTable)e.Argument;
            BackgroundWorker bw = (BackgroundWorker)sender;

            foreach (DataRow dr in dt.Rows)
            {
                //没有取消后台操作
                if (!bw.CancellationPending)
                {
                    string proxy = string.Format("{0}:{1}", dr["IP"].ToString(), dr["Port"]);
                    if (Ping(dr["IP"].ToString()))
                    {
                        RefreshIESettings(proxy);
                        IEProxy ie = new IEProxy(proxy);
                        if (ie.RefreshIESettings())
                        {
                            webBrowser1.Navigate(txt_url.Text.Trim(), null, null, null);
                        }
                    }
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (webBrowser1 != null && !webBrowser1.IsDisposed)
                {
                    webBrowser1.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}