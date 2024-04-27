using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;

namespace TelerikWinFormsApp1
{
    public partial class Register : Telerik.WinControls.UI.RadForm
    {
        
        public FileStream File;
        public static string path = "UserData.txt";
        public static StreamWriter sw;
        public static StreamReader sr;
        public Register()
        {
            InitializeComponent();
            File = new FileStream(path, FileMode.Append, FileAccess.ReadWrite);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (tbusername.Text == "")
            {
                RadMessageBox.Show("UserName Can't Be Empty!", "UserName Required", MessageBoxButtons.OK, RadMessageIcon.Exclamation);
                return;
            }
            sw = new StreamWriter(File);
            sw.WriteLine(tbusername);
        }
        private bool CheckUserName()
        {
            //loop through all user names to see if it's already added
            return true;
        }
        private void basicEncryption(string s)
        {
            char[] editedstring = s.ToCharArray();
            for (int i = 0; i < s.Length; i++)
            {
                int x = (int)s[i];
                if (i % 2 == 0)
                {
                    x += 13;
                }
                else
                {
                    x += 5;
                }
                editedstring[i] = (char)x;
            }
        }
    }
}
