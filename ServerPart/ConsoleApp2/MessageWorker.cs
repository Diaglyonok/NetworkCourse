using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;

namespace ServerElementsGame
{
    internal class MessageWorker
    {
        Client client;
        ArrayList gameCommands = new ArrayList { "fire", "earth", "iron", "water", "tree", "nothing" };
        ArrayList controlCommands = new ArrayList { "start", "ready", "ended_round" };
        public MessageWorker(Client client)
        {
            this.client = client;
        }

        public void receiveMessage(string message)
        {
            if (gameCommands.Contains(message))
            {
                client.Choosed = message;
            }

            if (controlCommands.Contains(message))
            {
                switch (message)
                {
                    case "start":
                        client.Started = true;
                        break;

                    case "ready":
                        client.MyServer.outputClients();
                        client.Ready = true;
                        client.MyServer.Game.Start();
                        break;

                    case "ended_round":
                        client.Ended = true;
                        if (client.MyServer.Clients.Count != 2 || client.MyServer.Clients[0] == null || client.MyServer.Clients[1] == null)
                            break;
                        if (!client.MyServer.Clients[0].Ended || !client.MyServer.Clients[1].Ended)
                            break;

                        client.MyServer.Game.End();
                        break;
                }
            }
        }
    }    
}