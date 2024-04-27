using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
namespace TelerikWinFormsApp1
{
    public partial class Register : Telerik.WinControls.UI.RadForm
    {
        public static string path = "UserData.txt";
        public static StreamWriter sw;
        public static StreamReader sr;
        public static FileStream MyFile;

        public Register()
        {
            InitializeComponent();
        }

        private void SaveUserData(string username, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                RadMessageBox.Show("Username cannot be empty!", "Username Required", MessageBoxButtons.OK, RadMessageIcon.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                RadMessageBox.Show("Password must be at least 8 characters long!", "Invalid Password", MessageBoxButtons.OK, RadMessageIcon.Exclamation);
                return;
            }

            if (password != confirmPassword)
            {
                RadMessageBox.Show("Passwords do not match!", "Password Mismatch", MessageBoxButtons.OK, RadMessageIcon.Exclamation);
                return;
            }

            string encryptedPassword = Encrypt(password);

            // Save user data to the file
            using (StreamWriter writer = new StreamWriter("UserData.txt", true))
            {
                writer.WriteLine($"{username}|{encryptedPassword}");
            }

            RadMessageBox.Show("User data saved successfully!", "Save Data", MessageBoxButtons.OK, RadMessageIcon.Info);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = tbusername.Text.Trim();
            string password = tbpassword.Text;
            string confirmPassword = tbConfirmPassword.Text;

            if (CheckUserName(username))
            {
                RadMessageBox.Show("Username already exists! Please choose a different username.", "Username Exists", MessageBoxButtons.OK, RadMessageIcon.Error);
                return;
            }

            SaveUserData(username, password, confirmPassword);
        }
        private bool CheckUserName(string username)
        {
            if (File.Exists("UserData.txt"))
            {
                sr = new StreamReader("UserData.txt");
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length > 0 && parts[0] == username)
                    {
                        sr.Close(); // Close the StreamReader after reading
                        return true; // Username found
                    }
                }
                sr.Close(); // Close the StreamReader after reading
            }
            return false; // Username not found
        }

        private string Encrypt(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            // Simple XOR encryption
            byte key = 42;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ key);
            }

            return Convert.ToBase64String(bytes);
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            tbusername.Text = "";
            tbpassword.Text = "";
            tbConfirmPassword.Text = "";
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            //Simply Toggle password visibility based on the state of the checkbox
            tbpassword.UseSystemPasswordChar = chkShowPassword.Checked;
            tbConfirmPassword.UseSystemPasswordChar = chkShowPassword.Checked;
        }
        private void labelLogin_Click(object sender, EventArgs e)
        {
            Login L = new Login();
            this.Hide();
            L.Show();
        }

        private void Register_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
