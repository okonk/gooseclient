﻿using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace AsperetaClient
{
    class NetworkClient
    {
        public event Action<Exception> ConnectionError;
        public event Action Connected;
        public event Action<Exception> ReceiveError;

        public PacketManager PacketManager { get; set; } = new PacketManager();

        public bool IsConnected { get { return socket == null ? false : socket.Connected; } }

        private Socket socket;

        private string packetBuffer;

        public NetworkClient()
        {
            PacketManager.Register<LoginSuccessPacket>();
            PacketManager.Register<LoginFailPacket>();
            PacketManager.Register<SendCurrentMapPacket>();
            PacketManager.Register<DoneSendingMapPacket>();
            PacketManager.Register<MakeCharacterPacket>();
            PacketManager.Register<SetYourCharacterPacket>();
            PacketManager.Register<PingPacket>();
            PacketManager.Register<MoveCharacterPacket>();
            PacketManager.Register<ChangeHeadingPacket>();
            PacketManager.Register<SetYourPositionPacket>();
            PacketManager.Register<VitalsPercentagePacket>();
            PacketManager.Register<EraseCharacterPacket>();
            PacketManager.Register<ServerMessagePacket>();
            PacketManager.Register<ChatPacket>();
            PacketManager.Register<HashMessagePacket>();
            PacketManager.Register<StatusInfoPacket>();
            PacketManager.Register<ExperienceBarPacket>();
            PacketManager.Register<InventorySlotPacket>();
        }

        public void Connect()
        {
            try
            {
                packetBuffer = "";
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect("127.0.0.1", 2006);

                Connected?.Invoke();
            }
            catch (Exception e)
            {
                ConnectionError?.Invoke(e);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (socket != null && socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket?.Close();
            }

            socket = null;
        }

        public void Send(string packet)
        {
            packet += '\x1';
            socket.Send(System.Text.Encoding.ASCII.GetBytes(packet));
        }

        public void Update()
        {
            if (!IsConnected) return;

            var readSockets = new List<Socket> { socket };

            Socket.Select(readSockets, null, null, 1000);

            foreach (var socket in readSockets)
            {
                try
                {
                    var buffer = new byte[4196];

                    int received = socket.Receive(buffer);
                    packetBuffer += System.Text.Encoding.ASCII.GetString(buffer, 0, received);

                    if (packetBuffer.Length == 0) continue;

                    string[] packets = packetBuffer.Split("\x1".ToCharArray());
                    int limit = packets.Length - 1;

                    if (!packetBuffer.EndsWith("\x1"))
                    {
                        packetBuffer = packetBuffer.Substring(packetBuffer.Length - packets[limit - 1].Length - 1);
                        limit--;
                    }
                    else
                    {
                        packetBuffer = "";
                    }

                    for (int i = 0; i < limit; i++)
                    {
                        PacketManager.Handle(packets[i]);
                    }
                }
                catch (Exception e)
                {
                    ReceiveError?.Invoke(e);
                }
            }
        }



        public void Login(string username, string password)
        {            
            Send($"LOGIN{username},{password},GooseClient");
        }

        public void LoginContinued()
        {
            Send($"LCNT");
        }

        public void DoneLoadingMap()
        {
            Send($"DLM");
        }

        public void Pong()
        {
            Send($"PONG");
        }

        public void Facing(Direction d)
        {
            int facing = (int)d + 1;
            // old asp client remaps facing for some reason...
            facing = (facing == 3 ? 2 : (facing == 4 ? 3 : (facing == 2 ? 4 : facing)));

            Send($"F{facing}");
        }

        public void Move(Direction d)
        {
            Send($"M{(int)d + 1}");
        }

        public void ChatMessage(string message)
        {
            Send($";{message}");
        }

        public void Command(string command)
        {
            Send(command);
        }
    }
}
