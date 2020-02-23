using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class GameScreen : State
    {
        private int mapNumber;
        private string mapName;
        public Map Map { get; set; }

        private Character player;

        private bool moveKeyDown = false;
        private Direction moveKeyDirection = Direction.Down;
        private double moveKeyPressedTime = 0;

        private bool moveDelay = false;
        private double moveDelayElapsedTime = 0;
        private double attackElapsedTime = 0;
        private bool attacking = false;
        private double weaponSpeed = 1;

        private RootPanel uiRoot;

        public GameScreen(int mapNumber, string mapName)
        {
            this.mapNumber = mapNumber;
            this.mapName = mapName;

            
            GameClient.NetworkClient.PacketManager.Listen<SetYourCharacterPacket>(OnSetYourCharacter);
            GameClient.NetworkClient.PacketManager.Listen<PingPacket>(OnPing);
            GameClient.NetworkClient.PacketManager.Listen<SetYourPositionPacket>(OnSetYourPosition);
            GameClient.NetworkClient.PacketManager.Listen<SendCurrentMapPacket>(OnSendCurrentMap);
            GameClient.NetworkClient.PacketManager.Listen<WeaponSpeedPacket>(OnWeaponSpeed);
            GameClient.NetworkClient.PacketManager.Listen<MakeWindowPacket>(OnMakeWindow);
            GameClient.NetworkClient.PacketManager.Listen<EndWindowPacket>(OnEndWindow);
        }

        public override void Resuming()
        {
            GameClient.ScreenWidth = 640;
            GameClient.ScreenHeight = 480;
            SDL.SDL_RenderSetLogicalSize(GameClient.Renderer, GameClient.ScreenWidth, GameClient.ScreenHeight);
        }

        public override void Starting()
        {
            Resuming();

            CreateGui();

            LoadMap();
        }

        public void LoadMap()
        {
            GameClient.StateManager.AppendState(new MapLoadingScreen(mapNumber, mapName, this));
        }

        public void CreateGui()
        {
            this.uiRoot = new RootPanel();
            this.uiRoot.DropWasUnhandled += OnDropWasUnhandled;
            this.uiRoot.RightClickUnhandled += OnRightClick;

            this.uiRoot.AddChild(new ChatWindow());
            this.uiRoot.AddChild(new FpsWindow());
            this.uiRoot.AddChild(new BuffBarWindow());
            this.uiRoot.AddChild(new HPBarWindow());
            this.uiRoot.AddChild(new MPBarWindow());
            this.uiRoot.AddChild(new XPBarWindow());

            this.uiRoot.AddChild(new CharacterWindow());

            var spellbookWindow = new SpellbookWindow();
            spellbookWindow.CastSpell += (slot) => { Map.OnCastSpell(slot); };
            this.uiRoot.AddChild(spellbookWindow);

            this.uiRoot.AddChild(new InventoryWindow());

            // This needs to be added last since it has to take the slots from character/spellbook/inventory
            this.uiRoot.AddChild(new HotkeyBarWindow());
        }

        private int RenderOffsetX()
        {
            int half_x = GameClient.ScreenWidth / 2 - Constants.TileSize / 2;
            int start_x = player != null ? player.PixelXi - half_x : 0;

            return start_x;
        }

        private int RenderOffsetY()
        {
            int half_y = GameClient.ScreenHeight / 2 - Constants.TileSize;
            int start_y = player != null ? player.PixelYi - half_y : 0;

            return start_y;
        }
        
        public override void Render(double dt)
        {
            int start_x = RenderOffsetX();
            int start_y = RenderOffsetY();

            Map.Render(start_x, start_y);
            this.uiRoot.Render(dt, 0, 0);
        }

        public override void Update(double dt)
        {
            var keysPtr = SDL.SDL_GetKeyboardState(out int keysLength);
            byte[] keys = new byte[keysLength];
            Marshal.Copy(keysPtr, keys, 0, keysLength);

            if (!Map.Targeting && uiRoot.FocusedTextBox == null)
            {
                if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_UP] == 1)
                {
                    MoveKeyPressed(Direction.Up);
                }
                else if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
                {
                    MoveKeyPressed(Direction.Right);
                }
                else if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN] == 1)
                {
                    MoveKeyPressed(Direction.Down);
                }
                else if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
                {
                    MoveKeyPressed(Direction.Left);
                }
                else
                {
                    moveKeyDown = false;
                    moveKeyPressedTime = 0;
                }

                if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_SPACE] == 1)
                {
                    AttackKeyPressed();
                }
            }

            this.Map.Update(dt);

            if (moveKeyDown)
            {
                moveKeyPressedTime += dt;
            }

            if (moveDelay)
            {
                moveDelayElapsedTime += dt;

                if (moveDelayElapsedTime >= 0.5)
                {
                    moveDelay = false;
                    moveDelayElapsedTime = 0;
                }
            }

            if (attacking)
            {
                attackElapsedTime += dt;

                if (attackElapsedTime >= weaponSpeed)
                {
                    attackElapsedTime = 0;
                    attacking = false;
                }
            }

            this.uiRoot.Update(dt);
        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            bool preventFurtherEvents = this.Map.HandleEvent(ev);
            if (preventFurtherEvents)
                return;

            this.uiRoot.HandleEvent(ev, 0, 0);

            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    SaveUserSettings();
                    break;
            }
        }

        public void SaveUserSettings()
        {
            foreach (var window in uiRoot.Children.Where(g => g is BaseWindow).Cast<BaseWindow>())
            {
                window.SaveState();
            }

            GameClient.GameSettings.SaveFile();
            GameClient.UserSettings.SaveFile();
        }

        public void AttackKeyPressed()
        {
            if (attacking) return;

            attacking = true;
            attackElapsedTime = 0;

            player.Attack();
            GameClient.NetworkClient.Attack();
        }

        public bool CanMove()
        {
            return !this.uiRoot.Children.Any(c => c is VendorWindow);
        }

        public void MoveKeyPressed(Direction direction)
        {
            if (player == null || player.Moving || moveDelay) return;
            if (!CanMove()) return;

            bool delay = true;
            if (!moveKeyDown || moveKeyDirection != direction)
            {
                moveKeyDown = true;
                moveKeyDirection = direction;
                moveKeyPressedTime = 0;

                if (player.Facing != direction)
                {
                    player.Facing = direction;
                    GameClient.NetworkClient.Facing(direction);
                    return;
                }
                else
                {
                    delay = false;
                }
            }

            if (delay && moveKeyPressedTime < 0.1) return;

            int destX = player.TileX;
            int destY = player.TileY;

            switch (direction)
            {
                case Direction.Up:
                    destY -= 1;
                    break;
                case Direction.Right:
                    destX += 1;
                    break;
                case Direction.Down:
                    destY += 1;
                    break;
                case Direction.Left:
                    destX -= 1;
                    break;
            }

            if (Map.CanMoveTo(destX, destY))
            {
                Map.MoveCharacter(player, destX, destY);
                GameClient.NetworkClient.Move(direction);
            }
        }

        public void OnSetYourCharacter(object packet)
        {
            var p = (SetYourCharacterPacket)packet;

            this.player = this.Map.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);

            this.uiRoot.Player = this.player;
        }

        public void OnPing(object packet)
        {
            GameClient.NetworkClient.Pong();
        }

        public void OnSendCurrentMap(object packet)
        {
            var sendCurrentMapPacket = (SendCurrentMapPacket)packet;
            this.mapNumber = sendCurrentMapPacket.MapNumber;
            this.mapName = sendCurrentMapPacket.MapName;
            LoadMap();
        }

        public void OnSetYourPosition(object packet)
        {
            var p = (SetYourPositionPacket)packet;

            if (Map[player.TileX, player.TileY].Character == player)
                Map[player.TileX, player.TileY].Character = null;

            player.SetPosition(p.MapX, p.MapY);
            Map[player.TileX, player.TileY].Character = player;
            moveDelay = true;
        }

        public void OnWeaponSpeed(object packet)
        {
            var p = (WeaponSpeedPacket)packet;

            weaponSpeed = p.Speed / 1000d;
        }

        public void OnDropWasUnhandled(object dropData)
        {
            if (dropData is ItemSlot)
            {
                var fromSlot = dropData as ItemSlot;

                if (fromSlot.Parent is CharacterWindow)
                {
                    fromSlot.Use();
                }
                else if (fromSlot.Parent is InventoryWindow)
                {
                    GameClient.NetworkClient.DropItem(fromSlot.SlotNumber, fromSlot.StackSize);
                }
            }
        }

        public void OnRightClick(int x, int y)
        {
            int startX = RenderOffsetX();
            int startY = RenderOffsetY();

            Map.OnRightClick(startX, startY, x, y);
        }

        public void OnMakeWindow(object packet)
        {
            var p = (MakeWindowPacket)packet;

            switch (p.WindowFrame)
            {
                case WindowFrames.Vendor:
                    this.uiRoot.AddChild(new VendorWindow(p));
                    break;
            }
        }

        public void OnEndWindow(object packet)
        {
            var p = (EndWindowPacket)packet;

            var window = this.uiRoot.Children.Where(w => w is BaseWindow).Cast<BaseWindow>()
                .FirstOrDefault(w => w.WindowId == p.WindowId);

            window?.EndWindow();
        }
    }
}