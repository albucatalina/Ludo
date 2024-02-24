using Ludo_C.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ludo_C
{
    public partial class clientForm : Form
    {
        #region Variabile pentru joc
        private Player_c player1; //player curent
        private Player_c player2; //adversar
        private Board_c board;
        private int diceValue;
        private System.Windows.Forms.Timer timer;
        private ProgressBar progressBar;
        private ImageList imageList;
        #endregion

        #region Variabile pentru retea
        public TcpClient client;
        private static clientForm cForm;
        Thread t;
        bool workThread;
        NetworkStream streamClient;
        #endregion

        public clientForm()
        {
            InitializeComponent();
            cForm = this;

            player1 = new Player_c(Color.Blue);
            player2 = new Player_c(Color.Red);
            board = new Board_c(10, 10);

            for (int i = 0; i < 5; i++)
            {
                player1.pawnList[i].btnPawn.Click += BtnPawn_Click;
            }

            #region pentru zar
            timer = new System.Windows.Forms.Timer { Interval = 25 };
            timer.Tick += new EventHandler(this.timer_Tick);
            progressBar = new ProgressBar { Minimum = 0, Maximum = 200, Step = 20 };
            imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(128, 128)
            };
            imageList.Images.Add(Resources.dice1);
            imageList.Images.Add(Resources.dice2);
            imageList.Images.Add(Resources.dice3);
            imageList.Images.Add(Resources.dice4);
            imageList.Images.Add(Resources.dice5);
            imageList.Images.Add(Resources.dice6);
            #endregion
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Conectare")
            {
                client = new TcpClient("10.13.10.236", 3000);//ip-ul meu
                workThread = true;
                t = new Thread(new ThreadStart(ClientListen));
                t.Start();
                streamClient = client.GetStream();
                btnConnect.Text = "Deconectare";

                StreamWriter streamWriter = new StreamWriter(streamClient);
                streamWriter.AutoFlush = true; // enable automatic flushing
                streamWriter.WriteLine("#Connected");
                streamWriter.WriteLine(tbPlayerName.Text);
                player1.Name = tbPlayerName.Name;
            }
            else
            {
                btnConnect.Enabled = false;
                workThread = false;
                t.Abort();
                StreamWriter streamWriter = new StreamWriter(streamClient);
                streamWriter.AutoFlush = true;
                streamWriter.WriteLine("#Gata");
            }
        }

        private void btnRoll_Click(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            progressBar.PerformStep();
            if (progressBar.Value == progressBar.Maximum)
            {
                timer.Stop();
                if (diceValue != 6)
                {
                    btnRoll.Enabled = false;
                    StreamWriter streamWriter = new StreamWriter(streamClient);
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine("#Turn");
                }
                else
                    btnRoll.Enabled = true;

                #region conditii ca pionii sa poata fi mutati
                for (int i = 0; i < 5; i++)
                {
                    player1.pawnList[i].btnPawn.Enabled = true;  //pp ca este valid
                    if (player1.pawnList[i].Position == -1 && diceValue != 6)  //la iesirea din casa trb sa fie val zarului 6
                        player1.pawnList[i].btnPawn.Enabled = false;
                    if (player1.pawnList[i].Position + diceValue < board.cellList.Count) //sa nu iese din lista
                    {
                        if (board.cellList[player1.pawnList[i].Position + diceValue].State == true) //daca unde va fi afisat este ocupat
                            player1.pawnList[i].btnPawn.Enabled = false;
                    }
                    else
                        player1.pawnList[i].btnPawn.Enabled = false;
                }
                #endregion

                progressBar.Value = 0;
            }
            else
            {
                diceValue = Dice_c.Roll();
                pbDice.Image = imageList.Images[diceValue - 1];
                StreamWriter streamWriter = new StreamWriter(streamClient);
                streamWriter.AutoFlush = true;
                streamWriter.WriteLine("#DiceValue");
                streamWriter.WriteLine(diceValue);
            }
        }

        private void BtnPawn_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < 5; i++)
                if (((Button)sender) == player1.pawnList[i].btnPawn) //am verificat care buton a fost apasat
                {
                    player1.pawnList[i].btnPawn.Left = 0;
                    player1.pawnList[i].btnPawn.Top = 0;
                    if (player1.pawnList[i].Position != -1)
                        board.cellList[player1.pawnList[i].Position].State = false;
                    player1.pawnList[i].Position += diceValue;
                    board.cellList[player1.pawnList[i].Position].cellPanel.Controls.Add(player1.pawnList[i].btnPawn);
                    board.cellList[player1.pawnList[i].Position].State = true;

                    //trimit nr pionului si pozitia lui
                    if (workThread)
                    {
                        StreamWriter streamWriter = new StreamWriter(streamClient);
                        streamWriter.AutoFlush = true;
                        streamWriter.WriteLine("#Move");
                        streamWriter.WriteLine(i);
                        streamWriter.WriteLine(player1.pawnList[i].Position);
                    }

                }
            for (int i = 0; i < 5; i++)
            {
                player1.pawnList[i].btnPawn.Enabled = false;  //sa nu pot muta de mai multe ori sau mai multi pioni
            }

            //dupa ce am mutat pionul se verifica daca toti pionii sunt in casutele finale
            bool winner = true;
            for (int i = 52; i < 57; i++)
            {
                if (board.cellList[i].State == false)
                    winner = false;
            }
            if (winner == true)
            {
                lblWinner.Text = "Ai castigat!";
                lblWinner.Visible = true;
                StreamWriter streamWriter = new StreamWriter(streamClient);
                streamWriter.AutoFlush = true;
                streamWriter.WriteLine("#Winner");
            }

        }

        private void ClientListen()
        {
            StreamReader readFromServer = new StreamReader(streamClient);
            String clientData1;
            String clientData2;
            MethodInvoker m;

            while (workThread)
            {
                clientData1 = readFromServer.ReadLine();

                if (clientData1 == null) break;
                if (clientData1 == "#Start")
                {
                    clientData2 = readFromServer.ReadLine();
                    player2.Name = clientData2;
                    m = new MethodInvoker(() => board.Show(cForm));
                    cForm.Invoke(m);
                    m = new MethodInvoker(() => board.ShowPlayers(player1, player2));
                    cForm.Invoke(m);

                }
                else if (clientData1 == "#Move")
                {
                    clientData1 = readFromServer.ReadLine(); //indexul pionului
                    clientData2 = readFromServer.ReadLine(); //pozitia
                    Move_Player2(int.Parse(clientData1), int.Parse(clientData2));
                }
                else if (clientData1 == "#Turn")
                {
                    m = new MethodInvoker(() => btnRoll.Enabled = true);
                    btnRoll.Invoke(m);
                }
                else if (clientData1 == "#DiceValue")
                {
                    clientData2 = readFromServer.ReadLine();
                    m = new MethodInvoker(() => pbDice.Image = imageList.Images[int.Parse(clientData2) - 1]);
                    pbDice.Invoke(m);
                }
                else if (clientData1 == "Winner")
                {
                    m = new MethodInvoker(() => lblWinner.Text="Ai pierdut...");
                    lblWinner.Invoke(m);
                    m = new MethodInvoker(() => lblWinner.Visible = true);
                    lblWinner.Invoke(m);
                }
            }
        }

        private void Move_Player2(int pawnListIndex, int position)
        {
            MethodInvoker m;
            m = new MethodInvoker(() => player2.pawnList[pawnListIndex].btnPawn.Left = 0);
            player2.pawnList[pawnListIndex].btnPawn.Invoke(m);
            m = new MethodInvoker(() => player2.pawnList[pawnListIndex].btnPawn.Top = 0);
            player2.pawnList[pawnListIndex].btnPawn.Invoke(m);

            if (position >= 52 && position <= 56) //daca a ajuns pe patratele rosii
            {
                m = new MethodInvoker(() => board.redCellList[position - 52].cellPanel.Controls.Add(player2.pawnList[pawnListIndex].btnPawn));
                board.redCellList[position - 52].cellPanel.Invoke(m);
            }
            else
            {
                if (player2.pawnList[pawnListIndex].Position != -1)
                    board.cellList[player2.pawnList[pawnListIndex].Position].State = false;
                if (position >= 0 && position <= 25)
                    player2.pawnList[pawnListIndex].Position = position + 26;
                if (position >= 26 && position <= 51)
                    player2.pawnList[pawnListIndex].Position = position - 26;

                m = new MethodInvoker(() => board.cellList[player2.pawnList[pawnListIndex].Position].cellPanel.Controls.Add(player2.pawnList[pawnListIndex].btnPawn));
                board.cellList[player2.pawnList[pawnListIndex].Position].cellPanel.Invoke(m);
                board.cellList[player2.pawnList[pawnListIndex].Position].State = true;

            }
        }

        private void clientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            workThread = false;
            t.Abort();
            streamClient.Close();
            client.Close();
        }
    }
}
