using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.code_inspect.core
{
    public class StreamEventArgs : EventArgs
    {

        public StreamEventArgs() : this(string.Empty)
        {

        }

        public StreamEventArgs(string? message)
        {
            Message = message;
        }
        public string? Message { get; set; }
    }
}
