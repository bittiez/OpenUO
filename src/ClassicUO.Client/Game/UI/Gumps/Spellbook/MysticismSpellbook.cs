using System;
using System.Collections.Generic;
using ClassicUO.Assets;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class MysticismSpellbookGump : BaseSpellbookGump
{
    public MysticismSpellbookGump(World world, uint item) : base(world, item) { }
    public MysticismSpellbookGump(World world) : base(world) { }
    
    protected override void GetSpellToolTip(out int offset)
    {
        offset = 1095193;
    }
    
    protected override SpellDefinition GetSpellDefinition(int idx) => SpellsMysticism.GetSpell(idx);
    
    protected override void GetBookInfo
    (
        out ushort bookGraphic, out ushort minimizedGraphic, out ushort iconStartGraphic, out int maxSpellsCount, out int spellsOnPage, out int dictionaryPagesCount
    )
    {
        maxSpellsCount = SpellsMysticism.MaxSpellCount;
        bookGraphic = 0x2B32;
        minimizedGraphic = 0x2B30;
        iconStartGraphic = 0x5DC0;
        
        spellsOnPage = Math.Min(maxSpellsCount >> 1, 8);
        dictionaryPagesCount = (int)Math.Ceiling(maxSpellsCount / 8.0f);

        if (dictionaryPagesCount % 2 != 0)
        {
            dictionaryPagesCount++;
        }
    }

    protected override void GetSpellNames(int offset, out string name, out string abbreviature, out string reagents)
    {
        var def = SpellsMysticism.GetSpell(offset + 1);
        name = def.Name;
        abbreviature = def.PowerWords;
        reagents = def.CreateReagentListString("\n");
    }
    protected override void CreateBook()
    {
        _dataBox.Clear();
        _dataBox.WantUpdateSize = true;

        GetBookInfo(
            out ushort bookGraphic,
            out ushort minimizedGraphic,
            out ushort iconStartGraphic,
            out int maxSpellsCount,
            out int spellsOnPage,
            out int dictionaryPagesCount
        );

        int totalSpells = 0;

        Item item = World.Items.Get(LocalSerial);

        if (item == null)
        {
            Dispose();

            return;
        }

        for (LinkedObject i = item.Items; i != null; i = i.Next)
        {
            Item spell = (Item)i;
            int currentCount = spell.Amount;

            if (currentCount > 0 && currentCount <= maxSpellsCount)
            {
                _spells[currentCount - 1] = true;
                totalSpells++;
            }
        }

        int pagesToFill = dictionaryPagesCount >> 1;

        _maxPage = pagesToFill + ((totalSpells + 1) >> 1);

        int currentSpellIndex = 0;

        int spellDone = 0;

        for (int page = 1; page <= pagesToFill; page++)
        {
            for (int j = 0; j < 2; j++)
            {
                
                int indexX = 106;
                int dataX = 62;
                int y = 0;

                if (j % 2 != 0)
                {
                    indexX = 269;
                    dataX = 225;
                }

                Label text = new Label(ResGumps.Index, false, 0x0288, font: 6)
                {
                    X = indexX,
                    Y = 10
                };

                _dataBox.Add(text, page);
                
                int topage = pagesToFill + ((spellDone + 1) >> 1);
                
                
                for (int k = 0; k < spellsOnPage; k++, currentSpellIndex++)
                {
                    if (_spells[currentSpellIndex])
                    {
                        GetSpellNames(
                            currentSpellIndex,
                            out string name,
                            out string abbreviature,
                            out string reagents
                        );

                        if (spellDone % 2 == 0)
                        {
                            topage++;
                        }

                        spellDone++;

                        text = new HoveredLabel(
                            name,
                            false,
                            0x0288,
                            0x33,
                            0x0288,
                            font: 9,
                            maxwidth: 130,
                            style: FontStyle.Cropped
                        )
                        {
                            X = dataX,
                            Y = 52 + y,
                            LocalSerial = (uint)topage,
                            AcceptMouseInput = true,
                            Tag = currentSpellIndex + 1,
                            CanMove = true
                        };

                        text.MouseUp += OnLabelMouseUp;
                        text.MouseDoubleClick += OnLabelMouseDoubleClick;
                        _dataBox.Add(text, page);

                        y += 15;
                    }
                }
            }
        }

        int page1 = pagesToFill + 1;
        int topTextY = 6;

        for (int i = 0, spellsDone = 0; i < maxSpellsCount; i++)
        {
            if (!_spells[i])
            {
                continue;
            }

            int iconX = 62;
            int topTextX = 87;
            int iconTextX = 112;
            uint iconSerial = 100 + (uint)i;

            if (spellsDone > 0)
            {
                if (spellsDone % 2 != 0)
                {
                    iconX = 225;
                    topTextX = 224;
                    iconTextX = 275;
                    iconSerial = 1000 + (uint)i;
                }
                else
                {
                    page1++;
                }
            }

            spellsDone++;

            GetSpellNames(i, out string name, out string abbreviature, out string reagents);

            Label text = new Label(name, false, 0x0288, font: 6)
            {
                X = topTextX,
                Y = topTextY
            };

            _dataBox.Add(text, page1);

            if (!string.IsNullOrEmpty(abbreviature))
            {
                text = new Label(abbreviature, false, 0x0288, 80, 6)
                {
                    X = iconTextX,
                    Y = 34
                };

                _dataBox.Add(text, page1);
            }

            ushort iconGraphic;
            int toolTipCliloc;

            iconGraphic = (ushort)(iconStartGraphic + i);
            GetSpellToolTip(out toolTipCliloc);

            var spellDef = GetSpellDefinition(iconSerial);
            HueGumpPic icon = new HueGumpPic(
                this,
                iconX,
                40,
                iconGraphic,
                0,
                (ushort)spellDef.ID,
                spellDef.Name
            )
            {
                X = iconX,
                Y = 40,
                LocalSerial = iconSerial
            };

            if (toolTipCliloc > 0)
            {
                string tooltip = ClilocLoader.Instance.GetString(toolTipCliloc + i);
                icon.SetTooltip(tooltip, 250);
            }

            icon.MouseDoubleClick += OnIconDoubleClick;
            icon.DragBegin += OnIconDragBegin;

            _dataBox.Add(icon, page1);

            if (!string.IsNullOrEmpty(reagents))
            {
                _dataBox.Add(new GumpPicTiled(iconX, 88, 120, 5, 0x0835), page1);
                
                Label textReags = new Label(ResGumps.Reagents, false, 0x0288, font: 6)
                {
                    X = iconX,
                    Y = 92
                };

                _dataBox.Add(textReags, page1);

                textReags = new Label(reagents, false, 0x0288, font: 9) { X = iconX, Y = 114 };

                _dataBox.Add(textReags, page1);
            }

            GetSpellRequires(i, out int requiriesY, out string requires);

            Label textRequires = new Label(requires, false, 0x0288, font: 6)
            {
                X = iconX,
                Y = requiriesY
            };

            _dataBox.Add(textRequires, page1);
        }

        SetActivePage(1);
    }
    
    
    protected override void GetSpellRequires(int offset, out int y, out string text)
    {
        y = 162;
        int manaCost = 0;
        int minSkill = 0;
        var def = SpellsMysticism.GetSpell(offset + 1);
        manaCost = def.ManaCost;
        minSkill = def.MinSkill;
        text = string.Format(ResGumps.ManaCost0MinSkill1, manaCost, minSkill);
    }
}