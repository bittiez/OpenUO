using System;
using System.Collections.Generic;
using ClassicUO.Assets;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class MasterySpellbookGump : BaseSpellbookGump
{
    public MasterySpellbookGump(World world, uint item) : base(world, item) { }
    public MasterySpellbookGump(World world) : base(world) { }
    
    protected override void GetSpellToolTip(out int offset)
    {
        offset = 0;
    }
    protected override SpellDefinition GetSpellDefinition(int idx) => SpellsMastery.GetSpell(idx);

    
    protected override void GetBookInfo
    (
        out ushort bookGraphic, out ushort minimizedGraphic, out ushort iconStartGraphic, out int maxSpellsCount, out int spellsOnPage, out int dictionaryPagesCount
    )
    {
        maxSpellsCount = SpellsMastery.MaxSpellCount;
        bookGraphic = 0x8AC;
        minimizedGraphic = 0x08BA;
        iconStartGraphic = 0x945;
        
        spellsOnPage = Math.Min(maxSpellsCount >> 1, 8);
        dictionaryPagesCount = (int)Math.Ceiling(maxSpellsCount / 8.0f);

        if (dictionaryPagesCount % 2 != 0)
        {
            dictionaryPagesCount++;
        }
    }
    protected override void GetSpellNames(int offset, out string name, out string abbreviature, out string reagents)
    {
        var def = SpellsMastery.GetSpell(offset + 1);
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

        int pagesToFill = dictionaryPagesCount;

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

                if (j >= 1)
                {
                    text = new Label(ResGumps.Abilities, false, 0x0288, font: 6)
                    {
                        X = dataX,
                        Y = 30
                    };

                    _dataBox.Add(text, page);

                    if (
                        World.OPL.TryGetNameAndData(
                            LocalSerial,
                            out string name,
                            out string data
                        )
                    )
                    {
                        data = data.ToLower();
                        string[] buff = data.Split(
                            new[] { '\n' },
                            StringSplitOptions.RemoveEmptyEntries
                        );

                        for (int i = 0; i < buff.Length; i++)
                        {
                            if (buff[i] != null)
                            {
                                int index = buff[i].IndexOf(
                                    "mastery",
                                    StringComparison.InvariantCulture
                                );

                                if (--index < 0)
                                {
                                    continue;
                                }

                                string skillName = buff[i].Substring(0, index);

                                if (!string.IsNullOrEmpty(skillName))
                                {
                                    List<int> activedSpells =
                                        SpellsMastery.GetSpellListByGroupName(skillName);

                                    for (int k = 0; k < activedSpells.Count; k++)
                                    {
                                        int id = activedSpells[k];

                                        SpellDefinition spell = SpellsMastery.GetSpell(id);

                                        if (spell != null)
                                        {
                                            ushort iconGraphic = (ushort)spell.GumpIconID;
                                            int toolTipCliloc =
                                                id >= 0 && id < 6 ? 1115689 : 1155938 - 6;

                                            int iconMY = 55 + 44 * k;

                                            GumpPic icon = new GumpPic(
                                                225,
                                                iconMY,
                                                iconGraphic,
                                                0
                                            )
                                            {
                                                LocalSerial = (uint)(id - 1)
                                            };

                                            _dataBox.Add(icon, page);
                                            icon.MouseDoubleClick += OnIconDoubleClick;
                                            icon.DragBegin += OnIconDragBegin;

                                            text = new Label(spell.Name, false, 0x0288, 80, 6)
                                            {
                                                X = 225 + 44 + 4,
                                                Y = iconMY + 2
                                            };

                                            _dataBox.Add(text, page);

                                            if (toolTipCliloc > 0)
                                            {
                                                string tooltip =
                                                    ClilocLoader.Instance.GetString(
                                                        toolTipCliloc + id
                                                    );

                                                icon.SetTooltip(tooltip, 250);
                                            }
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }

                    break;
                }

                text = new Label(
                    page == pagesToFill ? ResGumps.Passive : ResGumps.Activated,
                    false,
                    0x0288,
                    font: 6
                )
                {
                    X = dataX,
                    Y = 30
                };

                _dataBox.Add(text, page);

                int topage = pagesToFill + ((spellDone + 1) >> 1);

                int length = SpellsMastery.SpellbookIndices[page - 1].Length;

                for (int k = 0; k < length; k++)
                {
                    currentSpellIndex = SpellsMastery.SpellbookIndices[page - 1][k] - 1;

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
                            LocalSerial = (uint)(pagesToFill + currentSpellIndex / 2 + 1),
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
            Label textSpellName = new Label(
                SpellsMastery.GetMasteryGroupByID(i + 1),
                false,
                0x0288,
                font: 6
            )
            {
                X = topTextX,
                Y = topTextY + 4
            };

            _dataBox.Add(textSpellName, page1);

            textSpellName = new Label(name, false, 0x0288, 80, 6) { X = iconTextX, Y = 34 };

            _dataBox.Add(textSpellName, page1);

            if (!string.IsNullOrEmpty(abbreviature))
            {
                int abbreviatureY = 26;

                if (textSpellName.Height < 24)
                {
                    abbreviatureY = 31;
                }

                abbreviatureY += textSpellName.Height;

                textSpellName = new Label(abbreviature, false, 0x0288, 80, 6)
                {
                    X = iconTextX,
                    Y = abbreviatureY
                };

                _dataBox.Add(textSpellName, page1);
            }

            ushort iconGraphic;
            int toolTipCliloc;

            iconGraphic = (ushort)SpellsMastery.GetSpell(i + 1).GumpIconID;

            toolTipCliloc = i >= 0 && i < 6 ? 1115689 : 1155938 - 6;

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
                Label text = new Label(ResGumps.Reagents, false, 0x0288, font: 6)
                {
                    X = iconX,
                    Y = 92
                };

                _dataBox.Add(text, page1);

                text = new Label(reagents, false, 0x0288, font: 9) { X = iconX, Y = 114 };

                _dataBox.Add(text, page1);
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
        var def = SpellsMastery.GetSpell(offset + 1);
        manaCost = def.ManaCost;
        minSkill = def.MinSkill;

        if (def.TithingCost > 0)
        {
            y = 148;
            text = string.Format(
                ResGumps.Upkeep0Mana1MinSkill2,
                def.TithingCost,
                manaCost,
                minSkill
            );
        }
        else
        {
            text = string.Format(ResGumps.ManaCost0MinSkill1, manaCost, minSkill);
        }
    }
}