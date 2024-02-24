using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo_C
{
    class Position_c
    {
        protected int x, y;

        public Position_c()
        {
            x = 0; y = 0;
        }

        public Position_c(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
