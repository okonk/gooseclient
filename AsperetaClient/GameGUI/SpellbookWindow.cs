using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class SpellbookWindow : BaseWindow
    {
        private SpellSlot[] slots;
        private int rows;
        private int columns;

        public event Action<SpellSlot> CastSpell;

        public SpellbookWindow() : base("SpellBook")
        {
            var windim = GameClient.WindowSettings.GetCoords(this.Name, "windim");
            rows = windim.ElementAt(0);
            columns = windim.ElementAt(1);

            var objoff = GameClient.WindowSettings.GetCoords(this.Name, "objoff");

            int offsetX = objoff.ElementAt(0);
            int offsetY = objoff.ElementAt(1);

            var objdim = GameClient.WindowSettings.GetCoords(this.Name, "objdim");
            int slotW = objdim.ElementAt(0);
            int slotH = objdim.ElementAt(1);
            
            slots = new SpellSlot[rows * columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int x = offsetX + c * slotW;
                    int y = offsetY + r * slotH;

                    var slot = new SpellSlot(r * columns + c, x, y, slotW, slotH);
                    slot.DoubleClicked += OnSlotDoubleClicked;
                    this.AddChild(slot);

                    slots[r * columns + c] = slot;
                }
            }

            GameClient.NetworkClient.PacketManager.Listen<SpellbookSlotPacket>(OnSpellbookSlot);
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_s)
                    {
                        this.Hidden = !this.Hidden;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }

        public void OnSpellbookSlot(object packet)
        {
            var p = (SpellbookSlotPacket)packet;

            if (p.SpellName == null)
            {
                slots[p.SlotNumber].Clear();
            }
            else
            {
                slots[p.SlotNumber].SetSlot(p.SpellName, p.Graphic, p.Targetable);
            }
        }

        public void OnSlotDoubleClicked(GuiElement element)
        {
            var slot = (SpellSlot)element;
            if (!slot.IsEmpty)
                this.CastSpell?.Invoke(slot);
        }
    }
}
