using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo_C
{
    class Player_c
    {
        private String name;
        private Color color;
        public List<Pawn_c> pawnList = new List<Pawn_c>();

        public Player_c(Color color)
        {
            this.color = color;
            GeneratePawnList();
        }

        private void GeneratePawnList()
        {
            int dim = Cell_c.dim;

            //adaug pionii in lista(in functie de locatia pe acel panel unde vor sta pionii
            pawnList.Add(new Pawn_c(dim, dim));
            pawnList.Add(new Pawn_c(4 * dim, dim));
            pawnList.Add(new Pawn_c(2 * dim + dim / 2, 2 * dim + dim / 2));
            pawnList.Add(new Pawn_c(dim, 4 * dim));
            pawnList.Add(new Pawn_c(4 * dim, 4 * dim));
        }

        public String Name
        {
            get { return name; }
            set { this.name = value; }
        }

        public Color Color
        {
            get { return color; }
            set { this.color = value; }
        }
    }
}
