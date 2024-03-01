using System;
using System.Collections.Generic;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal abstract class BaseSpellbookGump : Gump
{
    protected DataBox _dataBox;
    protected HitBox _hitBox;
    protected bool _isMinimized;
    protected int _maxPage;
    protected GumpPicBase _pageCornerLeft,
                    _pageCornerRight,
                    _picBase;
    protected readonly bool[] _spells = new bool[64];
    protected readonly bool[] _extraspells = new bool[64];
    /*{
        true, true, true, true,
        true, true, true, true,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
        false, false, false, false,
    };*/
    protected int _enqueuePage = -1;
    protected abstract void GetSpellNames(int offset, out string name, out string abbreviature, out string reagents);
    protected abstract void GetSpellToolTip(out int offset);
    
    
    public BaseSpellbookGump(World world, uint item) : this(world)
    {
        LocalSerial = item;

        BuildGump();
    }
    
    
    public BaseSpellbookGump(World world) : base(world, 0, 0)
    {
        CanMove = true;
        AcceptMouseInput = false;
        CanCloseWithRightClick = true;
    }

    public bool IsMinimized
    {
        get => _isMinimized;
        set
        {
            if (_isMinimized != value)
            {
                _isMinimized = value;

                GetBookInfo(
                    out ushort bookGraphic,
                    out ushort minimizedGraphic,
                    out ushort iconStartGraphic,
                    out int maxSpellsCount,
                    out int spellsOnPage,
                    out int dictionaryPagesCount
                );

                _picBase.Graphic = value ? minimizedGraphic : bookGraphic;

                foreach (Control c in Children)
                {
                    c.IsVisible = !value;
                }
                _picBase.IsVisible = value;

                //_picBase.IsVisible = true;
                WantUpdateSize = true;
            }
        }
    }
    
    public override void Save(XmlTextWriter writer)
    {
        base.Save(writer);
        writer.WriteAttributeString("isminimized", IsMinimized.ToString());
    }

    public override void Restore(XmlElement xml)
    {
        base.Restore(xml);

        Client.Game.GetScene<GameScene>().DoubleClickDelayed(LocalSerial);

        Dispose();
    }
    protected virtual void BuildGump()
    {
        Item item = World.Items.Get(LocalSerial);

        if (item == null)
        {
            Dispose();

            return;
        }

        //AssignGraphic(item);

        GetBookInfo(
            out ushort bookGraphic,
            out ushort minimizedGraphic,
            out ushort iconStartGraphic,
            out int maxSpellsCount,
            out int spellsOnPage,
            out int dictionaryPagesCount
        );

        Add(_picBase = new GumpPic(0, 0, bookGraphic, 0));
        _picBase.MouseDoubleClick += _picBase_MouseDoubleClick;

        _dataBox = new DataBox(0, 0, 0, 0)
        {
            CanMove = true,
            AcceptMouseInput = true,
            WantUpdateSize = true
        };

        Add(_dataBox);
        _hitBox = new HitBox(0, 98, 27, 23);
        Add(_hitBox);
        _hitBox.MouseUp += _hitBox_MouseUp;

        Add(_pageCornerLeft = new GumpPic(50, 8, 0x08BB, 0));
        _pageCornerLeft.LocalSerial = 0;
        _pageCornerLeft.Page = int.MaxValue;
        _pageCornerLeft.MouseUp += PageCornerOnMouseClick;
        _pageCornerLeft.MouseDoubleClick += PageCornerOnMouseDoubleClick;
        Add(_pageCornerRight = new GumpPic(321, 8, 0x08BC, 0));
        _pageCornerRight.LocalSerial = 1;
        _pageCornerRight.Page = 1;
        _pageCornerRight.MouseUp += PageCornerOnMouseClick;
        _pageCornerRight.MouseDoubleClick += PageCornerOnMouseDoubleClick;

        RequestUpdateContents();

        Client.Game.Audio.PlaySound(0x0055);
    }
    

    protected void _picBase_MouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
    {
        if (e.Button == MouseButtonType.Left && IsMinimized)
        {
            IsMinimized = false;
        }
    }

    protected void _hitBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtonType.Left && !IsMinimized)
        {
            IsMinimized = true;
        }
    }

    public override void Dispose()
    {
        Client.Game.Audio.PlaySound(0x0055);
        UIManager.SavePosition(LocalSerial, Location);
        base.Dispose();
    }

    protected abstract void CreateBook();
    
    protected override void UpdateContents()
    {
        Item item = World.Items.Get(LocalSerial);

        if (item == null)
        {
            Dispose();

            return;
        }

        //AssignGraphic(item);

        CreateBook();
    }
    
    protected virtual void OnIconDoubleClick(object sender, MouseDoubleClickEventArgs e)
    {
        if (e.Button == MouseButtonType.Left)
        {
            SpellDefinition def = GetSpellDefinition((sender as Control).LocalSerial);

            if (def != null)
            {
                GameActions.CastSpell(def.ID);
            }
        }
    }

    protected void OnIconDragBegin(object sender, EventArgs e)
    {
        if (UIManager.DraggingControl != this || UIManager.MouseOverControl != sender)
        {
            return;
        }

        SpellDefinition def = GetSpellDefinition((sender as Control).LocalSerial);

        if (def == null)
        {
            return;
        }

        GetSpellFloatingButton(def.ID)?.Dispose();

        UseSpellButtonGump gump = new UseSpellButtonGump(World, def)
        {
            X = Mouse.LClickPosition.X - 22,
            Y = Mouse.LClickPosition.Y - 22
        };

        UIManager.Add(gump);
        UIManager.AttemptDragControl(gump, true);
    }

    private static UseSpellButtonGump GetSpellFloatingButton(int id)
    {
        for (LinkedListNode<Gump> i = UIManager.Gumps.Last; i != null; i = i.Previous)
        {
            if (i.Value is UseSpellButtonGump g && g.SpellID == id)
            {
                return g;
            }
        }

        return null;
    }
    
    protected SpellDefinition GetSpellDefinition(uint serial)
    {
        int idx =
            (int)(
                serial > 1000
                    ? serial - 1000
                    : serial >= 100
                        ? serial - 100
                        : serial
            ) + 1;

        return GetSpellDefinition(idx);
    }

    protected abstract SpellDefinition GetSpellDefinition(int idx);

    protected abstract void GetBookInfo
    (
        out ushort bookGraphic, out ushort minimizedGraphic, out ushort iconStartGraphic, out int maxSpellsCount, out int spellsOnPage, out int dictionaryPagesCount
    );
    
    protected override void OnDragBegin(int x, int y)
    {
        if (UIManager.MouseOverControl?.RootParent == this)
        {
            UIManager.MouseOverControl.InvokeDragBegin(new Point(x, y));
        }

        base.OnDragBegin(x, y);
    }

    protected override void OnDragEnd(int x, int y)
    {
        if (UIManager.MouseOverControl?.RootParent == this)
        {
            UIManager.MouseOverControl.InvokeDragEnd(new Point(x, y));
        }

        base.OnDragEnd(x, y);
    }

    protected abstract void GetSpellRequires(int offset, out int y, out string text);

    protected void SetActivePage(int page)
    {
        if (page == _dataBox.ActivePage)
        {
            return;
        }

        if (page < 1)
        {
            page = 1;
        }
        else if (page > _maxPage)
        {
            page = _maxPage;
        }

        _dataBox.ActivePage = page;
        _pageCornerLeft.Page = _dataBox.ActivePage != 1 ? 0 : int.MaxValue;
        _pageCornerRight.Page = _dataBox.ActivePage != _maxPage ? 0 : int.MaxValue;

        Client.Game.Audio.PlaySound(0x0055);
    }

    protected void OnLabelMouseUp(object sender, MouseEventArgs e)
    {
        if (
            e.Button == MouseButtonType.Left
            && Mouse.LDragOffset == Point.Zero
            && sender is HoveredLabel l
        )
        {
            _enqueuePage = (int)l.LocalSerial;
        }
    }

    protected void OnLabelMouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
    {
        if (e.Button == MouseButtonType.Left && sender is HoveredLabel l)
        {
            SpellDefinition def = GetSpellDefinition((int)l.Tag - 1);

            if (def != null)
            {
                GameActions.CastSpell(def.ID);
            }

            _enqueuePage = -1;
            e.Result = true;
        }
    }

    public override void Update()
    {
        base.Update();

        Item item = World.Items.Get(LocalSerial);

        if (item == null)
        {
            Dispose();
        }

        if (IsDisposed)
        {
            return;
        }

        if (
            _enqueuePage >= 0
            && Time.Ticks - Mouse.LastLeftButtonClickTime >= Mouse.MOUSE_DELAY_DOUBLE_CLICK
        )
        {
            SetActivePage(_enqueuePage);
            _enqueuePage = -1;
        }
    }
    
    /*private void AssignGraphic(Item item)
        {
            switch (item.Graphic)
            {
                default:
                case 0x0EFA:
                    _spellBookType = SpellBookType.Magery;

                    break;

                case 0x2253:
                    _spellBookType = SpellBookType.Necromancy;

                    break;

                case 0x2252:
                    _spellBookType = SpellBookType.Chivalry;

                    break;

                case 0x238C:

                    if ((World.ClientFeatures.Flags & CharacterListFlags.CLF_SAMURAI_NINJA) != 0)
                    {
                        _spellBookType = SpellBookType.Bushido;
                    }

                    break;

                case 0x23A0:

                    if ((World.ClientFeatures.Flags & CharacterListFlags.CLF_SAMURAI_NINJA) != 0)
                    {
                        _spellBookType = SpellBookType.Ninjitsu;
                    }

                    break;

                case 0x2D50:
                    _spellBookType = SpellBookType.Spellweaving;

                    break;

                case 0x2D9D:
                    _spellBookType = SpellBookType.Mysticism;

                    break;

                case 0x225A:
                case 0x225B:
                    _spellBookType = SpellBookType.Mastery;

                    break;
            }
        }*/

        protected void PageCornerOnMouseClick(object sender, MouseEventArgs e)
        {
            if (
                e.Button == MouseButtonType.Left
                && Mouse.LDragOffset == Point.Zero
                && sender is Control ctrl
            )
            {
                SetActivePage(
                    ctrl.LocalSerial == 0 ? _dataBox.ActivePage - 1 : _dataBox.ActivePage + 1
                );
            }
        }

        protected void PageCornerOnMouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
        {
            if (e.Button == MouseButtonType.Left && sender is Control ctrl)
            {
                SetActivePage(ctrl.LocalSerial == 0 ? 1 : _maxPage);
            }
        }
        protected class HueGumpPic : GumpPic
        {
            private readonly BaseSpellbookGump _gump;
            private readonly MacroManager _mm;
            private readonly ushort _spellID;
            private readonly string _spellName;

            /// <summary>
            /// ShowEdit button when user pressing ctrl + alt
            /// </summary>
            private bool ShowEdit =>
                Keyboard.Ctrl && Keyboard.Alt && ProfileManager.CurrentProfile.FastSpellsAssign;

            public HueGumpPic(
                BaseSpellbookGump gump,
                int x,
                int y,
                ushort graphic,
                ushort hue,
                ushort spellID,
                string spellName
            ) : base(x, y, graphic, hue)
            {
                _gump = gump;
                _spellID = spellID;
                _spellName = spellName;

                _mm = gump.World.Macros;
            }

            public override void Update()
            {
                base.Update();

                if (_gump.World.ActiveSpellIcons.IsActive(_spellID))
                {
                    Hue = 38;
                }
                else if (Hue != 0)
                {
                    Hue = 0;
                }
            }

            /// <summary>
            /// Overide Draw method to include + icon when ShowEdit is true
            /// </summary>
            /// <param name="batcher"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                base.Draw(batcher, x, y);

                if (ShowEdit)
                {
                    Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);

                    ref readonly var gumpInfo = ref Client.Game.UO.Gumps.GetGump(0x09CF);

                    if (gumpInfo.Texture != null)
                    {
                        if (
                            UIManager.MouseOverControl != null
                            && (
                                UIManager.MouseOverControl == this
                                || UIManager.MouseOverControl.RootParent == this
                            )
                        )
                        {
                            hueVector.X = 34;
                            hueVector.Y = 1;
                        }
                        else
                        {
                            hueVector.X = 0x44;
                            hueVector.Y = 1;
                        }

                        batcher.Draw(
                            gumpInfo.Texture,
                            new Vector2(x + (Width - gumpInfo.UV.Width), y),
                            gumpInfo.UV,
                            hueVector
                        );
                    }
                }

                return true;
            }

            /// <summary>
            /// On User Click and ShowEdit true we should show them macro editor
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="button"></param>
            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left && ShowEdit)
                {
                    Macro mCast = Macro.CreateFastMacro(_gump.World, _spellName, MacroType.CastSpell, (MacroSubType)GetSpellsId() + SpellBookDefinition.GetSpellsGroup(_spellID), (MacroSubType)0);
                    if (_mm.FindMacro(_spellName) == null)
                    {
                        _mm.MoveToBack(mCast);
                    }
                    GameActions.OpenMacroGump(_gump.World, _spellName);
                }
            }

            /// <summary>
            /// Get Spell Id
            /// </summary>
            /// <returns></returns>
            private int GetSpellsId()
            {
                return _spellID % 100;
            }
        }

}