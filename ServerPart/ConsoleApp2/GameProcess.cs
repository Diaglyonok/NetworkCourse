using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ServerElementsGame
{
    public class GameProcess
    {
        Server myServer;

        private bool running;

        public bool Running { get => running; set => running = value; }

        ArrayList elements = new ArrayList { "fire", "tree", "water", "iron", "earth", "nothing"};

        public GameProcess(Server server)
        {
            this.myServer = server;
        }

        public void Start()
        {
            if (!myServer.checkClients())
                return;
            if (!myServer.checkClientsStarted())
                return;
            if (!myServer.checkClientsReady())
                return;
            if (myServer.Game.Running)
                return;
            myServer.BroadcastMessage("game_started");
            resetClients();
            Running = true;
        }

        public int compare(int a, int b) // >0 if a, <0 if b, == 0 if none 
        {
            if (a != 5 && b == 5)
                return 1;
            if (a == 5 && b != 5)
                return -1;
            if ((a + 1) % 5 == b || (a + 3) % 5 == b)
                return 1;
            if ((a + 2) % 5 == b || (a + 4) % 5 == b)
                return -1;

            return 0;
        }

        internal void End()
        {
            if (!myServer.checkClients())
                return;
               

            int a = myServer.Clients[0].Choosed == "" ? -1 : elements.IndexOf(myServer.Clients[0].Choosed);
            int b = myServer.Clients[1].Choosed == "" ? -1 : elements.IndexOf(myServer.Clients[1].Choosed);
            int result = compare(elements.IndexOf(myServer.Clients[0].Choosed), elements.IndexOf(myServer.Clients[1].Choosed));
            running = false;
            if (result > 0)
            {
                myServer.Clients[0].Score += 1;
                //Выйграл клиент 0
                myServer.BroadcastMessageByEndPoint("results: win yours " + (elements.IndexOf(myServer.Clients[0].Choosed) + 1) + " score " + myServer.Clients[0].Score +
                    " enemy " + (elements.IndexOf(myServer.Clients[1].Choosed) + 1) + " score " + myServer.Clients[1].Score + " .", myServer.Clients[0].EndPoint );
                myServer.BroadcastMessageByEndPoint("results: lose yours " + (elements.IndexOf(myServer.Clients[1].Choosed) + 1) + " score " + myServer.Clients[1].Score +
                   " enemy " + (elements.IndexOf(myServer.Clients[0].Choosed) + 1) + " score " + myServer.Clients[0].Score + " .", myServer.Clients[1].EndPoint);
                Console.WriteLine(myServer.Clients[0].Name + " won");
            }
            else if (result < 0)
            {
                myServer.Clients[1].Score += 1;
                //Выйграл клиент 1
                myServer.BroadcastMessageByEndPoint("results: win yours " + (elements.IndexOf(myServer.Clients[1].Choosed) + 1) + " score " + myServer.Clients[1].Score +
                    " enemy " + (elements.IndexOf(myServer.Clients[0].Choosed) + 1) + " score " + myServer.Clients[0].Score + " .", myServer.Clients[1].EndPoint);
                myServer.BroadcastMessageByEndPoint("results: lose yours " + (elements.IndexOf(myServer.Clients[0].Choosed) + 1) + " score " + myServer.Clients[0].Score +
                   " enemy " + (elements.IndexOf(myServer.Clients[1].Choosed) + 1) + " score " + myServer.Clients[1].Score + " .", myServer.Clients[0].EndPoint);

                Console.WriteLine(myServer.Clients[1].Name + " won");
            }
            else
            {
                myServer.Clients[1].Score += 1;
                myServer.Clients[0].Score += 1;
                myServer.BroadcastMessageByEndPoint("results: draw yours " + (elements.IndexOf(myServer.Clients[1].Choosed) + 1) + " score yours " + myServer.Clients[1].Score + " enemy " + myServer.Clients[0].Score + " .", myServer.Clients[1].EndPoint);
                myServer.BroadcastMessageByEndPoint("results: draw yours " + (elements.IndexOf(myServer.Clients[0].Choosed) + 1) + " score yours " + myServer.Clients[0].Score + " enemy " + myServer.Clients[1].Score + " .", myServer.Clients[0].EndPoint);
                Console.WriteLine("Ничья");
            }
        }

        private void resetClients()
        {
            
            myServer.Clients[0].Choosed = "nothing";
            myServer.Clients[1].Choosed = "nothing";
            myServer.Clients[0].Started = true;
            myServer.Clients[1].Started = true;
            myServer.Clients[0].Ready = false;
            myServer.Clients[1].Ready = false;
            myServer.Clients[0].Ended = false;
            myServer.Clients[1].Ended = false;
        }

        private void resetScores()
        {

            //НЕ ВЫЗЫВАТЬ БЕЗ ЧЕКА
            myServer.Clients[0].Score = 0;
            myServer.Clients[1].Score = 0;
        }
    }
}
