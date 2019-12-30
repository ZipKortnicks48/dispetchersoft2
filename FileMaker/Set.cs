using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniFiles;
using System.Windows.Forms;

namespace FileMaker
{
    public partial class Set : Form
    {
        private static string path = "set.ini";
        IniFile INI = new IniFile(path);
        
        public Set()
        {
            InitializeComponent();
            
            
            if (INI.KeyExists("path", "set"))
                textBox2.Text = INI.ReadINI("set", "distinct");
            if (INI.KeyExists("distinct", "set"))
                textBox3.Text = INI.ReadINI("set", "path");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkPassword(textBox1.Text);
        }
        private void checkPassword(string p)
        {
            if (p == "Djljrfyfk48")
            {
                textBox1.Enabled = false;
                label2.Visible = true;
                label3.Visible = true;
                textBox2.Visible = true;
                textBox3.Visible = true;
                button2.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            INI.Write("set", "path", textBox3.Text);
            INI.Write("set", "distinct", textBox2.Text);
            MessageBox.Show("Настройки сохранены. Для их вступления в силу перезагрузите программу.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); // Говорим пользователю, что сохранили текст.
        }
    }
}
