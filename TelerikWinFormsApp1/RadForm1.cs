using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace TelerikWinFormsApp1
{
    public partial class RadForm1 : Telerik.WinControls.UI.RadForm
    {
        List<Project> projects = new List<Project>();
        public static ArrayList TasksList = new ArrayList();
        Dictionary<int, Task> taskDictionary = new Dictionary<int, Task>();
        public FileStream mystream;
        public StreamReader sr;
        public StreamWriter sw;
        private static int taskIdCounter;
        public RadForm1()
        {
            InitializeComponent();
            InitializeDataLayouts();
            LoadDataFromFile();
            autocompleteid();
            taskIdCounter = LoadTaskIdCounter();
            radTextBox1.AutoCompleteCustomSource.AddRange(new string[] { "Task 1", "Task 2", "Task 3" });
            radTextBox1.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            radTextBox2.AutoCompleteCustomSource.AddRange(new string[] { "1", "2", "3" });
            radTextBox2.AutoCompleteCustomSource = new AutoCompleteStringCollection();
        }

        private void InitializeDataLayouts()
        {

            this.radDataLayout1.ItemDefaultHeight = 26;
            this.radDataLayout1.ColumnCount = 2;
            this.radDataLayout1.FlowDirection = FlowDirection.TopDown;
            this.radDataLayout1.AutoSizeLabels = true;
            this.bindingSource1.DataSource = projects;
            this.radDataLayout1.DataSource = this.bindingSource1;
            this.radBindingNavigator1.BindingSource = this.bindingSource1;



        }
        #region Scheduler
        public void SaveSchedulerData(RadScheduler scheduler)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Subject");
            dataTable.Columns.Add("Start");
            dataTable.Columns.Add("End");
            dataTable.Columns.Add("Location");
            dataTable.Columns.Add("Reminder");
            dataTable.Columns.Add("backgroundid");
            dataTable.Columns.Add("statusid");
            dataTable.Columns.Add("Description");

            foreach (Appointment appointment in scheduler.Appointments)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["Subject"] = appointment.Subject;
                dataRow["Start"] = appointment.Start.ToString();
                dataRow["End"] = appointment.End.ToString();
                dataRow["Location"] = appointment.Location;
                dataRow["Reminder"] = appointment.Reminder.ToString();
                dataRow["backgroundid"] = appointment.BackgroundId.ToString();
                dataRow["statusid"] = appointment.StatusId.ToString();
                dataRow["Description"] = appointment.Description;

                dataTable.Rows.Add(dataRow);
            }
            string filePath = "scheduler_data.csv";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    writer.Write(dataTable.Columns[i].ColumnName + ",");
                }
                writer.WriteLine();

                foreach (DataRow row in dataTable.Rows)
                {
                    string dataRow = string.Join(",", row.ItemArray);
                    writer.WriteLine(dataRow);
                }
            }

        }

        //Loading Scheduler
        public static void LoadingAppointments(RadScheduler scheduler1, string filePath)
        {
            DateTime appointmentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 0, 0);

            scheduler1.Appointments.BeginUpdate();


            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                string[] headers = null; // Initialize to null

                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        headers = line.Split(','); // Read and store headers only once
                        continue;
                    }

                    string[] values = line.Split(',');

                    string subject, location, reminder, description = string.Empty;
                    DateTime dtStart, dtEnd;
                    int backgroundId, statusId;



                    int startIndex = Array.IndexOf(headers, "Start");
                    int endIndex = Array.IndexOf(headers, "End");
                    int subjectIndex = Array.IndexOf(headers, "Subject");
                    int locationIndex = Array.IndexOf(headers, "Location");
                    int reminderIndex = Array.IndexOf(headers, "Reminder");
                    int backgroundIdIndex = Array.IndexOf(headers, "backgroundid");
                    int statusIdIndex = Array.IndexOf(headers, "statusid");
                    int descriptionIndex = Array.IndexOf(headers, "Description");
                    dtStart = DateTime.Parse(values[startIndex]);
                    dtEnd = DateTime.Parse(values[endIndex]);
                    subject = values[subjectIndex].Trim();
                    location = values[locationIndex].Trim();
                    reminder = values[reminderIndex].Trim();
                    description = values[descriptionIndex].Trim();
                    if (!int.TryParse(values[backgroundIdIndex], out backgroundId))
                    {
                        backgroundId = 0;
                        Console.WriteLine("Error parsing backgroundId for row: {0}, using default value", line);
                    }

                    if (!int.TryParse(values[statusIdIndex], out statusId))
                    {
                        statusId = 0;
                        Console.WriteLine("Error parsing statusId for row: {0}, using default value", line);
                    }



                    Appointment appointment = new Appointment(dtStart, dtEnd, subject, description);
                    appointment.BackgroundId = backgroundId;
                    appointment.StatusId = statusId;
                    appointment.Location = location;
                    scheduler1.Appointments.Add(appointment);
                }
            }

            scheduler1.Appointments.EndUpdate();
        }

        #endregion
        #region Projects
        private void LoadDataFromFile()
        {
            LoadPropertiesFromFile(projects);
            LoadPropertiesFromFile(employees);
        }

        private void SaveDataToFile()
        {
            SavePropertiesToFile(projects);
            SavePropertiesToFile(employees);
        }

        private void SavePropertiesToFile<T>(List<T> list) where T : class
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

        private void LoadPropertiesFromFile<T>(List<T> list) where T : class, new()
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
                string headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    list.Clear();
                    return; // Empty list if file is empty
                }

                string[] propertyNames = headerLine.Split(',');

                while (!reader.EndOfStream)
                {
                    string dataLine = reader.ReadLine();
                    if (dataLine == null)
                    {
                        break;
                    }

                    string[] dataValues = dataLine.Split(',');

                    T item = new T();

                    for (int i = 0; i < propertyNames.Length; i++)
                    {
                        string propertyName = propertyNames[i];
                        string value = dataValues[i];

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

        private void radButton3_Click(object sender, EventArgs e)
        {
            SaveDataToFile();
            RadMessageBox.Show("Projects and employees data saved successfully!", "Save Data", MessageBoxButtons.OK, RadMessageIcon.Info);
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            LoadDataFromFile();
            RadMessageBox.Show("Projects and employees data loaded successfully!", "Load Data", MessageBoxButtons.OK, RadMessageIcon.Info);
        }
        #endregion/////////////////////////////////////////////////////////////////////////////////////

        #region ToDoList
        private void SaveTaskIdCounter()
        {
            // Save the task ID counter to a file
            using (StreamWriter writer = new StreamWriter("taskIdCounter.txt"))
            {
                writer.WriteLine(taskIdCounter);
            }
        }

        private int LoadTaskIdCounter()
        {
            int counter = 1; // Default value if file does not exist or cannot be read

            if (File.Exists("taskIdCounter.txt"))
            {
                try
                {
                    counter = int.Parse(File.ReadAllText("taskIdCounter.txt"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading task ID counter: " + ex.Message);
                }
            }
            return counter;
        }

        private void AddTask()
        {
            if (tbTaskName.Text == "")
            {
                RadMessageBox.Show("Enter Task Name!", "Task Name Missing", MessageBoxButtons.OK);
                return;
            }
            if (tbTaskDescription.Text == "")
            {
                RadMessageBox.Show("Enter Task Description!", "Task Description Missing", MessageBoxButtons.OK);
                return;
            }
            if (cmbPriority.Text == "")
            {
                RadMessageBox.Show("Select Task Priority!", "Task Priority Missing", MessageBoxButtons.OK);
                return;
            }
            if (!radioButton1.Checked && !radioButton2.Checked)
            {
                RadMessageBox.Show("Select Task Statues!", "Task Statues Missing", MessageBoxButtons.OK);
                return;
            }

            Task task = new Task();
            task.Name = tbTaskName.Text;
            task.description = tbTaskDescription.Text;
            task.DueDate = dateTimePicker1.Value;
            if (cmbPriority.SelectedIndex == 0)
            {
                task.TaskPriority = ePriority.High;
            }
            else if (cmbPriority.SelectedIndex == 1)
            {
                task.TaskPriority = ePriority.Medium;
            }
            else if (cmbPriority.SelectedIndex == 2)
            {
                task.TaskPriority = ePriority.Low;
            }
            else
            {
                RadMessageBox.Show("Select Task Priority!", "Task Priority Missing", MessageBoxButtons.OK);
                return;
            }
            if (radioButton1.Checked)
            {
                task.statues = eCompletedStatues.Completed;
            }
            else
            {
                task.statues = eCompletedStatues.Not_Completed;
            }

            task.id = taskIdCounter++;
            TasksList.Add(task);
            taskDictionary.Add(task.id, task);
            RadMessageBox.Show("Task Added Succefully(Save To Keep On Device)", "Success", MessageBoxButtons.OK); ;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Save_Tasks_Locally();
            TasksList.Clear();
        }
        private void Save_Tasks_Locally()
        {
            if (TasksList.Count > 0)
            {
                mystream = new FileStream("Tasks.txt", FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(mystream);
                string taskinfo = "";
                foreach (Task task in TasksList)
                {
                    taskinfo = task.id + "|" + task.Name + "|" + task.description + "|" + task.DueDate + "|" + task.TaskPriority + "|" + task.statues;
                    sw.WriteLine(taskinfo);
                }
                sw.Close();
                mystream.Close();
                RadMessageBox.Show("Saved Succefully", "Success", MessageBoxButtons.OK);
                SaveTaskIdCounter();
            }
            else
            {
                RadMessageBox.Show("Task List Empty (Add New Tasks To Save)", "Failed To Save", MessageBoxButtons.OK); ;

            }

        }
        private void LoadAllTasks()
        {
            // Clear existing tasks from TasksList
            TasksList.Clear();
            taskDictionary.Clear();

            string filePath = "Tasks.txt";

            // Check if the file exists
            if (File.Exists(filePath))
            {
                Task task = null;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        task = new Task();
                        string line = reader.ReadLine();
                        string[] taskInfo = line.Split('|');

                        if (taskInfo.Length == 6) // Ensure all fields are present
                        {
                            task.id = int.Parse(taskInfo[0]);
                            task.Name = taskInfo[1];
                            task.description = taskInfo[2];
                            task.DueDate = DateTime.Parse(taskInfo[3]);
                            task.TaskPriority = (ePriority)Enum.Parse(typeof(ePriority), taskInfo[4]);
                            task.statues = (eCompletedStatues)Enum.Parse(typeof(eCompletedStatues), taskInfo[5]);

                            // Add loaded task to TasksList
                            TasksList.Add(task);
                            taskDictionary.Add(task.id, task);
                        }
                        else
                        {
                            RadMessageBox.Show("Error loading tasks. Task format is incorrect.", "Error", MessageBoxButtons.OK, RadMessageIcon.Error);
                            return;
                        }
                    }
                    if (task != null)
                    {
                        textBox3.Visible = true;
                        button6.Visible = true;
                        textBox3.Text = "Last Task Loaded :\r\n" +
                            "===============================\r\n" +
                            "ID: " + task.id + "\r\n" +
                            "===============================\r\n" +
                            "Name: " + task.Name + "\r\n" +
                            "===============================\r\n" +
                            "Description: " + task.description + "\r\n" +
                            "===============================\r\n" +
                            "Due Date: " + task.DueDate.ToString() + "\r\n" +
                            "===============================\r\n" +
                            "Priority: " + task.TaskPriority + "\r\n" +
                            "===============================\r\n" +
                            "Statues: " + task.statues;
                    }



                }

                RadMessageBox.Show("Tasks loaded successfully", "Success", MessageBoxButtons.OK);
            }
            else
            {
                RadMessageBox.Show("No tasks found locally", "No Tasks", MessageBoxButtons.OK);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (TasksList.Count > 0)
            {
                DialogResult res = RadMessageBox.Show("Loading Now Will Erease All Unsaved Tasks, Do You Want To Save First ?", "Tasks Not Saved!!", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    Save_Tasks_Locally();
                }
                else if (res == DialogResult.No)
                {
                    LoadAllTasks();

                }
                else
                {
                    return;
                }

            }
            //if taskslist is empty
            else
            {
                LoadAllTasks();
            }

        }
        //Add Task Button , only adds it to ram.

        private void button1_Click(object sender, EventArgs e)
        {
            AddTask();
            DialogResult res = RadMessageBox.Show("Clear Feilds ? ", "Edit Current Task Or Add New", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                tbTaskDescription.Clear();
                tbTaskName.Clear();
            }
        }
        //display information about last task loaded in the very big box
        private void button6_Click(object sender, EventArgs e)
        {
            textBox3.Clear();
            textBox3.Visible = false;
            button6.Visible = false;
        }
        private void RemoveTaskByName(string taskName)
        {
            if (TasksList.Count == 0)
            {
                RadMessageBox.Show("Task List is empty", "No Tasks", MessageBoxButtons.OK, RadMessageIcon.Exclamation);
                return;
            }

            Task taskToRemove = null;

            foreach (Task task in TasksList)
            {
                if (task.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase))
                {
                    taskToRemove = task;
                    break;
                }
            }

            if (taskToRemove != null)
            {
                TasksList.Remove(taskToRemove);
                UpdateTaskFile(); // Update the task file after removing the task , طريقة تبديل الملف بالكامل
                RadMessageBox.Show($"Task '{taskName}' removed successfully", "Success", MessageBoxButtons.OK, RadMessageIcon.Info);
            }
            else
            {
                RadMessageBox.Show($"Task '{taskName}' not found", "Task Not Found", MessageBoxButtons.OK, RadMessageIcon.Exclamation);
            }
        }

        private void UpdateTaskFile()
        {
            mystream = new FileStream("Tasks.txt", FileMode.Create, FileAccess.Write); // Open the file in Create mode to overwrite existing data
            sw = new StreamWriter(mystream);

            foreach (Task task in TasksList)
            {
                string taskinfo = $"{task.id}|{task.Name}|{task.description}|{task.DueDate}|{task.TaskPriority}|{task.statues}";
                sw.WriteLine(taskinfo);
            }

            sw.Close();
            mystream.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (tbTaskName.Text == "")
            {
                RadMessageBox.Show("Enter Task Name For Removal!", "Name Empty!", MessageBoxButtons.OK, RadMessageIcon.Exclamation);
                return;
            }
            string taskname = tbTaskName.Text;
            RemoveTaskByName(taskname);
        }
        private List<Task> FindTasksByName(string name)
        {
            List<Task> foundTasks = new List<Task>();

            foreach (Task task in TasksList)
            {
                if (task.Name.Contains(name))
                {
                    foundTasks.Add(task);
                }
            }

            return foundTasks;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string searchName = radTextBox1.Text;
            List<Task> foundTasks = FindTasksByName(searchName);
            // Display foundTasks data as needed
            foreach (Task task in foundTasks)
            {
                // Display task data, for example:
                MessageBox.Show($"Task Name: {task.Name}\nDescription: {task.description}\nDue Date: {task.DueDate}\nPriority: {task.TaskPriority}\nStatus: {task.statues}");
            }
        }
        //auto complete... name
        private void radTextBox1_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = radTextBox1.Text;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                radTextBox1.AutoCompleteCustomSource.Clear();
                foreach (Task task in TasksList)
                {
                    if (task.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        radTextBox1.AutoCompleteCustomSource.Add(task.Name);
                    }
                }
            }
        }
        private void autocompleteid()
        {
            string filePath = "Tasks.txt";

            // Check if the file exists
            if (File.Exists(filePath))
            {
                Task task = null;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        task = new Task();
                        string line = reader.ReadLine();
                        string[] taskInfo = line.Split('|');

                        if (taskInfo.Length == 6) // Ensure all fields are present
                        {
                            task.id = int.Parse(taskInfo[0]);
                            task.Name = taskInfo[1];
                            task.description = taskInfo[2];
                            task.DueDate = DateTime.Parse(taskInfo[3]);
                            task.TaskPriority = (ePriority)Enum.Parse(typeof(ePriority), taskInfo[4]);
                            task.statues = (eCompletedStatues)Enum.Parse(typeof(eCompletedStatues), taskInfo[5]);

                            // Add loaded task to dictionary
                            taskDictionary.Add(task.id, task);
                        }
                        else
                        {
                            RadMessageBox.Show("Error loading tasks. Task format is incorrect.", "Error", MessageBoxButtons.OK, RadMessageIcon.Error);
                            return;
                        }
                    }
                }

                foreach (int id in taskDictionary.Keys)
                {
                    radTextBox2.AutoCompleteCustomSource.Add(id.ToString());
                }
            }
        }

        private Task FindById(int id)
        {
            // Check if the dictionary contains the task with the given ID
            if (taskDictionary.ContainsKey(id))
            {
                // Return the task with the given ID
                return taskDictionary[id];
            }
            else
            {
                return null;
            }
        }
        //we could have saved the indecies with the dectionary , but i had already implemented the load all tasks
        //so idecided to get the tasks to the dec from ram

        private void button7_Click(object sender, EventArgs e)
        {
            string input = radTextBox2.Text;
            if (!string.IsNullOrEmpty(input))
            {
                if (int.TryParse(input, out int id))
                {
                    // Find the task by ID
                    Task task = FindById(id);
                    if (task != null)
                    {
                        // Display the task details
                        MessageBox.Show($"Task Name: {task.Name}\nDescription: {task.description}\nDue Date: {task.DueDate}\nPriority: {task.TaskPriority}\nStatus: {task.statues}");
                    }
                    else
                    {
                        MessageBox.Show("Task not found!");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid ID!");
                }
            }
        }

        private void radTextBox2_TextChanged(object sender, EventArgs e)
        {
            string input = radTextBox2.Text.ToLower();
            // Clear the existing suggestions
            radTextBox2.AutoCompleteCustomSource.Clear();

            foreach (string suggestion in taskDictionary.Keys.Select(id => id.ToString()))
            {
                if (suggestion.StartsWith(input))
                {
                    radTextBox2.AutoCompleteCustomSource.Add(suggestion);
                }
            }
        }
    }
#endregion
    #region Projects Data
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
    #endregion

    #region ToDoList Data
    public class Task
    {
        public string Name = "", description = "";
        public DateTime DueDate;
        public ePriority TaskPriority;
        public eCompletedStatues statues;
        public int id;
    }
    // an enum for task priorty 
    public enum ePriority
    {
        High,
        Medium,
        Low
    }
    // an enum for statues of whether or not a task has been completed 
    public enum eCompletedStatues
    {
        Completed,
        Not_Completed
    }
}
#endregion