using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NRecoVideoConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string sSourceFile;
        public string sTargetFile;
        delegate void _Convert();
        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(sSourceFile)&& !string.IsNullOrEmpty(sTargetFile))
            {
                //_Convert DelConvert = new _Convert(Convert);
                //DelConvert.Invoke();
                Thread t1 = new Thread(Convert);
                t1.Start();
                btnConvert.Enabled = false;
                TargetFile.Enabled = false;
                SourceFile.Enabled = false;
            }
            else
            {
                MessageBox.Show("請先決定來源與目標檔案位置名稱");
            }

        }
       
        private void SourceFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDialog = new OpenFileDialog();

            if (OpenDialog.ShowDialog() == DialogResult.OK)
            {
                sSourceFile = OpenDialog.FileName;
                label1.Text = sSourceFile;
            }
            TargetFile.Enabled = true;
        }

        private void TargetFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDialog = new SaveFileDialog();

            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                sTargetFile = SaveDialog.FileName;
                label2.Text = sTargetFile;
            }
            btnConvert.Enabled = true;
        }
        public static FieldInfo[] fieldInfos = typeof(Format).GetFields(BindingFlags.Public |
         BindingFlags.Static | BindingFlags.FlattenHierarchy);

        public List<FieldInfo> VideoTypes = fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        private void Form1_Load(object sender, EventArgs e)
        {
            var test = typeof(Format).GetProperties();
            foreach (var item in VideoTypes)
            {
                comboBox1.Items.Add(item.Name);
            }
            comboBox1.SelectedIndex = 0;
            TargetFile.Enabled = false;
            btnConvert.Enabled = false;
        }


        string Combox1SelectItem;
        public void GetComboBoxItem()
        {
            Combox1SelectItem = (string)comboBox1.SelectedItem;
        }
        public void Convert()
        {
            var ffMpeg = new FFMpegConverter();

            comboBox1.InvokeIfRequired(GetComboBoxItem);
            ConvertSettings setting = new ConvertSettings();
            //string sTargetType = (string)comboBox1.SelectedItem;
            string sTargetType = Combox1SelectItem;
            FieldInfo Info = VideoTypes.Where(x => x.Name == sTargetType).Select(y => y).FirstOrDefault();
            setting.AudioSampleRate = 44100;
            label4.InvokeIfRequired(() =>
            {
                label4.ForeColor = Color.Red;
                label4.Text = "轉檔中 請稍後";
            });
            try
            {
                ffMpeg.ConvertMedia(sSourceFile, null, sTargetFile + "." + Info.Name, Info.GetValue(null).ToString(), setting);
            }
            catch (Exception)
            {

                MessageBox.Show("轉檔失敗");
            }


            Console.WriteLine("Finish");

            //lambda非同步UI更新
            btnConvert.InvokeIfRequired(() =>
            {
                btnConvert.Enabled = true;
            });
            TargetFile.InvokeIfRequired(() =>
            {
                TargetFile.Enabled = false;
            });

            label4.InvokeIfRequired(() =>
            {
                label4.Text = "轉檔完畢，可以繼續進行下個轉換";
            });
        }

    }
    //擴充方法
    public static class Extension
    {
        //非同步委派更新UI
        public static void InvokeIfRequired(
            this Control control, MethodInvoker action)
        {
            if (control.InvokeRequired)//在非當前執行緒內 使用委派
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }

   
}
