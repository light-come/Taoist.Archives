using System;
using System.Collections.Generic;
using System.Text;

namespace Taoist.Archives.shared
{
    public class IO
    {
        public class IActionResult {
            public class Ok
            {
                public string code { get; set; } = "0";
                public object data { get; set; }
                public string msg { get; set; } = "success";
            }
            public class data
            {
                public int count { get; set; }
                public long size { get; set; }
            }
        }
    }
}
