using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class CharacterWindow : BaseWindow
    {
        private int windowId;
        private ItemSlot[] slots;
        private int rows;
        private int columns;
        private int inventorySlots;

        private Label name;
        private Label guild;
        private Label level;
        private Label className;
        private Label hp;
        private Label mp;
        private Label sp;
        private Label experience;
        private Label gold;
        private Label strength;
        private Label stamina;
        private Label intelligence;
        private Label dexterity;
        private Label ac;
        private Label fire;
        private Label water;
        private Label earth;
        private Label air;
        private Label spirit;

        public CharacterWindow() : base("Character")
        {
            this.windowId = 11;

            var windim = GameClient.WindowSettings.GetCoords(this.Name, "windim");
            rows = windim.ElementAt(0);
            columns = windim.ElementAt(1);

            var objoff = GameClient.WindowSettings.GetCoords(this.Name, "objoff");

            int offsetX = objoff.ElementAt(0);
            int offsetY = objoff.ElementAt(1);

            var objdim = GameClient.WindowSettings.GetCoords(this.Name, "objdim");
            int slotW = objdim.ElementAt(0);
            int slotH = objdim.ElementAt(1);

            var inventorywindim = GameClient.WindowSettings.GetCoords("Inventory", "windim");
            inventorySlots = inventorywindim.ElementAt(0) * inventorywindim.ElementAt(1);
            
            slots = new ItemSlot[rows * columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    var equip = GameClient.WindowSettings.GetCoords(this.Name, $"equip{r * columns + c + 1}");

                    int x = offsetX + equip.ElementAt(0);
                    int y = offsetY + equip.ElementAt(1);

                    var slot = new ItemSlot(inventorySlots + r * columns + c + 1, x, y, slotW, slotH);
                    slot.DoubleClicked += OnSlotDoubleClicked;
                    this.AddChild(slot);

                    slots[r * columns + c] = slot;
                }
            }

            name = CreateLabel("name");
            guild = CreateLabel("guild");
            level = CreateLabel("level");
            className = CreateLabel("class");
            hp = CreateLabel("hp");
            mp = CreateLabel("mp");
            sp = CreateLabel("sp");
            experience = CreateLabel("tnl");
            gold = CreateLabel("gold");
            strength = CreateLabel("strength");
            stamina = CreateLabel("stamina");
            intelligence = CreateLabel("intelligence");
            dexterity = CreateLabel("dexterity");
            ac = CreateLabel("ac");
            fire = CreateLabel("fire");
            water = CreateLabel("water");
            earth = CreateLabel("earth");
            air = CreateLabel("air");
            spirit = CreateLabel("spirit");

            var cboff = GameClient.WindowSettings.GetCoords(this.Name, "cboff");
            var cbdim = GameClient.WindowSettings.GetCoords(this.Name, "cbdim");
            var closeButton = new Button(cboff.ElementAt(0), cboff.ElementAt(1), cbdim.ElementAt(0), cbdim.ElementAt(1));
            closeButton.Clicked += (b) => { this.Hidden = true; };
            this.AddChild(closeButton);

            GameClient.NetworkClient.PacketManager.Listen<StatusInfoPacket>(OnStatusInfo);
            GameClient.NetworkClient.PacketManager.Listen<ExperienceBarPacket>(OnExperienceBar);
            GameClient.NetworkClient.PacketManager.Listen<WindowLinePacket>(OnWindowLine);
        }

        private Label CreateLabel(string settingKey)
        {
            var coord = GameClient.WindowSettings.GetCoords(this.Name, settingKey);
            var label = new Label(coord.ElementAt(0) + 6, coord.ElementAt(1), Colour.Yellow, "");
            this.AddChild(label);
            return label;
        }

        public void OnStatusInfo(object packet)
        {
            var p = (StatusInfoPacket)packet;

            name.Value = GameClient.UserName ?? "";
            guild.Value = p.GuildName;
            level.Value = p.Level.ToString();
            className.Value = p.ClassName;
            hp.Value = $"{p.CurrentHP}/{p.MaxHP}";
            mp.Value = $"{p.CurrentMP}/{p.MaxMP}";
            sp.Value = $"{p.CurrentSP}/{p.MaxSP}";
            gold.Value = p.Gold.ToString();
            strength.Value = p.Strength.ToString();
            stamina.Value = p.Stamina.ToString();
            intelligence.Value = p.Intelligence.ToString();
            dexterity.Value = p.Dexterity.ToString();
            ac.Value = p.ArmorClass.ToString();
            fire.Value = p.FireResist.ToString();
            water.Value = p.WaterResist.ToString();
            earth.Value = p.EarthResist.ToString();
            air.Value = p.AirResist.ToString();
            spirit.Value = p.SpiritResist.ToString();
        }

        public void OnExperienceBar(object packet)
        {
            var p = (ExperienceBarPacket)packet;

            experience.Value = p.Experience.ToString();
            name.Value = GameClient.UserName ?? "";
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_c)
                    {
                        this.Hidden = !this.Hidden;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }

        public void OnSlotDoubleClicked(GuiElement element)
        {
            GameClient.NetworkClient.Use(((ItemSlot)element).SlotNumber);
        }

        public void OnWindowLine(object packet)
        {
            var p = (WindowLinePacket)packet;

            if (p.WindowId != this.windowId) return;

            if (p.ItemId == 0)
            {
                slots[p.LineNumber].Clear();
            }
            else
            {
                slots[p.LineNumber].SetSlot(p.ItemId, p.Text, p.StackSize, p.GraphicId, new Colour(p.GraphicR, p.GraphicG, p.GraphicB, p.GraphicA));
            }
        }
    }
}
