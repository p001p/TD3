using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data.SQLite;
using System.IO;
using Ookii.Dialogs.Wpf;
using System.Net.NetworkInformation;

namespace TD3
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        //Добавляем проект
        private void addNewProject(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(scr1NameProject.Text))
            {
                string projectName = scr1NameProject.Text;
                projectBox.Items.Add(new ProjectModel { Project = projectName});
                scr1NameProject.Clear();

                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                    {
                        conn.Open();

                        var cmd = new SQLiteCommand("INSERT INTO Projects (Project) VALUES (@project)", conn);
                        cmd.Parameters.AddWithValue("@project", projectName);
                        cmd.ExecuteNonQuery();
                    }

                    //Добавляем объект ProjectModel в ListBox

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении проекта в базу данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        } //Добавляем проект

        //Удаляем проект    
        private void delProject(object sender, RoutedEventArgs e)
        {
            

            if (projectBox.SelectedItem != null)
                
            {
                string? projectName = projectBox.SelectedItem.ToString();
                projectBox.Items.Remove(projectBox.SelectedItem);

                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                    {
                        conn.Open();

                        var cmd = new SQLiteCommand("DELETE FROM Projects WHERE Project = @project", conn);
                        cmd.Parameters.AddWithValue("@project", projectName);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении из базы:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        } //Удаляем проект

        //Создаем новую базу данных
        private void CreateDb_Click(object sender, RoutedEventArgs e)
        {
            //Открываем окно запроса указать папку под файлы DB
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Выберите папку, где будет создана база данных",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.SelectedPath;

                // Строим пути
                string dbaseFolder = System.IO.Path.Combine(selectedPath, "DBase");
                string dbFilePath = System.IO.Path.Combine(dbaseFolder, "projectdata.db");
                string dbFilesFolder = System.IO.Path.Combine(dbaseFolder, "DBFiles");

                try
                {
                    // Создаём папки
                    Directory.CreateDirectory(dbaseFolder);
                    Directory.CreateDirectory(dbFilesFolder);

                    // Создаём файл базы данных, если его ещё нет
                    if (!File.Exists(dbFilePath))
                    {
                        SQLiteConnection.CreateFile(dbFilePath);
                    }

                    using (var conn = new SQLiteConnection($"Data Source={dbFilePath}"))
                    {
                        conn.Open();

                        // Включаем поддержку внешних ключей
                        var pragma = new SQLiteCommand("PRAGMA foreign_keys = ON;", conn);
                        pragma.ExecuteNonQuery();

                        // Создание таблицы Projects
                        var createProjects = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Projects (
                     ProjectID INTEGER PRIMARY KEY AUTOINCREMENT,
                     Project TEXT NOT NULL
                    );", conn);
                        createProjects.ExecuteNonQuery();

                        // Создание таблицы pplList
                        var createPplList = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS pplList (
                     ID INTEGER PRIMARY KEY AUTOINCREMENT,
                     People TEXT NOT NULL
                    );", conn);
                        createPplList.ExecuteNonQuery();

                        // Создание таблицы Tasks
                        var createTasks = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Tasks (
                     TasksID INTEGER PRIMARY KEY AUTOINCREMENT,
                     Name TEXT NOT NULL,
                     Person TEXT NOT NULL,
                     ProjectID INTEGER NOT NULL,
                        FOREIGN KEY (ProjectID) REFERENCES Projects(ProjectID) ON DELETE CASCADE
                    );", conn);
                        createTasks.ExecuteNonQuery();

                        // Создание таблицы Reports
                        var createReports = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Reports (
                         ReportID INTEGER PRIMARY KEY AUTOINCREMENT,
                         Date TEXT,
                         N1 TEXT, N2 TEXT, N3 TEXT, N4 TEXT, N5 TEXT,
                         N6 TEXT, N7 TEXT, N8 TEXT, N9 TEXT, N10 TEXT,
                         D1 TEXT, D2 TEXT, D3 TEXT, D4 TEXT, D5 TEXT,
                         D6 TEXT, D7 TEXT, D8 TEXT, D9 TEXT, D10 TEXT,
                         Discrb TEXT,
                         Problems TEXT,
                         File1 TEXT, File2 TEXT, File3 TEXT, File4 TEXT, File5 TEXT,
                         File6 TEXT, File7 TEXT, File8 TEXT, File9 TEXT, File10 TEXT,
                         TaskID INTEGER NOT NULL,
                        FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE
                    );", conn);
                        createReports.ExecuteNonQuery();

                        // Сохраняем путь для дальнейшей работы
                        dbPass.CurrentDbPath = dbFilePath;

                        MessageBox.Show("База данных успешно создана и подключена ✅", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании базы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        } //Создаем новую базу данных

        //Считываем из вновь открытой базы список проектов
        private void OpenDb_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "SQLite database (*.db)|*.db|Все файлы (*.*)|*.*";
            openDialog.Title = "Выберите файл базы данных";

            if (openDialog.ShowDialog() == true)
            {
                string dbFilePath = openDialog.FileName;

                if (!File.Exists(dbFilePath))
                {
                    MessageBox.Show("Файл базы данных не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Сохраняем путь в AppState (если используешь)
                    dbPass.CurrentDbPath = dbFilePath;

                    // Подключаемся и читаем Projects
                    using (var conn = new SQLiteConnection($"Data Source={dbFilePath}"))
                    {
                        conn.Open();

                        var cmd = new SQLiteCommand("SELECT ProjectID, Project FROM Projects", conn);
                        var reader = cmd.ExecuteReader();

                        projectBox.Items.Clear();

                        while (reader.Read())
                        {
                            var project1 = new ProjectModel
                            {
                                ProjectID = Convert.ToInt32(reader["ProjectID"]),
                                Project = reader["Project"].ToString()
                            };
                           
                            projectBox.Items.Add(project1);
                        }
                    }

                    MessageBox.Show("База подключена. Проекты загружены ✅", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении базы:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        } //Считываем из вновь открытой базы список проектов

        //Открываем новое окно
        private void MDC_OpenProj(object sender, RoutedEventArgs e)
        { 
        if (projectBox.SelectedItem is ProjectModel selectedProject)
            {
                MessageBox.Show("Открываем окно...");
                var taskWindow = new TaskWindow(selectedProject.ProjectID, selectedProject.Project);
                taskWindow.ShowDialog(); // можно и Show() если не модально
            }
        } //Открываем новое окно

        //Открываем новое окно pplList
        private void openPpl (object sender, RoutedEventArgs e)
        {
                var newOpenPPl = new pplList();
                newOpenPPl.ShowDialog(); // можно и Show() если не модально
            
        }
    }
}