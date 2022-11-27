using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexEd
{
    public record Selection
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public ConsoleColor Color { get; set; }
    }
}
