using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD3
{
    internal class dbPass
    {
        /// <summary>
        /// Путь к текущей SQLite базе данных
        /// </summary>
        public static string? CurrentDbPath { get; set; }

        /// <summary>
        /// Указывает, подключено ли приложение к базе данных
        /// </summary>
        public static bool IsConnected => !string.IsNullOrWhiteSpace(CurrentDbPath);
    }
}
