﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JConsole
{
    public class DocumentationAttribute : Attribute
    {
        public string Summary { get; set; }

        public DocumentationAttribute(string summary)
        {
            Summary = summary;
        }
    }
}
