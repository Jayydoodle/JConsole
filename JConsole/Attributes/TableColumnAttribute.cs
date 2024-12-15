using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JConsole
{
    public class TableColumnAttribute : Attribute
    {
        public int DisplayOrder { get; set; }

        public TableColumnAttribute() { }

        public TableColumnAttribute(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }
    }
}
