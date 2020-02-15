using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class InventoryWindow : BaseWindow
    {
        private ItemSlot[] slots;
        private int rows;
        private int columns;

        public InventoryWindow() : base("Inventory")
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
            
            slots = new ItemSlot[rows * columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int x = offsetX + c * slotW;
                    int y = offsetY + r * slotH;

                    var slot = new ItemSlot(x, y, slotW, slotH);
                    this.AddChild(slot);

                    slots[r * columns + c] = slot;
                }
            }

            GameClient.NetworkClient.PacketManager.Listen<InventorySlotPacket>(OnInventorySlot);
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_i)
                    {
                        this.Hidden = !this.Hidden;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }

        public void OnInventorySlot(object packet)
        {
            var p = (InventorySlotPacket)packet;

            if (p.ItemId == 0)
            {
                slots[p.SlotNumber].Clear();
            }
            else
            {
                slots[p.SlotNumber].SetSlot(p);
            }
        }
    }
}
