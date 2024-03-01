using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.Data.OpenUO;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Network;

internal class EnhancedPacketHandler
{
    static EnhancedPacketHandler()
    {
        Handler.Add(0, FeaturePacket);
        Handler.Add(1, SettingsPacket);
        Handler.Add(2, DefaultMovementSpeedPacket);
        Handler.Add(3, EnhancedPotionMacrosPacket);
        Handler.Add(4, GeneralSettings);
        Handler.Add(5, EnhancedSpellbookSettings);

        
        
        Handler.Add(151, ActiveAbilityCompletePacket);
        Handler.Add(150, ActiveAbilityUpdatePacket);
        
        Handler.Add(103, SetProfileOption);
        Handler.Add(102, ExtraTargetInformationPacket);
        Handler.Add(101, RollingTextSimple);
        Handler.Add(100, RollingText);
        
        Handler.Add(110, CooldownTimerPacket);
        
        Handler.Add(120, PlayableAreaPacket);
        Handler.Add(121, HighlightedAreasPacket);
        
        //Handler.Add(200, EnhancedGraphicEffect);
        Handler.Add(201, EnhancedEffectMultiPoint);
        Handler.Add(200, EnhancedGraphicEffect);
        Handler.Add(205, EnhancedSpellbookContent);
        Handler.Add(206, CloseContainer);
    }

    private static void PacketTemplate(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static void CloseContainer(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void EnhancedSpellbookContent(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                Item spellbook = world.GetOrCreateItem(p.ReadUInt32BE());
                spellbook.Graphic = p.ReadUInt16BE();
                spellbook.Clear();
                ushort type = p.ReadUInt16BE();

                for (int j = 0; j < 2; j++)
                {
                    uint spells = 0;

                    for (int i = 0; i < 4; i++)
                    {
                        spells |= (uint)(p.ReadUInt8() << (i * 8));
                    }

                    for (int i = 0; i < 32; i++)
                    {
                        if ((spells & (1 << i)) != 0)
                        {
                            ushort cc = (ushort)(j * 32 + i + 1);
                            // FIXME: should i call Item.Create ?
                            Item spellItem = Item.Create(world, cc); // new Item()
                            spellItem.Serial = cc;
                            spellItem.Graphic = 0x1F2E;
                            spellItem.Amount = cc;
                            spellItem.Container = spellbook;
                            spellbook.PushToBack(spellItem);
                        }
                    }
                }
                
                for (int j = 0; j < 2; j++)
                {
                    uint spells = 0;

                    for (int i = 0; i < 4; i++)
                    {
                        spells |= (uint)(p.ReadUInt8() << (i * 8));
                    }

                    for (int i = 0; i < 32; i++)
                    {
                        if ((spells & (1 << i)) != 0)
                        {
                            ushort cc = (ushort)(j * 32 + i + 1);
                            cc += 64;
                            // FIXME: should i call Item.Create ?
                            Item spellItem = Item.Create(world, cc); // new Item()
                            spellItem.Serial = cc;
                            spellItem.Graphic = 0x1F2E;
                            spellItem.Amount = cc;
                            spellItem.Container = spellbook;
                            spellbook.PushToBack(spellItem);
                        }
                    }
                }

                //spellbook.

                UIManager.GetGump<BaseSpellbookGump>(spellbook)?.RequestUpdateContents();

                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void FeaturePacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                GameActions.SendOpenUOHello();
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void EnhancedSpellbookSettings(World world, ref StackDataReader p, int version)
    {
        SpellsMagery.Clear();
        switch (version)
        {
            case 0:
            {
                int count = p.ReadInt32BE();

                for (int i = 0; i < count; i++)
                {
                    ushort id = p.ReadUInt16BE();
                    byte circle = p.ReadUInt8();
                    ushort gumpid = p.ReadUInt16BE();
                    string name;
                    string tooltips;
                    string powerwords;

                    if (p.ReadBool())
                    {
                        name = ClilocLoader.Instance.GetString(p.ReadInt32BE());
                    }
                    else
                    {
                        ushort len = p.ReadUInt16BE();
                        name = p.ReadASCII(len);
                    }
                    
                    if (p.ReadBool())
                    {
                        tooltips = ClilocLoader.Instance.GetString(p.ReadInt32BE());
                    }
                    else
                    {
                        ushort len = p.ReadUInt16BE();
                        tooltips = p.ReadASCII(len);
                    }
                    
                    if (p.ReadBool())
                    {
                        powerwords = ClilocLoader.Instance.GetString(p.ReadInt32BE());
                    }
                    else
                    {
                        ushort len = p.ReadUInt16BE();
                        powerwords = p.ReadASCII(len);
                    }

                    List<Reagents> regs = new List<Reagents>();
                    string[] reags = new string[p.ReadUInt8()];
                    for (int j = 0; j < reags.Length; j++)
                    {
                        int cliloc = p.ReadInt32BE();
                        reags[j] = ClilocLoader.Instance.GetString(cliloc);//reag;

                        switch (cliloc)
                        {
                            case 1015004: regs.Add(Reagents.Bloodmoss); break;
                            case 1015016: regs.Add(Reagents.Nightshade); break; 
                            case 1015021: regs.Add(Reagents.Garlic); break;
                            case 1015009: regs.Add(Reagents.Ginseng); break;
                            case 1015013: regs.Add(Reagents.MandrakeRoot); break;
                            case 1015007: regs.Add(Reagents.SpidersSilk); break;
                            case 1044359: regs.Add(Reagents.SulfurousAsh); break;
                            case 1015001: regs.Add(Reagents.BlackPearl); break;
                        }
                    }

                    TargetType targetFlags = (TargetType) p.ReadUInt8();
                    SpellsMagery.SetSpell(id, new SpellDefinition(name, circle, id, gumpid, tooltips, powerwords, targetFlags, reags, regs.ToArray() ));
                }

                var buttons = UIManager.Gumps.OfType<UseSpellButtonGump>().ToList();
                foreach (var gump in buttons)
                    gump.Rebuild();
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void EnhancedGraphicEffect(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                //if (world.PlayableArea == null)
                //{
                //    return;
                //}
                GraphicEffectType type = (GraphicEffectType)p.ReadUInt8();

                uint source = p.ReadUInt32BE();
                uint target = p.ReadUInt32BE();
                ushort graphic = p.ReadUInt16BE();
                ushort srcX = p.ReadUInt16BE();
                ushort srcY = p.ReadUInt16BE();
                sbyte srcZ = p.ReadInt8();
                ushort targetX = p.ReadUInt16BE();
                ushort targetY = p.ReadUInt16BE();
                sbyte targetZ = p.ReadInt8();
                byte speed = p.ReadUInt8();
                ushort duration = p.ReadUInt8();
                short fixedDirection = p.ReadInt16BE();
                bool doesExplode = p.ReadBool();
                ushort hue = 0;
                GraphicEffectBlendMode blendmode = 0;
                hue = (ushort)p.ReadUInt32BE();
                blendmode = (GraphicEffectBlendMode)(p.ReadUInt32BE() % 7);
                ushort effect = p.ReadUInt16BE();
                ushort explodeEffect = p.ReadUInt16BE();
                ushort explodeSound = p.ReadUInt16BE();
                uint serial = p.ReadUInt32BE();
                byte layer = p.ReadUInt8();
                ushort unknown = p.ReadUInt16BE();
                TimeSpan durationTimeSpan = TimeSpan.FromMilliseconds(p.ReadUInt32BE());
                short spinning = p.ReadInt16BE();

                /*if (speed > 7)
                {
                    speed = 7;
                }*/

                world.SpawnEffect
                (
                    type,
                    source,
                    target,
                    graphic,
                    hue,
                    srcX,
                    srcY,
                    srcZ,
                    targetX,
                    targetY,
                    targetZ,
                    speed,
                    duration,
                    fixedDirection,
                    doesExplode,
                    false,
                    blendmode, durationTimeSpan, spinning, null
                );
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static void EnhancedEffectMultiPoint(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                if (world.Player == null)
                {
                    return;
                }
                GraphicEffectType type = (GraphicEffectType)p.ReadUInt8();

                uint source = p.ReadUInt32BE();
                ushort graphic = p.ReadUInt16BE();
                ushort srcX = p.ReadUInt16BE();
                ushort srcY = p.ReadUInt16BE();
                sbyte srcZ = p.ReadInt8();
                byte speed = p.ReadUInt8();
                ushort duration = p.ReadUInt8();
                short fixedDirection = p.ReadInt16BE();
                bool doesExplode = p.ReadBool();
                ushort hue = 0;
                GraphicEffectBlendMode blendmode = 0;
                hue = (ushort)p.ReadUInt32BE();
                blendmode = (GraphicEffectBlendMode)(p.ReadUInt32BE() % 7);
                ushort effect = p.ReadUInt16BE();
                ushort explodeEffect = p.ReadUInt16BE();
                ushort explodeSound = p.ReadUInt16BE();
                uint serial = p.ReadUInt32BE();
                byte layer = p.ReadUInt8();
                ushort unknown = p.ReadUInt16BE();
                short spinning = p.ReadInt16BE();
                ushort pointCount = p.ReadUInt16BE();

                List<Tuple<TimeSpan, Vector3>> points = new List<Tuple<TimeSpan, Vector3>>();

                for (int i = 0; i < pointCount; i++)
                {
                    TimeSpan durationTimeSpan = TimeSpan.FromMilliseconds(p.ReadUInt32BE());
                    ushort targetX = p.ReadUInt16BE();
                    ushort targetY = p.ReadUInt16BE();
                    sbyte targetZ = p.ReadInt8();
                    points.Add(new Tuple<TimeSpan, Vector3>(durationTimeSpan, new Vector3(targetX, targetY, targetZ)));
                }
                /*
                 
                
                 if (speed > 7)
                {
                    speed = 7;
                }*/

                world.SpawnEffect
                (
                    type,
                    source,
                    0,
                    graphic,
                    hue,
                    srcX,
                    srcY,
                    srcZ,
                    0,
                    0,
                    0,
                    speed,
                    duration,
                    fixedDirection,
                    doesExplode,
                    false,
                    blendmode, TimeSpan.Zero, spinning, points
                );
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void SetProfileOption(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                ushort cmd = p.ReadUInt16BE();
                ushort len = p.ReadUInt16BE();
                string name = p.ReadASCII(len);
                switch (cmd)
                {
                    case 0:
                    {
                        bool val = p.ReadBool();
                        var prop = typeof(Profile).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                        try
                        {
                            prop.SetValue(ProfileManager.CurrentProfile, val);
                            ProfileManager.CurrentProfile?.Save(world, ProfileManager.ProfilePath);
                        }
                        catch { }
                        break;
                    }
                    case 1:
                    {
                        int val = p.ReadInt32BE();
                        var prop = typeof(Profile).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                        try
                        {
                            prop.SetValue(ProfileManager.CurrentProfile, val);
                            ProfileManager.CurrentProfile?.Save(world, ProfileManager.ProfilePath);
                        }
                        catch { }
                        break;
                    }
                }
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    
    private static void RollingTextSimple(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                uint serial = p.ReadUInt32BE();
                Entity entity = world.Get(serial);
                uint source = p.ReadUInt32BE();
                ushort hue = p.ReadUInt16BE();

                uint cliloc = p.ReadUInt32BE();
                bool neg = p.ReadBool();
                long val = p.ReadUInt32BE() * (neg ? -1 : 1);
                
                var text = ClilocLoader.Instance.Translate((int) cliloc, $"{(!neg ? "+" : "")}{val}") + " ";

                bool display = true;

                switch (cliloc)
                {
                    case 8011100:
                        if (!ProfileManager.CurrentProfile.RollingTextHitPoints)
                            display = false;
                        else if (ProfileManager.CurrentProfile.RollingTextHitPointsOverride)
                        {
                            if (neg) hue = ProfileManager.CurrentProfile.RollingTextHitPointsOverrideLoss;
                            else hue = ProfileManager.CurrentProfile.RollingTextHitPointsOverrideGain;
                        }
                        break;
                    case 8011101:
                        if (!ProfileManager.CurrentProfile.RollingTextStamina)
                            display = false;
                        else if (ProfileManager.CurrentProfile.RollingTextStaminaOverride)
                        {
                            if (neg) hue = ProfileManager.CurrentProfile.RollingTextStaminaOverrideLoss;
                            else hue = ProfileManager.CurrentProfile.RollingTextStaminaOverrideGain;
                        }

                        break;
                    case 8011102:
                        if (!ProfileManager.CurrentProfile.RollingTextMana)
                            display = false;
                        else if (ProfileManager.CurrentProfile.RollingTextManaOverride)
                        {
                            if (neg) hue = ProfileManager.CurrentProfile.RollingTextManaOverrideLoss;
                            else hue = ProfileManager.CurrentProfile.RollingTextManaOverrideGain;
                        }

                        break;
                    case 8011103: 
                    case 8011104: 
                    case 8011105:
                    case 8011106:
                        if (!ProfileManager.CurrentProfile.RollingTextStat)
                            display = false;
                        else if (ProfileManager.CurrentProfile.RollingTextStatOverride)
                        {
                            if (neg) hue = ProfileManager.CurrentProfile.RollingTextStatOverrideLoss;
                            else hue = ProfileManager.CurrentProfile.RollingTextStatOverrideGain;
                        }

                        break;

                    default:
                    {
                        
                        if (!ProfileManager.CurrentProfile.RollingTextOther)
                            display = false;
                        else if (ProfileManager.CurrentProfile.RollingTextOtherOverride)
                        {
                            if (neg) hue = ProfileManager.CurrentProfile.RollingTextOtherOverrideLoss;
                            else hue = ProfileManager.CurrentProfile.RollingTextOtherOverrideGain;
                        }

                        break;
                    }
                }
                
                if (display && entity != null && text.Length > 0)
                {
                    world.WorldTextManager.AddRollingText(entity, hue, 3, text);
                }
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static void RollingText(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                uint serial = p.ReadUInt32BE();
                Entity entity = world.Get(serial);
                uint source = p.ReadUInt32BE();
                ushort hue = p.ReadUInt16BE();
                byte font = p.ReadUInt8();
                int amount = p.ReadUInt8();

                string text = "";

                for (int i = 0; i < amount; i++)
                {
                    ushort len = p.ReadUInt16BE();
                    uint cliloc = p.ReadUInt32BE();
                    
                    string arguments = null;
                    
                    arguments = p.ReadUnicodeLE((int)len);
                    p.Skip(2);
                    text += ClilocLoader.Instance.Translate((int) cliloc, arguments) + " ";
                }
                bool display = true;

                if (!ProfileManager.CurrentProfile.RollingTextOther)
                    display = false;
                else if (ProfileManager.CurrentProfile.RollingTextOtherOverride)
                {
                    hue = ProfileManager.CurrentProfile.RollingTextOtherOverrideGain;
                }
                
                if (entity != null && text.Length > 0)
                {
                    world.WorldTextManager.AddRollingText(entity, hue, font, text);
                }
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }


    
    private static void ExtraTargetInformationPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                var cursorID = p.ReadUInt32BE();
                var type = p.ReadUInt16BE();

                switch (type)
                {
                    case 2:
                    {
                        ushort posX = p.ReadUInt16BE();
                        ushort posY = p.ReadUInt16BE();
                        ushort posZ = p.ReadUInt16BE();
                        uint preview = p.ReadUInt32BE();
                        ushort hue = p.ReadUInt16BE();
                        world.TargetManager.SetExtra
                        (
                            cursorID, type, new Vector3(posX, posY, posZ), preview, hue
                        );

                        break;
                    }

                    case 3:
                    {
                        ushort posX = p.ReadUInt16BE();
                        ushort posY = p.ReadUInt16BE();
                        short posZ = p.ReadInt16BE();
                        uint preview = p.ReadUInt32BE();
                        ushort hue = p.ReadUInt16BE();
                        world.TargetManager.SetExtra
                        (
                            cursorID, type, new Vector3(posX, posY, posZ), preview, hue
                        );
                        
                        world.TargetManager.TargetingState = CursorTarget.MultiPlacement;
                        world.TargetManager.MultiTargetInfo = new MultiTargetInfo
                        (
                            (ushort)preview,
                            posX,
                            posY,
                            posZ,
                            hue
                        );
                        
                        break;
                    }
                    default:
                    {
                        ushort range = p.ReadUInt16BE();
                        uint preview = p.ReadUInt32BE();
                        ushort hue = p.ReadUInt16BE();

                        world.TargetManager.SetExtra
                        (
                            cursorID, type, range, preview,
                            hue
                        );
                        break;
                    }
                }
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static void ExtraTargetInformationClearPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                world.TargetManager.SetExtra(
                    p.ReadUInt32BE(), 
                    p.ReadUInt16BE(), 
                    p.ReadUInt16BE(),
                    0,
                    0);
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static void CooldownTimerPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                //add cooldown timer

                int itemID = (int)p.ReadUInt32BE();
                ushort itemHue = p.ReadUInt16BE();
                float timeInSeconds = p.ReadUInt32BE() / 100f;
                int offsetX = (int)p.ReadUInt16BE();
                int offsetY = (int)p.ReadUInt16BE();

                int textLength = p.ReadUInt16BE();
                string text = null;

                if (textLength > 0)
                    text = p.ReadASCII(textLength);
                ushort circleHue = p.ReadUInt16BE();
                ushort textHue = p.ReadUInt16BE();
                ushort countdownHue = p.ReadUInt16BE();

                world.Player.CooldownTimers.Add(new CooldownTimer(
                                                    itemID,
                                                    itemHue,
                                                    timeInSeconds,
                                                    offsetX,
                                                    offsetY,
                                                    text,
                                                    circleHue,
                                                    textHue,
                                                    countdownHue
                                                ));

                CooldownTimersGump gump = UIManager.GetGump<CooldownTimersGump>();
                gump?.RequestUpdateContents();
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static void SpecialHealthBarPacket(World world, ref StackDataReader p)
    {
        int version = p.ReadUInt16BE();

        switch (version)
        {
            case 0:
            {
                int operation = p.ReadUInt16BE();

                    switch (operation)
                    {
                        case 1:
                        {
                            uint serial = p.ReadUInt32BE();
                            short offX = p.ReadInt16BE();
                            short offY = p.ReadInt16BE();
                            double scaleWidth = p.ReadUInt32BE() / 10000d;
                            double scaleHeight = p.ReadUInt32BE() / 10000d;
                            ushort bgHue = p.ReadUInt16BE();
                            ushort fgOff = p.ReadUInt16BE();
                            ushort fgHue = p.ReadUInt16BE();
                            ushort locID = p.ReadUInt16BE();
                            bool hideWhenFull = p.ReadBool();

                            Entity source = world.Get(serial);

                            if (source != null)
                            {
                                World.HealthBarEntities.Remove(source);
                                World.HealthBarEntities.Add
                                (
                                    source,
                                    new SpecialHealthBarData
                                    (
                                        source, new Point(offX, offY), scaleWidth, scaleHeight,
                                        bgHue, (SpecialHealthBarData.SpecialStatusBarID)fgOff, fgHue, (SpecialHealthBarData.SpecialStatusLocation)locID,
                                        hideWhenFull
                                    )
                                );
                            }

                            break;
                        }

                        case 2:
                        {
                            uint serial = p.ReadUInt32BE();
                            Entity source = world.Get(serial);

                            if (source != null)
                            {
                                World.HealthBarEntities.Remove(source);
                            }

                            break;
                        }
                    }
                    
                    break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void PlayableAreaPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                ushort operation = p.ReadUInt16BE();

                switch (operation)
                {
                    case 0:
                    {
                        world.PlayableArea = null;
                        break;
                    }
                    case 1:
                    {
                        bool blocking = p.ReadBool();
                        ushort hue = p.ReadUInt16BE();
                        ushort count = p.ReadUInt16BE();
                        List<Rectangle> areas = new List<Rectangle>();

                        for (int i = 0; i < count; i++)
                        {
                            ushort x = p.ReadUInt16BE();
                            ushort y = p.ReadUInt16BE();
                            ushort w = p.ReadUInt16BE();
                            ushort h = p.ReadUInt16BE();
                            areas.Add(new Rectangle(x, y, w, h));
                        }

                        world.PlayableArea = new PlayableAreaInformation(blocking, hue, areas);

                        break;
                    }
                }
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void HighlightedAreasPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                ushort subcmd = p.ReadUInt16BE();

                switch (subcmd)
                {
                    case 1: //add 
                    {
                        ushort x = p.ReadUInt16BE();
                        ushort y = p.ReadUInt16BE();
                        ushort w = p.ReadUInt16BE();
                        ushort h = p.ReadUInt16BE();

                        ushort hue = p.ReadUInt16BE();
                        HighlightType type = (HighlightType) p.ReadUInt16BE();
                        byte priority = p.ReadUInt8();

                        UIManager.HighlightedAreas.Add(new HighlightedArea(new Rectangle(x,y,w,h), hue, type, priority));
                        UIManager.HighlightedAreas.Sort((b1,b2) => b2.Priority.CompareTo(b1.Priority));
                        break;
                    }

                    case 2: // remove
                    {
                        ushort x = p.ReadUInt16BE();
                        ushort y = p.ReadUInt16BE();
                        ushort w = p.ReadUInt16BE();
                        ushort h = p.ReadUInt16BE();

                        ushort hue = p.ReadUInt16BE();
                        HighlightType type = (HighlightType) p.ReadUInt16BE();
                        byte priority = p.ReadUInt8();
                        var rect = new Rectangle(x, y, w, h);

                        foreach (var item in UIManager.HighlightedAreas.ToList())
                        {
                            if (item.Rectangle == rect && item.Hue == hue && item.Type == type && item.Priority == priority)
                                UIManager.HighlightedAreas.Remove(item);
                        }
                            
                        break;
                    }

                    case 3: //clear
                    {
                        UIManager.HighlightedAreas.Clear();
                        break;
                    }
                }
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void ActiveAbilityUpdatePacket(World world, ref StackDataReader p, int version)
    {
        if (!world.Settings.GeneralFlags.EnableEnhancedAbilities)
            return;
        switch (version)
        {
            case 0:
            {
                //active abilities update for single item
                ushort row = p.ReadUInt16BE();
                ushort slot = p.ReadUInt16BE();
                
                TimeSpan cooldown = TimeSpan.FromMilliseconds(p.ReadUInt32BE());
                TimeSpan cooldownRemaining = TimeSpan.FromMilliseconds(p.ReadUInt32BE());
                DateTime cooldownEnd = DateTime.UtcNow + cooldownRemaining;
                short hue = (short) p.ReadUInt16BE();
                short charge = (short) p.ReadUInt16BE();
                bool useNextMove = p.ReadUInt16BE() == 0x1 ? true : false;
                var inUseMS = p.ReadUInt32BE();
                var inUseUntilMS = p.ReadUInt32BE();
                DateTime inUseEnd = DateTime.MinValue;
                DateTime inUseStart = DateTime.MinValue;
                if (inUseMS > 0 && inUseUntilMS > 0)
                {
                    TimeSpan inUseTotal = TimeSpan.FromMilliseconds(inUseMS);
                    TimeSpan inUseRemaining = TimeSpan.FromMilliseconds(inUseUntilMS);
                    inUseEnd = DateTime.UtcNow + inUseRemaining;
                    inUseStart = inUseEnd - inUseTotal;
                }

                if (row >= EnhancedAbilitiesGump.EnhancedAbilities.Count)
                {
                    Console.WriteLine($"Recieved Ability info for an ability I don't have. Ability: {row} {slot}");
                    return;
                }
                
                if (slot >= EnhancedAbilitiesGump.EnhancedAbilities[row].Abilities.Count)
                {
                    Console.WriteLine($"Recieved Ability info for an ability I don't have. Ability: {row} {slot}");
                    return;
                }

                var a = EnhancedAbilitiesGump.EnhancedAbilities[row].Abilities[slot];
                a.Cooldown = cooldown;
                a.CooldownStart = cooldownEnd - cooldown;
                a.CooldownEnd = cooldownEnd;
                a.Hue = hue;
                a.Charges = charge;
                a.UseNextMove = useNextMove;
                a.InUseStart = inUseStart;
                a.InUseUntil = inUseEnd;
                
                var gump = UIManager.GetGump<EnhancedAbilitiesGump>();

                if (gump == null)
                {
                    UIManager.Add(gump = new EnhancedAbilitiesGump(world));
                }

                gump.Updated();
                
                break;
            }
        }
    }

    private static void ActiveAbilityCompletePacket(World world, ref StackDataReader p, int version)
    {
        if (!world.Settings.GeneralFlags.EnableEnhancedAbilities)
            return;
        switch (version)
        {
            case 0:
            {
                ushort rows = p.ReadUInt16BE();
                if (rows == 0)
                {
                    
                    UIManager.GetGump<EnhancedAbilitiesGump>()?.Dispose();

                    break;
                }
                var list = new List<ActiveAbilityObject>();
                for (int i = 0; i < rows; i++)
                {
                    
                    uint serial = p.ReadUInt32BE();
                    //ushort len = p.ReadUInt16BE();
                    
                    string name = p.ReadASCII(p.ReadUInt16BE())?.Replace('\r', '\n');
                    //string name = p.ReadUnicodeLE((int)len);
                    ushort slots = p.ReadUInt16BE();

                    var obj = new ActiveAbilityObject() { Name = name, Serial = (int)serial, Abilities = new List<ActiveAbility>() };
                    list.Add(obj);

                    for (int j = 0; j < slots; j++)
                    {
                        int abilityName = (int)p.ReadUInt32BE();
                        int abilityDescription = (int)p.ReadUInt32BE();
                        int count = p.ReadUInt8();

                        string args = "";

                        for (int k = 0; k < count; k++)
                        {
                            uint color = p.ReadUInt32BE();
                            float val = ((float)p.ReadUInt32BE()) / 1000f;
                            args += $"<BASEFONT COLOR=#\"{color:x6}>{val}<BASEFONT COLOR=#FFFFFF>";

                            if (k < count - 1)
                                args += "\t";
                        }
                        
                        string description = 
                             ClilocLoader.Instance.Translate((int) abilityDescription, args );
                        ;
                        
                        int gumpid = (int) p.ReadUInt32BE();
                        TimeSpan cooldown = TimeSpan.FromSeconds(p.ReadUInt32BE() / 1000d);
                        TimeSpan cooldownRemaining = TimeSpan.FromSeconds(p.ReadUInt32BE() / 1000d);
                        DateTime cooldownEnd = DateTime.UtcNow + cooldownRemaining;
                        short hue = (short) p.ReadUInt16BE();
                        short charge = (short) p.ReadUInt16BE();
                        bool useNextMove = p.ReadUInt16BE() == 0x1 ? true : false;
                        
                        var inUseMS = p.ReadUInt32BE();
                        var inUseUntilMS = p.ReadUInt32BE();
                        DateTime inUseEnd = DateTime.MinValue;
                        DateTime inUseStart = DateTime.MinValue;
                        if (inUseMS > 0 && inUseUntilMS > 0)
                        {
                            TimeSpan inUseTotal = TimeSpan.FromSeconds(inUseMS / 1000d);
                            TimeSpan inUseRemaining = TimeSpan.FromSeconds(inUseUntilMS / 1000d);
                            inUseEnd = DateTime.UtcNow + inUseRemaining;
                            inUseStart = inUseEnd - inUseTotal;
                        }

                        
                        obj.Abilities.Add(new ActiveAbility()
                        {
                            Name = ClilocLoader.Instance.GetString(abilityName), 
                            Description = description,
                            IconLarge = gumpid,
                            Cooldown = cooldown,
                            CooldownStart = cooldownEnd - cooldown,
                            CooldownEnd = cooldownEnd,
                            Hue = hue,
                            Charges = charge,
                            UseNextMove = useNextMove,
                            InUseStart = inUseStart,
                            InUseUntil = inUseEnd,
                        });
                    }
                }

                EnhancedAbilitiesGump.EnhancedAbilities = list;

                var gump = UIManager.GetGump<EnhancedAbilitiesGump>();

                if (gump == null)
                {
                    UIManager.Add(gump = new EnhancedAbilitiesGump(world));
                }

                gump.Updated();
                break;
            }
        }
    }

    private static void EnhancedPotionMacrosPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                ushort count = p.ReadUInt16BE();
                for (int i = 0; i < count; i++)
                {
                    ushort id = p.ReadUInt16BE();
                    int cliloc = p.ReadInt32BE();
                    world.Settings.Potions.Add(new PotionDefinition()
                    {
                        ID = id,
                        Name = StringHelper.CapitalizeAllWords(ClilocLoader.Instance.Translate(cliloc))
                    });
                }
                
                count = p.ReadUInt16BE();
                for (int i = 0; i < count; i++)
                {
                    ushort id = p.ReadUInt16BE();
                    ushort len = p.ReadUInt16BE();
                    string name = p.ReadASCII(len);
                    world.Settings.Potions.Add(new PotionDefinition()
                    {
                        ID = id,
                        Name = StringHelper.CapitalizeAllWords(name)
                    });
                }
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static void DefaultMovementSpeedPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                world.Settings.MovementSettings.TurnDelay = p.ReadUInt16BE();
                world.Settings.MovementSettings.MoveSpeedWalkingUnmounted = p.ReadUInt16BE();
                world.Settings.MovementSettings.MoveSpeedRunningUnmounted = p.ReadUInt16BE();
                world.Settings.MovementSettings.MoveSpeedWalkingMounted = p.ReadUInt16BE();
                world.Settings.MovementSettings.MoveSpeedRunningMounted = p.ReadUInt16BE();
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    
    private static void GeneralSettings(World world, ref StackDataReader p, int version)
    {
        world.Settings.GeneralSettings = new GeneralSettings();
        
        switch (version)
        {
            case 0:
            {
                if (p.ReadBool())
                {
                    int len = p.ReadUInt16BE();
                    world.Settings.GeneralSettings.StoreOverride = p.ReadASCII(len);
                }
                TopBarGump.Create(world);
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }

    private static void SettingsPacket(World world, ref StackDataReader p, int version)
    {
        switch (version)
        {
            case 0:
            {
                int length = (int)p.ReadInt32BE();
                byte[] clientOptions = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    clientOptions[i] = p.ReadUInt8();
                }

                length = (int)p.ReadInt32BE();
                byte[] generalOptions = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    generalOptions[i] = p.ReadUInt8();
                }

                length = (int)p.ReadInt32BE();
                byte[] macroOptions = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    macroOptions[i] = p.ReadUInt8();
                }

                var props = typeof(SettingGeneralFlags).GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (var prop in props)
                {
                    var id = GetID(prop);

                    if (id > -1)
                    {
                        var isOn = IsSettingOn(generalOptions, id);
                        Console.WriteLine($"{prop.Name} => {isOn}");
                        prop.SetValue(world.Settings.GeneralFlags, isOn);
                    }
                }

                props = typeof(SettingsMacrosFlags).GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (var prop in props)
                {
                    var id = GetID(prop);
                    if (id > -1)
                    {
                        var isOn = IsSettingOn(macroOptions, id);
                        Console.WriteLine($"{prop.Name} => {isOn}");
                        prop.SetValue(world.Settings.MacroFlags, isOn);
                    }
                }

                props = typeof(SettingOptionFlags).GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (var prop in props)
                {
                    var id = GetID(prop);
                    if (id > -1)
                    {
                        var isOn = IsSettingOn(clientOptions, id);
                        Console.WriteLine($"{prop.Name} => {isOn}");
                        prop.SetValue(world.Settings.ClientOptionFlags, isOn);
                    }
                }
                TopBarGump.Create(world);
                
                MacroControl.GenerateNames(world);
                break;
            }
            default: InvalidVersionReceived( ref p ); break;
        }
    }
    
    private static bool IsSettingOn(byte[] settings, int id)
    {
        var pos = id / 8;
        var bit = id % 8;
        var val = settings[pos] & (1 << bit);

        if (val != 0)
            return true;
        return false;
    }
    private static int GetID(FieldInfo prop)
    {
        object[] attrs = prop.GetCustomAttributes(typeof(OptionIDAttribute), false);

        return attrs.Length > 0 ? (attrs[0] as OptionIDAttribute).ID : -1;
    }

    
    public static void OpenUOEnhancedRx(World world, ref StackDataReader p)
    {
        ushort id = p.ReadUInt16BE();
        ushort ver = p.ReadUInt16BE();
        Handler.HandlePacket(id, world, ref p, ver);
    }
    
    public delegate void EnhancedOnPacketBufferReader(World world, ref StackDataReader p, int version);
    public static EnhancedPacketHandler Handler { get; } = new EnhancedPacketHandler();
    
    public void Add(ushort id, EnhancedOnPacketBufferReader handler)
        => _handlers[id] = handler;

    public void HandlePacket(ushort packetID, World world, ref StackDataReader p, int version)
    {
        if (_handlers.ContainsKey(packetID))
        {
            _handlers[packetID].Invoke(world, ref p, version);
        }
        else
        {
            Console.WriteLine($"Received invalid enhanced packet {packetID} (0x{packetID:X}) len={p.Length}");
        }
    }


    private readonly Dictionary<ushort, EnhancedOnPacketBufferReader> _handlers = new Dictionary<ushort, EnhancedOnPacketBufferReader>();

    private static void InvalidVersionReceived(ref StackDataReader p)
    {
        Console.WriteLine($"Version of a packet recieved, likely client is out of date.");
    }
}