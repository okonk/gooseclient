using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class BuffBarWindow : BaseWindow
    {
        public BuffSlot[] slots;

        private int rows;
        private int columns;

        public BuffBarWindow() : base("SpellEffects")
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
            
            slots = new BuffSlot[rows * columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int x = offsetX + c * slotW;
                    int y = offsetY + r * slotH;

                    var slot = new BuffSlot(r * columns + c, x, y, slotW, slotH);
                    slot.DoubleClicked += OnSlotDoubleClicked;
                    this.AddChild(slot);

                    slots[r * columns + c] = slot;
                }
            }

            GameClient.NetworkClient.PacketManager.Listen<BuffBarPacket>(OnBuffBar);
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_F3)
                    {
                        this.Hidden = !this.Hidden;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }

        public void OnBuffBar(object packet)
        {
            var p = (BuffBarPacket)packet;

            if (p.GraphicId == 0 && p.Name == null)
            {
                slots[p.SlotNumber].Clear();
            }
            else
            {
                slots[p.SlotNumber].SetSlot(p.Name, p.GraphicId);
            }
        }

        public void OnSlotDoubleClicked(GuiElement element)
        {
            GameClient.NetworkClient.KillBuff(((BuffSlot)element).SlotNumber);
        }
    }
}
