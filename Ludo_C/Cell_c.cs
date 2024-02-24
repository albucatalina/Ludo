using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ludo_C
{
    class Cell_c: Position_c
    {
        public static int dim = 30;
        public Panel cellPanel;
        private Color color;
        private bool state;

        public Cell_c(int x, int y, Color color) : base(x, y)
        {
            this.color = color;
            state = false;
            cellPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = color,
                Left = x,
                Top = y,
                Width = dim,
                Height = dim
            };
        }

        public bool State
        {
            get { return state; }
            set { this.state = value; }
        }
    }
}
