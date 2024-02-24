using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo_S
{
    class Position_s
    {
        protected int x, y;

        public Position_s()
        {
            x = 0; y = 0;
        }

        public Position_s(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
