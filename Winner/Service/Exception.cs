using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winner
{
    class UIException : Exception
    {        
        public int errorCode { get; set; }

        public UIException(int errorCode) : this(errorCode, "asdfsaf") {}

        public UIException(int errorCode, string message) : base(message)
        {
            this.errorCode = errorCode;
        }
    }
}
