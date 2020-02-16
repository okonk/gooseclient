using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SDL2;

namespace AsperetaClient
{
    class ChatWindow : BaseWindow
    {
        private ChatListBox chatListBox;

        private TextBox inputBox;

        public ChatWindow() : base("Chat")
        {
            var chatXY = GameClient.WindowSettings.GetCoords(this.Name, "objoff");
            var chatWH = GameClient.WindowSettings.GetCoords(this.Name, "objdim");
            var windim = GameClient.WindowSettings.GetCoords(this.Name, "windim");

            var h = windim.ElementAt(1) * chatWH.ElementAt(1);

            chatListBox = new ChatListBox(chatXY.ElementAt(0), chatXY.ElementAt(1), chatWH.ElementAt(0), h, windim.ElementAt(0));
            AddChild(chatListBox);

            var inputXY = GameClient.WindowSettings.GetCoords(this.Name, "obj2off");
            var inputWH = GameClient.WindowSettings.GetCoords(this.Name, "obj2dim");
            inputBox = new TextBox(inputXY.ElementAt(0), inputXY.ElementAt(1), inputWH.ElementAt(0), inputWH.ElementAt(1), null, Colour.White);
            inputBox.Padding = 6;
            inputBox.MaxLength = 200;
            inputBox.EnterPressed += ChatEnterPressed;
            inputBox.EscapePressed += ChatEscapePressed;
            inputBox.PreventFurtherEventsOnInput = true;
            AddChild(inputBox);

            GameClient.NetworkClient.PacketManager.Listen<ServerMessagePacket>(OnServerMessage);
            GameClient.NetworkClient.PacketManager.Listen<ChatPacket>(OnChat);
            GameClient.NetworkClient.PacketManager.Listen<HashMessagePacket>(OnHashMessage);
        }

        public void OnServerMessage(object packet)
        {
            var p = (ServerMessagePacket)packet;

            chatListBox.AddLine(p.Colour, p.Message);
        }

        public void OnChat(object packet)
        {
            var p = (ChatPacket)packet;

            chatListBox.AddLine(1, p.Message);
        }

        public void OnHashMessage(object packet)
        {
            var p = (HashMessagePacket)packet;

            chatListBox.AddLine(1, p.Message);
        }

        public void ChatEnterPressed()
        {
            string message = inputBox.Value;
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message[0] == '/')
                {
                    // TODO: map aliases
                    GameClient.NetworkClient.Command(message);
                }
                else
                {
                    GameClient.NetworkClient.ChatMessage(message);
                }
            }

            inputBox.SetValue("");
            inputBox.HasFocus = false;
        }

        public void ChatEscapePressed()
        {
            inputBox.SetValue("");
            inputBox.HasFocus = false;
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (!inputBox.HasFocus && 
                            (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_SLASH || 
                             ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_RETURN || 
                             ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_KP_ENTER))
                    {
                        inputBox.SetFocussed();
                        return true;
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_F4)
                    {
                        this.Hidden = !this.Hidden;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }
    }
}
