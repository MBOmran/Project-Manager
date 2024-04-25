using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace TelerikWinFormsApp1
{
    public partial class RadForm1 : Telerik.WinControls.UI.RadForm
    {
        List<Project> projects = new List<Project>();
        List<Employee> employees = new List<Employee>();
        public RadForm1()
        {
            InitializeComponent();
            this.radDataLayout1.ItemDefaultHeight = 26;
            this.radDataLayout1.ColumnCount = 2;
            this.radDataLayout1.FlowDirection = FlowDirection.TopDown;
            this.radDataLayout1.AutoSizeLabels = true;
            this.radDataLayout2.ItemDefaultHeight = 26;
            this.radDataLayout2.ColumnCount = 2;
            this.radDataLayout2.FlowDirection = FlowDirection.TopDown;
            this.radDataLayout2.AutoSizeLabels = true;
            employees.Add(new Employee()
            {
                FirstName = "Sarah",
                LastName = "Blake",
                Occupation = "Supplied Manager",
                StartingDate = new DateTime(2005, 04, 12),
                IsMarried = true
            });

            this.bindingSource2.DataSource = employees;
            this.bindingSource1.DataSource = projects;
            this.radDataLayout1.DataSource = this.bindingSource1;
            this.radBindingNavigator1.BindingSource = this.bindingSource1;
            this.radDataLayout2.DataSource = this.bindingSource2;
            this.radBindingNavigator2.BindingSource = this.bindingSource2;
        }
        public class Employee
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Occupation { get; set; }
            public DateTime StartingDate { get; set; }
            public bool IsMarried { get; set; }
        }
                

        private void radButton3_Click(object sender, EventArgs e)
        {
            SavePropertiesToFile(projects);
        }
        public static void SavePropertiesToFile<T>(List<T> list) where T : class
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            string className = typeof(T).Name;
            string filePath = $"{className}.txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                var properties = typeof(T).GetProperties();
                writer.WriteLine(string.Join(",", properties.Select(p => p.Name)));

                foreach (var item in list)
                {
                    string line = string.Join(",", properties.Select(p => p.GetValue(item)?.ToString()));
                    writer.WriteLine(line);
                }
            }

        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            LoadPropertiesFromFile(projects);
        }
        public static void LoadPropertiesFromFile<T>(List<T> list) where T : class, new()
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            string className = typeof(T).Name;
            string filePath = $"{className}.txt";

            if (!File.Exists(filePath))
            {
                return; // No action if file doesn't exist
            }
            list.Clear();
            using (StreamReader reader = new StreamReader(filePath))
            {
                // Read header line with property names
                string headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    list.Clear();
                    return; // Empty list if file is empty
                }

                string[] propertyNames = headerLine.Split(',');
                
                // Read data lines
                while (!reader.EndOfStream)
                {
                    string dataLine = reader.ReadLine();
                    if (dataLine == null)
                    {
                        break;
                    }

                    // Split data values
                    string[] dataValues = dataLine.Split(',');

                    // Create new instance of T for each data line
                    T item = new T();

                    // Assign values to properties
                    for (int i = 0; i < propertyNames.Length; i++)
                    {
                        string propertyName = propertyNames[i];
                        string value = dataValues[i];

                        // Get property by name
                        var property = typeof(T).GetProperty(propertyName);

                        if (property != null && property.CanWrite)
                        {
                            object parsedValue;
                            if (property.PropertyType == typeof(int))
                            {
                                parsedValue = int.Parse(value);
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                parsedValue = value;
                            }
                            else if (property.PropertyType == typeof(bool))
                            {
                                parsedValue = bool.Parse(value);
                            }
                            else if (property.PropertyType == typeof(DateTime))
                            {
                                parsedValue = DateTime.Parse(value);
                            }
                            else
                            {
                                continue; 
                            }

                            property.SetValue(item, parsedValue);
                        }
                    }
                    list.Add(item);
                }
            }
        }

        private void radButton4_Click(object sender, EventArgs e)
        {
            SavePropertiesToFile(employees);

        }

        private void radButton6_Click(object sender, EventArgs e)
        {
            LoadPropertiesFromFile(employees);

        }
    }
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string PM { get; set; }
        public int Budget { get; set; }
        public string Team { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        }

    }
