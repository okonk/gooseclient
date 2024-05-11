using System;
using System.Collections.Generic;
using System.Linq;

namespace AsperetaClient.Scripting.GameState;

public class GameState
{
    public Spellbook Spellbook { get; init; } = new();
}

public class Spellbook
{
    private const int MaxSpells = 30;
    private Spell[] spells = new Spell[MaxSpells];

    public Spellbook()
    {
        GameClient.NetworkClient.PacketManager.Listen<SpellbookSlotPacket>(OnSpellbookSlot);
    }

    public void OnSpellbookSlot(object packet)
    {
        var p = (SpellbookSlotPacket)packet;

        spells[p.SlotNumber] = p.SpellName is null ? null : new Spell(p);
    }

    public Spell Find(string name)
    {
        return spells.FirstOrDefault(s => s?.Name == name);
    }
}

public class Spell
{
    public int SlotNumber { get; private set; }

    public string Name { get; private set; }

    public int Unknown1 { get; private set; }

    public int Unknown2 { get; private set; }

    public bool Targetable { get; private set; }

    public int Graphic { get; private set; }

    public DateTime LastCastTimeUtc { get; private set; }

    public TimeSpan Cooldown { get; private set; }

    public TimeSpan CooldownRemaining => TimeSpan.FromMilliseconds(Math.Max(0, (this.Cooldown - (DateTime.UtcNow - this.LastCastTimeUtc)).TotalMilliseconds));

    public bool CanCast => DateTime.UtcNow - this.LastCastTimeUtc >= this.Cooldown;

    public Spell(SpellbookSlotPacket spellSlotPacket)
    {
        this.SlotNumber = spellSlotPacket.SlotNumber;
        this.Name = spellSlotPacket.SpellName;
        this.Unknown1 = spellSlotPacket.Unknown1;
        this.Unknown2 = spellSlotPacket.Unknown2;
        this.Targetable = spellSlotPacket.Targetable;
        this.Graphic = spellSlotPacket.Graphic;
        this.LastCastTimeUtc = DateTime.UtcNow.AddDays(-5);
        this.Cooldown = TimeSpan.FromSeconds(0);
    }

    public bool Cast(Character target)
    {
        if (!CanCast || target.Erased)
            return false;

        GameClient.NetworkClient.Cast(this.SlotNumber, target.LoginId);
        this.LastCastTimeUtc = DateTime.UtcNow;

        return true;
    }

    public void SetCooldown(TimeSpan cooldown)
    {
        this.Cooldown = cooldown;
    }
}