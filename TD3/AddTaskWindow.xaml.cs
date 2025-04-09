using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
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
using System.Xml.Linq;

namespace TD3
{
    /// <summary>
    /// Логика взаимодействия для AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        public string? choisedPerson { get; private set; }
        public string? choisedTask { get; private set; }

        private int numberID;

        public AddTaskWindow(int _taskID) 
        {
            InitializeComponent();
            numberID = _taskID;
            LoadPplListToChoise(); //Запускаем чтение списка людей
            if (numberID != 0)
            {
                LoadDataFromDB();
            }
        } //Инициализатор

        //Кнопка открытия окна добавления участников
        private void addPplFromTask(object sender, RoutedEventArgs e) 
        {

            var addNewOpenPPl = new pplList();
            addNewOpenPPl.Owner = this;
            

            if (addNewOpenPPl.ShowDialog() == true)
            {
                // после закрытия окна — обновляем ListBox
                LoadPplListToChoise(); // ← это твой метод, который загружает задачи из БД в ListBox
            }

        } //Кнопка открытия окна добавления участников

        //Метод автоматической загрузки окна с пользователями, при первом открытии окна добавления/редактирования. Просто подгружает listbox.
        private void LoadPplListToChoise()
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                {
                    conn.Open();

                    var cmd = new SQLiteCommand("SELECT DISTINCT People FROM pplList", conn);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        pplToChoise.Items.Add(reader["People"].ToString());
                    }

                    if (pplToChoise.Items.Count > 0)
                        pplToChoise.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке исполнителей:\n" + ex.Message);
            }
        }

        //Загружаем если у нас редактирование
        private void LoadDataFromDB()
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}")) //коннектор к бд
                {
                    conn.Open();

                    //Запрос
                    var cmd = new SQLiteCommand("SELECT Name, Person FROM Tasks WHERE TasksID = @pid", conn);
                    cmd.Parameters.AddWithValue("@pid", numberID);

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string? readingPerson = reader["Person"].ToString();
                        whoChoised.Text = $"{readingPerson}";
                        string? readingName = reader["Name"].ToString();
                        nameTask.Text = $"{readingName}";
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке значения из базы:\n" + ex.Message);
            }
        }


        private void setName(object sender, RoutedEventArgs e)
        {
            if (pplToChoise.SelectedItems != null)
                {
                    whoChoised.Text = pplToChoise.SelectedItem.ToString();
                }
        }

        private void addTaskToKistAndDB(object sender, RoutedEventArgs e)
        {
            choisedPerson = whoChoised.Text.Trim();
            choisedTask = nameTask.Text.Trim();

            if (string.IsNullOrWhiteSpace(choisedPerson) || string.IsNullOrWhiteSpace(choisedTask))
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            DialogResult = true;
            Close();

        }
    }
}
