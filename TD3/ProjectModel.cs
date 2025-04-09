using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD3
{
    public class ProjectModel
    {
        public int ProjectID { get; set; }
        public string? Project { get; set; }

        public override string ToString()
        {
            return Project; // чтобы в ListBox отображалось красиво
        }
    }
}
