using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class ItemSlot : GuiElement
    {
        Texture itemGraphic;
        Colour colour;

        public ItemSlot(int x, int y, int w, int h) : base(x, y, w, h)
        {

        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            if (itemGraphic == null) return;

            itemGraphic.Render(X + xOffset, Y + yOffset, colour);
        }

        public void Clear()
        {
            itemGraphic = null;
        }

        public void SetSlot(InventorySlotPacket slotPacket)
        {
            itemGraphic = GameClient.ResourceManager.GetTexture(slotPacket.GraphicId);
            colour = new Colour(slotPacket.GraphicR, slotPacket.GraphicG, slotPacket.GraphicB, slotPacket.GraphicA);
        }
    }
}
