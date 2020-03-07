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

        public bool Typing { get { return inputBox.HasFocus; } }

        private string replyToName = null;

        private Dictionary<string, string> commandAliases = new Dictionary<string, string>();

        private bool ignoreTextInput = false;

        private List<string> inputHistory = new List<string>();
        private int inputHistoryIndex = 0;

        public ChatWindow() : base("Chat")
        {
            hideShortcutKey = SDL.SDL_Keycode.SDLK_F4;

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
            inputBox.UpPressed += ChatUpPressed;
            inputBox.DownPressed += ChatDownPressed;
            inputBox.PreventFurtherEventsOnInput = true;
            AddChild(inputBox);

            GameClient.NetworkClient.PacketManager.Listen<ServerMessagePacket>(OnServerMessage);
            GameClient.NetworkClient.PacketManager.Listen<ChatPacket>(OnChat);
            GameClient.NetworkClient.PacketManager.Listen<HashMessagePacket>(OnHashMessage);

            LoadAliases();

            chatListBox.FilterPickupMessages = GameClient.UserSettings.GetBool("GooseSettings", "FilterPickupMessages", false);
        }

        public override void SaveState()
        {
            base.SaveState();

            GameClient.UserSettings.SetValue<bool>("GooseSettings", "FilterPickupMessages", chatListBox.FilterPickupMessages);
        }
        private void LoadAliases()
        {
            var aliases = GameClient.UserSettings.Sections["Alias"];
            foreach (var value in aliases.Values)
            {
                int comma = value.IndexOf(',');

                string alias = value.Substring(0, comma);
                string replacement = value.Substring(comma + 1);

                commandAliases[alias.ToLowerInvariant()] = replacement;
            }
        }

        public void OnServerMessage(object packet)
        {
            var p = (ServerMessagePacket)packet;

            chatListBox.AddText(p.Colour, p.Message);

            if (p.Colour == 6 && p.Message.StartsWith("[tell from] "))
            {
                replyToName = p.Message.Substring(12, p.Message.IndexOf(':') - 12);
            }
        }

        public void OnChat(object packet)
        {
            var p = (ChatPacket)packet;

            chatListBox.AddText(1, p.Message);
        }

        public void OnHashMessage(object packet)
        {
            var p = (HashMessagePacket)packet;

            chatListBox.AddText(1, p.Message);
        }

        public void ChatEnterPressed()
        {
            string message = inputBox.Value;
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message[0] == '/')
                {
                    HandleCommand(message);
                }
                else
                {
                    GameClient.NetworkClient.ChatMessage(message);
                }

                inputHistory.Add(message);
                inputHistoryIndex = inputHistory.Count;
            }

            inputBox.SetValue("");
            inputBox.RemoveFocused();
        }

        public void ChatEscapePressed()
        {
            inputBox.SetValue("");
            inputBox.RemoveFocused();
            inputHistoryIndex = inputHistory.Count;
        }

        public void HandleCommand(string commandText)
        {
            int space = commandText.IndexOf(' ');

            string command = commandText;
            if (space != -1)
            {
                command = commandText.Substring(0, space);
            }

            string replacedCommand = null;
            if (commandAliases.TryGetValue(command.ToLowerInvariant(), out replacedCommand))
            {
                replacedCommand = replacedCommand + commandText.Substring(command.Length);
            }
            else
            {
                replacedCommand = commandText;
            }

            if (replacedCommand.ToLowerInvariant() == "/quit")
            {
                GameClient.Quit();
            }
            if (replacedCommand.ToLowerInvariant() == "/filter pickup")
            {
                ToggleFilterPickup();
            }
            else
            {
                if (replacedCommand[0] == '/')
                    GameClient.NetworkClient.Command(replacedCommand);
                else
                    GameClient.NetworkClient.ChatMessage(replacedCommand);
            }
        }

        private void ToggleFilterPickup()
        {
            chatListBox.FilterPickupMessages = !chatListBox.FilterPickupMessages;

            chatListBox.AddText(8, $"Group item pick up messages are now {(chatListBox.FilterPickupMessages ? "hidden" : "visible")}.");
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    if (ignoreTextInput)
                    {
                        ignoreTextInput = false;
                        return true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (!inputBox.HasFocus && 
                            (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_RETURN || 
                             ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_KP_ENTER))
                    {
                        inputBox.SetFocused();
                        return true;
                    }
                    else if (!inputBox.HasFocus && ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_SLASH)
                    {
                        inputBox.SetValue("/");
                        inputBox.SetFocused();
                        ignoreTextInput = true;
                        return true;
                    }
                    else if (!inputBox.HasFocus && ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_g)
                    {
                        inputBox.SetValue("/guild ");
                        inputBox.SetFocused();
                        ignoreTextInput = true;
                        return true;
                    }
                    else if (!inputBox.HasFocus && ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_t)
                    {
                        inputBox.SetValue("/tell ");
                        inputBox.SetFocused();
                        ignoreTextInput = true;
                        return true;
                    }
                    else if (!inputBox.HasFocus && ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_r)
                    {
                        inputBox.SetValue("/tell " + (replyToName == null ? "" : replyToName + " "));
                        inputBox.SetFocused();
                        ignoreTextInput = true;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }

        public void ChatUpPressed()
        {
            inputHistoryIndex = Math.Max(0, inputHistoryIndex - 1);
            inputBox.SetValue(inputHistory[inputHistoryIndex]);
        }

        public void ChatDownPressed()
        {
            inputHistoryIndex = inputHistoryIndex + 1;

            if (inputHistoryIndex < inputHistory.Count)
            {
                inputBox.SetValue(inputHistory[inputHistoryIndex]);
            }
            else
            {
                inputBox.SetValue("");
                inputHistoryIndex = inputHistory.Count;
            }
        }
    }
}
