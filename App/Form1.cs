using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;  

namespace App
{
    public partial class Form1 : Form
    {
        private SerialPort comm = new SerialPort();
        private StringBuilder builder = new StringBuilder();
         private long received_count = 0;//接收计数
         private long send_count = 0;//发送计数
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;   //防止跨线程访问出错，好多地方会用到  
           
            button1.Text = "打开串口";
            button1.Enabled = Enabled;
            int[] item = { 9600, 115200 };    //定义一个Item数组，遍历item中每一个变量a，增加到comboBox2的列表中  
            foreach (int a in item)
            {
                comboBox2.Items.Add(a.ToString());
            }
            comboBox2.SelectedItem = comboBox2.Items[1];    //默认为列表第二个变量  

            try
            {
                //button1.Text = "全部串口";
                comboBox1.Items.Clear();
                string[] str = SerialPort.GetPortNames();
                for (int i = 0; i < str.Length; i++)
                {
                    comboBox1.Items.Add(str[i]);
                }
                comboBox1.SelectedIndex = 1;
            }
            catch (Exception)
            {
                MessageBox.Show("没有可用端口", "提示");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comm.IsOpen)
            {
                comm.Close();
                button1.Text = "打开串口";
            }
            else
            {
                if (comboBox1.Text == "")
                {
                    MessageBox.Show("没有串口可用");
                    return;
                }
                //关闭时点击，
                comm.PortName = comboBox1.Text;//端口号
                comm.BaudRate = int.Parse(comboBox2.Text);
                comm.DataReceived += Comm_DataReceived ;
                try
                {
                    comm.Open();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                button1.Text = comm.IsOpen ? "关闭串口" : "打开串口";
                button1.Enabled = comm.IsOpen ? true : false;

            }

        }

        private void Comm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //throw new NotImplementedException();
            int n = comm.BytesToRead;//获取缓存字节数
            byte[] buf = new byte[n];//存储串口数据
            received_count += n;//接收计数
            comm.Read(buf,0,n);//读取串口缓存数据
            builder.Clear();
            string mystr1 = "[接收:"+DateTime.Now.ToString("HH:mm:ss:fff")+ "]->";
            this.Invoke((EventHandler)(delegate
            {  
                if(checkBox1.Checked)
                {
                    foreach(byte b in buf)
                   {
                        builder.Append(b.ToString("X2")+' ');
                   }
                }
                else
                {
                    builder.Append(Encoding.Default.GetString(buf));   
                }
                this.richTextBox1.AppendText(mystr1 + builder.ToString());
                this.richTextBox1.AppendText("\r\n");
                this.label3.Text = "Rev:" + received_count.ToString();
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
            this.label3.Text = "Rev:0";
            received_count = 0;
           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool lineflag = false;
            int n = 0;
            if (!comm.IsOpen)
            {
                MessageBox.Show("请打开串口!");
                return;
            }
            if(checkline.Checked)
            {
                lineflag = true;
            }
            if(checkhex.Checked)
            {
                MessageBox.Show("错误!");
            }
            else
            {
                if (lineflag == true)
                {
                    comm.Write(richTextBox2.Text + "\r\n");
                    n = richTextBox2.Text.Length + 2;
                }
                else//不包含换行符 
                {
                    comm.Write(richTextBox2.Text);
                    n = richTextBox2.Text.Length;
                }
            }
            send_count = send_count + n;
            this.Send6.Text = "Send:" + send_count;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.richTextBox2.Clear();
            send_count = 0;
            this.Send6.Text = "Send:" + send_count;
        }
    }
}
