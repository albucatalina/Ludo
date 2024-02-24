using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo_C
{
    class Dice_c
    {
        private static Random value = new Random();

        public static int Roll()
        {
            return value.Next(1, 7);
        }
    }
}
