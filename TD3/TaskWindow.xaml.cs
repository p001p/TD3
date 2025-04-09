using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace TD3
{
    /// <summary>
    /// Логика взаимодействия для TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        private int _projectId;
        private ObservableCollection<TaskModel> taskList1 = new ObservableCollection<TaskModel>();

        //Инициализатор
        public TaskWindow(int projectId, string projectName)
        {
            InitializeComponent();
            _projectId = projectId; //Сохраняем полученный ID проекта в переменную
            this.Title = $"Задачи проекта: {projectName} и ID: {_projectId}"; //Записываем значение переменной как название шапки окна
            LoadTasks(_projectId); //Загружаем все таски с полученным ID
        }

        //Если создаем новую задачу
        private void openWindowAddTask(object sender, RoutedEventArgs e)
        {
            int zeroTaskID = 0;
            var addTaskWindow = new AddTaskWindow(zeroTaskID); //создаем экземпляр окна
            if (addTaskWindow.ShowDialog() == true)  //открываем окно и ждем когда нажмут ОК
            {

                //Получаем данные из модального окна
                var task = new TaskModel   //создаем экземпляр класса как набор данных
                {
                    Person = addTaskWindow.choisedPerson,   //присваиваем значения переменным класса из открытого окна, в случае если TRUE
                    Name = addTaskWindow.choisedTask        //присваиваем значения переменным класса из открытого окна, в случае если TRUE
                };

                //Добавляем в ObservableCollection, которая связана с ListBox
                taskList1.Add(task);

                // Сохраняем в SQLite
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                    {
                        conn.Open();

                        var cmd = new SQLiteCommand("INSERT INTO Tasks (Name, Person, ProjectID) VALUES (@name, @person, @pid)", conn);
                        cmd.Parameters.AddWithValue("@name", task.Name);
                        cmd.Parameters.AddWithValue("@person", task.Person);
                        cmd.Parameters.AddWithValue("@pid", _projectId); // у тебя должен быть ProjectID

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении в БД: " + ex.Message);
                }



            }
        }

        //Делаем активной кнопку редактирования
        private void taskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editTaskButton.IsEnabled = taskListBox.SelectedItem != null;
        }

        //Если редактируем существующую
        private void openEditWindowAddTask(object sender, RoutedEventArgs e)
        {
            if (taskListBox.SelectedItem is TaskModel selectedTask)
            {
                int taskEditId = selectedTask.TaskID;

                var addEditTaskWindow = new AddTaskWindow(taskEditId); //создаем экземпляр окна
                if (addEditTaskWindow.ShowDialog() == true)  //открываем окно и ждем когда нажмут ОК
                {

                    //Получаем данные из модального окна
                    var task = new TaskModel   //создаем экземпляр класса как набор данных
                    {
                        Person = addEditTaskWindow.choisedPerson,   //присваиваем значения переменным класса из открытого окна, в случае если TRUE
                        Name = addEditTaskWindow.choisedTask        //присваиваем значения переменным класса из открытого окна, в случае если TRUE
                    };

                    //Добавляем в ObservableCollection, которая связана с ListBox
                    taskList1.Add(task);

                    // Сохраняем в SQLite
                    try
                    {
                        using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                        {
                            conn.Open();

                            var cmd = new SQLiteCommand("UPDATE Tasks SET Name = @name, Person = @person WHERE TasksID = @tid", conn);
                            cmd.Parameters.AddWithValue("@name", task.Name);
                            cmd.Parameters.AddWithValue("@person", task.Person);
                            cmd.Parameters.AddWithValue("@tid", taskEditId); // у тебя должен быть TaskID

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении в БД: " + ex.Message);
                    }

                    LoadTasks(_projectId);

                }



            }
        }


        //Загружаем список в ListBox из базы данных
        private void LoadTasks(int _projectId)
        {
            //var taskList1 = new List<TaskModel>(); //Создаем список List, который может хранить объекты типа TaskModel, и этот список называется taskList1. Т.е. это коллекция которая может хранить много объектов типа TaskModel
            taskList1.Clear();

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}")) //коннектор к бд
                {
                    conn.Open();

                    //Запрос
                    var cmd = new SQLiteCommand("SELECT TasksID, Name, Person FROM Tasks WHERE ProjectID = @pid", conn);
                    cmd.Parameters.AddWithValue("@pid", _projectId);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        taskList1.Add(new TaskModel
                        {
                            Name = reader["Name"].ToString(),
                            Person = reader["Person"].ToString(),
                            TaskID = Convert.ToInt32(reader["TasksID"])
                        });
                    }
                }

                // Привязка к ListBox
                        
                taskListBox.ItemsSource = taskList1;    // задаём обновлённый список
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач из базы:\n" + ex.Message);
            }
        }

        //Удаляем из списка
        private void deleteTask(object sender, RoutedEventArgs e)
        {
            var selectedTask = taskListBox.SelectedItem as TaskModel;

            if (selectedTask != null)
            {
                taskList1.Remove(selectedTask);
                     
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                    {
                        conn.Open();

                        var cmd = new SQLiteCommand("DELETE FROM Tasks WHERE Name = @name AND Person = @person AND ProjectID = @pid", conn);
                        cmd.Parameters.AddWithValue("@name", selectedTask.Name);
                        cmd.Parameters.AddWithValue("@person", selectedTask.Person);
                        cmd.Parameters.AddWithValue("@pid", _projectId);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении из базы:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        } //Удаляем проект





    }
}
