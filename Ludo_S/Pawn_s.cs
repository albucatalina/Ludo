using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ludo_S
{
    class Pawn_s: Position_s
    {
        public Button btnPawn;
        private int position;

        public Pawn_s(int x, int y) : base(x, y)
        {
            position = -1;
            btnPawn = new Button
            {
                Left = x,
                Top = y,
                Width = Cell_s.dim,
                Height = Cell_s.dim,
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
