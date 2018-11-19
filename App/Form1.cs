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
using System.Text.RegularExpressions;
using System.IO;

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
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button11.Enabled = false;
            button12.Enabled = false;
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
                button5.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                button11.Enabled = false;
                button12.Enabled = false;
                this.textBox1.Text = "";
                this.textBox2.Text = "";
                this.textBox3.Text = "";
                this.textBox4.Text = "";
                this.textBox5.Text = "";
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
                    return;
                }
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                button11.Enabled = true;
                button12.Enabled = true;
                this.textBox1.Text = "A5 FF 03 14 01 44";//open beep
                this.textBox2.Text = "A5 FF 03 14 00 45";//close beep
                this.textBox4.Text = "A5 FF 02 10 4A";//reset
                this.textBox3.Text = "A5 FF 03 18 01 40";//2.4Ghz close
                this.textBox5.Text = "A5 FF 02 1F 3B";//恢复出厂
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
                this.richTextBox1.AppendText(mystr1 + builder.ToString() + "\r\n");
                //this.richTextBox1.AppendText("\r\n");
                this.label3.Text = "Rev:" + received_count.ToString();
               // richTextBox1.DiscardInBuffer();//丢弃接收缓冲区数据
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
                MessageBox.Show("先打开串口!");
                return;
            }
            if (string.IsNullOrWhiteSpace(richTextBox2.Text))
            {
                MessageBox.Show("请输入发送内容!");
                return;
            }
            if(checkline.Checked)
            {
                lineflag = true;
            }
            if(checkhex.Checked)
            {
                MatchCollection mc = Regex.Matches(richTextBox2.Text, @"(?i)[\da-f]{2}");
                List<byte> buf = new List<byte>();//填充到这个临时列表中 //依次添加到列表中 
                foreach (Match m in mc)
                {
                    buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
                }
                //转换列表为数组后发送
                comm.Write(buf.ToArray(), 0, buf.Count);
                if(lineflag == true)
                {
                    comm.WriteLine("\r\n");
                    n = buf.Count + 2;
                }
                else
                {
                    n = buf.Count;
                }
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

        private void button5_Click(object sender, EventArgs e)//beep open
        {
            MatchCollection mc = Regex.Matches(textBox1.Text, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中 //依次添加到列表中 
            foreach (Match m in mc)
            {
                buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
            }
            //转换列表为数组后发送
            comm.Write(buf.ToArray(), 0, buf.Count);

            string mystr1 = "[发送:" + DateTime.Now.ToString("HH:mm:ss:fff") + "]->";

            this.richTextBox1.AppendText(mystr1 + this.textBox1.Text + "\r\n");
        }

        private void button12_Click(object sender, EventArgs e)//beep close
        {
            MatchCollection mc = Regex.Matches(textBox2.Text, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中 //依次添加到列表中 
            foreach (Match m in mc)
            {
                buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
            }
            //转换列表为数组后发送
            comm.Write(buf.ToArray(), 0, buf.Count);
            string mystr1 = "[发送:" + DateTime.Now.ToString("HH:mm:ss:fff") + "]->";

            this.richTextBox1.AppendText(mystr1 + this.textBox2.Text + "\r\n");
        }

        private void button11_Click(object sender, EventArgs e)//2.4ghz close
        {
            MatchCollection mc = Regex.Matches(textBox3.Text, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中 //依次添加到列表中 
            foreach (Match m in mc)
            {
                buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
            }
            //转换列表为数组后发送
            comm.Write(buf.ToArray(), 0, buf.Count);
            string mystr1 = "[发送:" + DateTime.Now.ToString("HH:mm:ss:fff") + "]->";

            this.richTextBox1.AppendText(mystr1 + this.textBox3.Text + "\r\n");
        }

        private void button6_Click(object sender, EventArgs e)//reset
        {
            MatchCollection mc = Regex.Matches(textBox4.Text, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中 //依次添加到列表中 
            foreach (Match m in mc)
            {
                buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
            }
            //转换列表为数组后发送
            comm.Write(buf.ToArray(), 0, buf.Count);
            string mystr1 = "[发送:" + DateTime.Now.ToString("HH:mm:ss:fff") + "]->";

            this.richTextBox1.AppendText(mystr1 + this.textBox4.Text + "\r\n");
        }

        private void button7_Click(object sender, EventArgs e)//Restore Factory
        {
            MatchCollection mc = Regex.Matches(textBox5.Text, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中 //依次添加到列表中 
            foreach (Match m in mc)
            {
                buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
            }
            //转换列表为数组后发送
            comm.Write(buf.ToArray(), 0, buf.Count);
            string mystr1 = "[发送:" + DateTime.Now.ToString("HH:mm:ss:fff") + "]->";

            this.richTextBox1.AppendText(mystr1 + this.textBox5.Text + "\r\n");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog objSave = new System.Windows.Forms.SaveFileDialog();
            objSave.Filter = "(*.txt)|*.txt|" + "(*.*)|*.*";
            objSave.FileName = "文件名" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";

            if (objSave.ShowDialog() == DialogResult.OK)
            {
                this.richTextBox1.SaveFile(objSave.FileName, RichTextBoxStreamType.PlainText);//重点在此句
            }

        }
    }
}
