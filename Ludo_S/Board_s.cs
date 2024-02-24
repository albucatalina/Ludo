using Ludo_S.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ludo_S
{
    class Board_s: Position_s
    {
        public List<Cell_s> cellList = new List<Cell_s>();
        public List<Cell_s> blueCellList = new List<Cell_s>();
        private Panel boardPanel;

        public Board_s(int x, int y) : base(x, y)
        {
            GenerateCellList();
        }

        public void Show(Form f)
        {
            boardPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Beige,
                Left = x,
                Top = y,
                Width = 15 * Cell_s.dim,
                Height = 15 * Cell_s.dim
            };
            boardPanel.BackgroundImage = Resources.board;
            boardPanel.BackgroundImageLayout = ImageLayout.Stretch;
            f.Controls.Add(boardPanel);

            foreach (Cell_s cell in cellList)
            {
                boardPanel.Controls.Add(cell.cellPanel);
            }

        }

        public void ShowPlayers(Player_s p1, Player_s p2)
        {
            #region Player1
            //panel-ul unde vor sta initial pionii
            Panel player1Panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = p1.Color,
                Left = 0,
                Top = 9 * Cell_s.dim,
                Width = Cell_s.dim * 6,
                Height = 6 * Cell_s.dim
            };

            //le pun imagine la pioni si ii adaug pe panel
            for (int i = 0; i < 5; i++)
            {
                p1.pawnList[i].btnPawn.BackgroundImage = Resources.red_pawn;
                p1.pawnList[i].btnPawn.BackgroundImageLayout = ImageLayout.Stretch;
                player1Panel.Controls.Add(p1.pawnList[i].btnPawn);
            }

            boardPanel.Controls.Add(player1Panel); //adaug panel-ul pe tabla

            int x = 7 * Cell_s.dim;  //coordonatele primei casute rosii
            int y = 13 * Cell_s.dim;
            for (int i = 0; i < 5; i++) //casutele rosii
            {
                Cell_s cell = new Cell_s(x, y, p1.Color);
                cellList.Add(cell); //le adaug in lista ca sa se poata afisa pionii(doar pt primul player)
                boardPanel.Controls.Add(cell.cellPanel); //adaug pe tabla panel-ul celulei
                y -= Cell_s.dim;
            }
            #endregion

            #region Player2
            Panel player2Panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = p2.Color,
                Left = 9 * Cell_s.dim,
                Top = 0,
                Width = Cell_s.dim * 6,
                Height = 6 * Cell_s.dim
            };
            for (int i = 0; i < 5; i++)
            {
                p2.pawnList[i].btnPawn.BackgroundImage = Resources.blue_pawn;
                p2.pawnList[i].btnPawn.BackgroundImageLayout = ImageLayout.Stretch;
                player2Panel.Controls.Add(p2.pawnList[i].btnPawn);
            }
            boardPanel.Controls.Add(player2Panel);

            y = Cell_s.dim;
            for (int i = 0; i < 5; i++)
            {
                Cell_s cell = new Cell_s(x, y, p2.Color);
                blueCellList.Add(cell);
                boardPanel.Controls.Add(cell.cellPanel);
                y += Cell_s.dim;
            }
            #endregion
        }

        private void GenerateCellList()
        {
            int dim = Cell_s.dim;
            int x = 6 * dim;  //pozitia primei casute
            int y = 14 * dim;
            for (int i = 1; i <= 52; i++)
            {
                if (i > 1 && i <= 7 || i == 13 || i == 14 || i >= 20 && i <= 25)
                    y -= dim;
                if (i >= 7 && i <= 12 || i >= 41 && i <= 46 || i == 52)
                    x -= dim;
                if (i >= 15 && i <= 20 || i == 26 || i == 27 || i >= 33 && i <= 38)
                    x += dim;
                if (i >= 28 && i <= 33 || i == 39 || i == 40 || i >= 46 && i <= 51)
                    y += dim;

                Cell_s cell = new Cell_s(x, y, Color.White);
                cellList.Add(cell);
            }
        }
    }
}
