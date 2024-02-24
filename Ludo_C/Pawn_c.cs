using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ludo_C
{
    class Pawn_c: Position_c
    {
        public Button btnPawn;
        private int position;

        public Pawn_c(int x, int y) : base(x, y)
        {
            position = -1;
            btnPawn = new Button
            {
                Left = x,
                Top = y,
                Width = Cell_c.dim,
                Height = Cell_c.dim,
                Enabled = false
            };
        }

        public int Position
        {
            get { return position; }
            set { this.position = value; }
        }
    }
}
