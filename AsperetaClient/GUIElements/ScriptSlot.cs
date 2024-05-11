using System;

namespace AsperetaClient
{
    public class ScriptSlot : BaseSlot
    {
        private Action<GuiElement> onUsed;

        public ScriptSlot(int slotNumber, int x, int y, int w, int h) : base(slotNumber, x, y, w, h)
        {
            this.DoubleClicked += OnDoubleClicked;
        }

        private void OnDoubleClicked(GuiElement element)
        {
            this.onUsed?.Invoke(element);
        }

        public void SetScript(string name, int graphicId, Action<GuiElement> onUsed)
        {
            this.Name = name;
            this.Graphic = GameClient.ResourceManager.GetTexture(graphicId, null);
            this.onUsed = onUsed;
        }

        public override void HandleDrop(object data)
        {
        }
    }
}
