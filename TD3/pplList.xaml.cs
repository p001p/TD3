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

namespace TD3
{
    /// <summary>
    /// Логика взаимодействия для pplList.xaml
    /// </summary>
    public partial class pplList : Window
    {
        public pplList()
        {
            InitializeComponent();
            LoadPeopleFromBD(); // загрузка списка людей из БД
        }

        private void newPpl (object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(newPplName.Text))
            {
                string newPplNameDB = newPplName.Text;
                pplListBD.Items.Add(newPplName.Text);
                newPplName.Clear();
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                    {
                        conn.Open();

                        var cmd = new SQLiteCommand("INSERT INTO pplList (People) VALUES (@people)", conn);
                        cmd.Parameters.AddWithValue("@people", newPplNameDB);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении проекта в базу данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        } //Добавляет проект

        private void delPpl(object sender, RoutedEventArgs e)
        {


            if (pplListBD.SelectedItem != null)

            {
                string? newPplNameDB = pplListBD.SelectedItem.ToString();
                pplListBD.Items.Remove(pplListBD.SelectedItem);

                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={dbPass.CurrentDbPath}"))
                    {
                        conn.Open();

                        var cmd = new SQLiteCommand("DELETE FROM pplList WHERE People = @people", conn);
                        cmd.Parameters.AddWithValue("@people", newPplNameDB);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении из базы:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        } //Удаляем проект

        private void LoadPeopleFromBD()
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
                        pplListBD.Items.Add(reader["People"].ToString());
                    }

                    if (pplListBD.Items.Count > 0)
                        pplListBD.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке исполнителей:\n" + ex.Message);
            }
        }

    }
}
