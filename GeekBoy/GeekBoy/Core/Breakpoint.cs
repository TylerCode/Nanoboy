using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBoy.Core
{
    public enum BreakpointFlag
    {
        Read = 1,
        Write = 2,
        Execute = 4
    }

    public class Breakpoint
    {
        public int Address { get; set; }
        public int TypeFlags { get; set; }

        public Breakpoint(int address, int flags)
        {
            Address = address;
            TypeFlags = flags;
        }
    }
}
