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
    public partial class Login : Telerik.WinControls.UI.RadForm
    {
        public Login()
        {
            InitializeComponent();

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = tbusername.Text.Trim();
            string password = tbpassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                RadMessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButtons.OK, RadMessageIcon.Error);
                return;
            }

            if (CheckUserExists(username, password))
            {
                // Successful login, redirect to main application or perform other actions
                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Redirect to main application
                RadForm1 mainForm = new RadForm1();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                RadMessageBox.Show("Invalid username or password.", "Login Error", MessageBoxButtons.OK, RadMessageIcon.Error);
            }
        }

        private bool CheckUserExists(string username, string password)
        {
            // Read user data from file and check if username and password match
            if (File.Exists("UserData.txt"))
            {
                using (StreamReader reader = new StreamReader("UserData.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length == 2 && parts[0] == username && Decrypt(parts[1]) == password)
                        {
                            return true; // User found
                        }
                    }
                }
            }
            return false; // User not found
        }

        private string Decrypt(string encryptedPassword)
        {
            byte[] bytes = Convert.FromBase64String(encryptedPassword);

            // Simple XOR decryption used in the register form too
            byte key = 42;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ key);
            }

            return Encoding.UTF8.GetString(bytes);
        }


        private void btClear_Click(object sender, EventArgs e)
        {
            tbusername.Text = "";
            tbpassword.Text = "";
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            //Simply Toggle password visibility based on the state of the checkbox
            tbpassword.UseSystemPasswordChar = chkShowPassword.Checked;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            Register reg = new Register();
            reg.Show();
            this.Hide();
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
