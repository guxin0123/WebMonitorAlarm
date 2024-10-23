using System;
using System.Windows.Forms;

namespace WebMonitorAlarm
{
    public partial class ConfigKey : Form
    {
        public ConfigKey()
        {
            InitializeComponent();
           
        }
        private void ConfigKey_Load(object sender, EventArgs e)
        {
            string oldKey = RegistryHelper.GetKeyValue("SendUrl");
            textBox1.Text = oldKey;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "") {
                //ConfigKeyStr = textBox1.Text;
                RegistryHelper.AddKey("SendUrl", textBox1.Text);
                DialogResult = DialogResult.OK;
                this.Dispose();
            }
           
            
        }
    }
}
