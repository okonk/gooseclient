using System;
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
            PacketManager.Register<WindowLinePacket>();
            PacketManager.Register<UpdateCharacterPacket>();
            PacketManager.Register<SpellbookSlotPacket>();
            PacketManager.Register<SpellCharacterPacket>();
            PacketManager.Register<SpellTilePacket>();
            PacketManager.Register<MapObjectPacket>();
            PacketManager.Register<EraseObjectPacket>();
            PacketManager.Register<BuffBarPacket>();
            PacketManager.Register<BattleTextPacket>();
            PacketManager.Register<AttackPacket>();
            PacketManager.Register<WeaponSpeedPacket>();
            PacketManager.Register<MakeWindowPacket>();
            PacketManager.Register<EndWindowPacket>();
        }

        public void Connect()
        {
            try
            {
                packetBuffer = "";
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(GameClient.ServerInfoSettings.Sections["Settings"]["IP"], GameClient.ServerInfoSettings.GetInt("Settings", "Port"));

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

        public void Use(int slotNumber)
        {
            Send($"USE{slotNumber + 1}");
        }

        public void Change(int fromSlot, int toSlot)
        {
            Send($"CHANGE{fromSlot + 1},{toSlot + 1}");
        }

        // SPLITslotid1,slotid2
        // ITWinvslot,windowid,windowslot
        // WTIwindowid,windowslot,invslot
        // WTWfromWindowId,fromWindowSlot,toWindowId,toWindowSlot

        public void Cast(int slot, int targetId)
        {
            Send($"CAST{slot + 1},{targetId}");
        }

        public void Get()
        {
            Send($"GET");
        }

        public void KillBuff(int slot)
        {
            Send($"KBUF{slot + 1}");
        }

        public void Attack()
        {
            Send($"ATT");
        }

        public void DropItem(int slot, int stackSize)
        {
            Send($"DRP{slot + 1},{stackSize}");
        }

        public void LeftClick(int tileX, int tileY)
        {
            Send($"LC{tileX + 1},{tileY + 1}");
        }

        public void RightClick(int tileX, int tileY)
        {
            Send($"RC{tileX + 1},{tileY + 1}");
        }

        public void WindowButtonClick(int buttonId, int windowId, int npcId, int unknownId1 = 0, int unknownId2 = 0)
        {
            Send($"WBC{buttonId + 1},{windowId},{npcId},{unknownId1},{unknownId2}");
        }

        public void VendorPurchaseItem(int npcId, int slotId)
        {
            Send($"VPI{npcId},{slotId + 1}");
        }

        public void VendorSellItem(int npcId, int slotId, int stackSize)
        {
            Send($"VSI{npcId},{slotId + 1},{stackSize}");
        }

        public void GetItemDetails(int itemId)
        {
            Send($"GID{itemId}");
        }
    }
}
