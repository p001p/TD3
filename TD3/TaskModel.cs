using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD3
{
    public class TaskModel
    {
        public string? Name { get; set; }    // Описание задачи
        public string? Person { get; set; }  // Ответственный
        public int TaskID { get; set; }
        public override string ToString() => $"{Person}: {Name}";
    }
}
