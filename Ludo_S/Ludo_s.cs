using Ludo_S.Properties;
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

namespace Ludo_S
{
    public partial class serverForm : Form
    {
        #region Variabile pentru joc
        private Player_s player1; //player curent
        private Player_s player2; //adversar
        private Board_s board;
        private int diceValue;
        private System.Windows.Forms.Timer timer;
        private ProgressBar progressBar;
        private ImageList imageList;
        #endregion

        #region Variabile pentru retea
        public TcpListener server;
        private static serverForm sForm;
        Thread t;
        bool workThread;
        NetworkStream streamServer;
        #endregion

        public serverForm()
        {
            InitializeComponent();
            player1 = new Player_s(Color.Red);
            player2 = new Player_s(Color.Blue);
            board = new Board_s(10, 10);

            for(int i = 0; i < 5; i++)
            {
                player1.pawnList[i].btnPawn.Click += BtnPawn_Click;
            }

            #region pentru zar
            timer = new System.Windows.Forms.Timer{ Interval = 25};
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

            #region pentru server
            server = new TcpListener(System.Net.IPAddress.Any, 3000);
            server.Start();

            t = new Thread(new ThreadStart(ServerListen));
            workThread = true;
            t.Start();

            sForm = this;
            #endregion

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            board.Show(this);
            board.ShowPlayers(player1, player2);
            player1.Name = tbPlayerName.Text;
            StreamWriter streamWriter = new StreamWriter(streamServer);
            streamWriter.AutoFlush = true; // enable automatic flushing
            streamWriter.WriteLine("#Start");
            streamWriter.WriteLine(player1.Name);
            lblMessage.Visible = false;
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
                    StreamWriter streamWriter = new StreamWriter(streamServer);
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
                diceValue = Dice_s.Roll();
                pbDice.Image = imageList.Images[diceValue - 1];
                StreamWriter streamWriter = new StreamWriter(streamServer);
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
                    if (player1.pawnList[i].Position != -1) //pionul este pe tabla
                        board.cellList[player1.pawnList[i].Position].State = false;
                    player1.pawnList[i].Position += diceValue;
                    board.cellList[player1.pawnList[i].Position].cellPanel.Controls.Add(player1.pawnList[i].btnPawn);
                    board.cellList[player1.pawnList[i].Position].State = true;

                    //trimit nr pionului si pozitia lui
                    if (workThread)
                    {
                        StreamWriter streamWriter = new StreamWriter(streamServer);
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

            //dupa ce am mutat pionul se verifica daca toti pionii sunt in casutele colorate
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
                StreamWriter streamWriter = new StreamWriter(streamServer);
                streamWriter.AutoFlush = true;
                streamWriter.WriteLine("#Winner");
            }
        }

        public void ServerListen()
        {
            while (workThread)
            {
                Socket socketServer = server.AcceptSocket();
                streamServer = new NetworkStream(socketServer);
                StreamReader readFromClient = new StreamReader(streamServer);

                while (workThread)
                {
                    String serverData1 = readFromClient.ReadLine();
                    String serverData2;
                    MethodInvoker m;
                    if (serverData1 == null) break; //nu primesc nimic - clientul a plecat
                    else if (serverData1 == "#Connected")
                    {
                        serverData2 = readFromClient.ReadLine();
                        player2.Name = serverData2;
                        m = new MethodInvoker(() => lblMessage.Text = player2.Name + " s-a conectat");
                        lblMessage.Invoke(m);
                        m = new MethodInvoker(() => btnStart.Enabled=true);
                        btnStart.Invoke(m);

                    }
                    else if (serverData1 == "#Gata") //ca sa pot sa inchid serverul
                    {
                        workThread = false;
                        m = new MethodInvoker(() => lblMessage.Text = player2.Name + " s-a deconectat");
                        lblMessage.Invoke(m);
                        m = new MethodInvoker(() => lblMessage.Visible=true);
                        lblMessage.Invoke(m);
                    }
                    else if (serverData1 == "#Move")
                    {
                        serverData1 = readFromClient.ReadLine(); //numarul pionului
                        serverData2 = readFromClient.ReadLine(); //pozitia
                        Move_Player2(int.Parse(serverData1), int.Parse(serverData2));
                    }
                    else if (serverData1 == "#Turn")
                    {
                        m = new MethodInvoker(() => btnRoll.Enabled = true);
                        btnRoll.Invoke(m);
                    }
                    else if (serverData1 == "#DiceValue")
                    {
                        serverData2 = readFromClient.ReadLine();
                        m = new MethodInvoker(() => pbDice.Image = imageList.Images[int.Parse(serverData2) - 1]);
                        pbDice.Invoke(m);
                    }
                    else if (serverData1 == "#Winner")
                    {
                        m = new MethodInvoker(() => lblWinner.Text = "Ai pierdut...");
                        lblWinner.Invoke(m);
                        m = new MethodInvoker(() => lblWinner.Visible=true);
                        lblWinner.Invoke(m);
                    }
                }
                streamServer.Close();
                socketServer.Close();
            }
        }

        private void Move_Player2(int pawnListIndex,int position)
        {
            MethodInvoker m;
            m = new MethodInvoker(() => player2.pawnList[pawnListIndex].btnPawn.Left = 0);
            player2.pawnList[pawnListIndex].btnPawn.Invoke(m);
            m = new MethodInvoker(() => player2.pawnList[pawnListIndex].btnPawn.Top = 0);
            player2.pawnList[pawnListIndex].btnPawn.Invoke(m);

            if (position >= 52 && position <= 56) //daca a ajuns pe patratele albastre
            {
                m = new MethodInvoker(() => board.blueCellList[position - 52].cellPanel.Controls.Add(player2.pawnList[pawnListIndex].btnPawn));
                board.blueCellList[position - 52].cellPanel.Invoke(m);
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

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            workThread = false;
            streamServer.Close();
        }
    }
}
