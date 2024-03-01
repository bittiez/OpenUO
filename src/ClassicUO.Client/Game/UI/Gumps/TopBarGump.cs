﻿#region license

// Copyright (c) 2024, andreakarasho
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal class TopBarGump : Gump
    {
        private TopBarGump(World world) : base(world, 0, 0)
        {
            CanMove = true;
            AcceptMouseInput = true;
            CanCloseWithRightClick = false;

            // little
            Add(new ResizePic(0x13BE) { Width = 30, Height = 27 }, 2);

            Add(
                new Button(0, 0x15A1, 0x15A1, 0x15A1)
                {
                    X = 5,
                    Y = 3,
                    ToPage = 1
                },
                2
            );

            // big
            int smallWidth = 50;
            ref readonly var gumpInfo = ref Client.Game.UO.Gumps.GetGump(0x098B);
            if (gumpInfo.Texture != null)
            {
                smallWidth = gumpInfo.UV.Width;
            }

            int largeWidth = 100;

            gumpInfo = ref Client.Game.UO.Gumps.GetGump(0x098D);
            if (gumpInfo.Texture != null)
            {
                largeWidth = gumpInfo.UV.Width;
            }
            var cliloc = ClilocLoader.Instance;

            List<Tuple<bool, int, string>> table = new List<Tuple<bool, int, string>>();
            table.Add(new Tuple<bool, int, string>(false, (int)Buttons.Map, cliloc.GetString(3000430, ResGumps.Map)));
            table.Add(new Tuple<bool, int, string>(true, (int)Buttons.Paperdoll, cliloc.GetString(3000133, ResGumps.Paperdoll)));
            table.Add(new Tuple<bool, int, string>(true, (int)Buttons.Inventory, cliloc.GetString(3000431, ResGumps.Inventory)));
            table.Add(new Tuple<bool, int, string>(true, (int)Buttons.Journal, cliloc.GetString(3000129, ResGumps.Journal)));
            if (!World.Settings.GeneralFlags.RemoveChatFromMenuBar)
                table.Add(new Tuple<bool, int, string>(false, (int)Buttons.Chat, cliloc.GetString(3000131, ResGumps.Chat)));
            table.Add(new Tuple<bool, int, string>(false, (int)Buttons.Help, cliloc.GetString(3000134, ResGumps.Help)));
            table.Add(new Tuple<bool, int, string>(true, (int)Buttons.WorldMap, StringHelper.CapitalizeAllWords(cliloc.GetString(1015233, ResGumps.WorldMap))));
            if (World.Settings.GeneralFlags.CooldownGumpEnabled)
                table.Add(new Tuple<bool, int, string>(false, (int)Buttons.Cooldowns, StringHelper.CapitalizeAllWords(cliloc.GetString(8016003))));
            if (!World.Settings.GeneralFlags.RemoveInfoFromMenuBar)
                table.Add(new Tuple<bool, int, string>(false, (int)Buttons.Info, cliloc.GetString(1079449, ResGumps.Info)));
            table.Add(new Tuple<bool, int, string>(false, (int)Buttons.Debug, cliloc.GetString(1042237, ResGumps.Debug)));
            table.Add(new Tuple<bool, int, string>(true, (int)Buttons.NetStats, cliloc.GetString(3000169, ResGumps.NetStats)));
            
            bool hasUOStore = Client.Game.UO.Version >= ClientVersion.CV_706400;

            if (hasUOStore)
            {
                table.Add(new Tuple<bool, int, string>(true, (int)Buttons.UOStore, cliloc.GetString(1158008, ResGumps.UOStore)));
                table.Add(new Tuple<bool, int, string>(true, (int)Buttons.GlobalChat, cliloc.GetString(1158390, ResGumps.GlobalChat)));
            }
            else if (World.Settings.GeneralSettings.StoreOverride != null)
                table.Add(new Tuple<bool, int, string>(true, (int)Buttons.UOStore, cliloc.GetString(1158008, ResGumps.UOStore)));
            

            ResizePic background;

            Add(background = new ResizePic(0x13BE) { Height = 27 }, 1);

            Add(
                new Button(0, 0x15A4, 0x15A4, 0x15A4)
                {
                    X = 5,
                    Y = 3,
                    ToPage = 2
                },
                1
            );

            int startX = 30;

            for (int i = 0; i < table.Count; i++)
            {
                ushort graphic = (ushort) (table[i].Item1 ? 0x098D : 0x098B);

                Add
                (
                    new RighClickableButton
                    (
                        table[i].Item2,
                        graphic,
                        graphic,
                        graphic,
                        table[i].Item3,
                        1,
                        true,
                        0,
                        0x0036
                    )
                    {
                        ButtonAction = ButtonAction.Activate,
                        X = startX,
                        Y = 1,
                        FontCenter = true
                    },
                    1
                );

                startX += (table[i].Item1 ? largeWidth : smallWidth) + 1;
                background.Width = startX;
            }

            background.Width = startX + 1;

            //layer
            LayerOrder = UILayer.Over;
        }

        public bool IsMinimized { get; private set; }

        public static void Create(World world)
        {
            TopBarGump gump = UIManager.GetGump<TopBarGump>();
            
            if (gump != null)
                gump.Dispose();

            if (ProfileManager.CurrentProfile.TopbarGumpPosition.X < 0 || ProfileManager.CurrentProfile.TopbarGumpPosition.Y < 0)
            {

                ProfileManager.CurrentProfile.TopbarGumpPosition = Point.Zero;
            }

            UIManager.Add
            (
                gump = new TopBarGump(world)
                {
                    X = ProfileManager.CurrentProfile.TopbarGumpPosition.X,
                    Y = ProfileManager.CurrentProfile.TopbarGumpPosition.Y
                }
            );

            if (ProfileManager.CurrentProfile.TopbarGumpIsMinimized)
            {
                gump.ChangePage(2);
            }
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button == MouseButtonType.Right && (X != 0 || Y != 0))
            {
                X = 0;
                Y = 0;

                ProfileManager.CurrentProfile.TopbarGumpPosition = Location;
            }
        }

        public override void OnPageChanged()
        {
            ProfileManager.CurrentProfile.TopbarGumpIsMinimized = IsMinimized = ActivePage == 2;
            WantUpdateSize = true;
        }

        protected override void OnDragEnd(int x, int y)
        {
            base.OnDragEnd(x, y);
            ProfileManager.CurrentProfile.TopbarGumpPosition = Location;
        }

        public override void OnButtonClick(int buttonID)
        {
            switch ((Buttons)buttonID)
            {
                case Buttons.Map:
                    GameActions.OpenMiniMap(World);

                    break;

                case Buttons.Paperdoll:
                    GameActions.OpenPaperdoll(World, World.Player);

                    break;

                case Buttons.Inventory:
                    GameActions.OpenBackpack(World);

                    break;

                case Buttons.Journal:
                    GameActions.OpenJournal(World);

                    break;

                case Buttons.Chat:
                    GameActions.OpenChat(World);

                    break;

                case Buttons.GlobalChat:
                    Log.Warn(ResGumps.ChatButtonPushedNotImplementedYet);
                    GameActions.Print(
                        World,
                        ResGumps.GlobalChatNotImplementedYet,
                        0x23,
                        MessageType.System
                    );

                    break;
                
                case Buttons.Cooldowns:
                {
                    GameActions.OpenCooldowns(World);
                    break;
                }

                case Buttons.UOStore:
                    if (World.Settings.GeneralSettings.StoreOverride != null)
                    {
                        PlatformHelper.LaunchBrowser(World.Settings.GeneralSettings.StoreOverride);
                    }
                    else
                    {
                        if (Client.Game.UO.Version >= ClientVersion.CV_706400)
                        {
                            NetClient.Socket.Send_OpenUOStore();
                        }
                    }

                    break;

                case Buttons.Help:
                    GameActions.RequestHelp();

                    break;

                case Buttons.Debug:

                    DebugGump debugGump = UIManager.GetGump<DebugGump>();

                    if (debugGump == null)
                    {
                        debugGump = new DebugGump(World, 100, 100);
                        UIManager.Add(debugGump);
                    }
                    else
                    {
                        debugGump.IsVisible = !debugGump.IsVisible;
                        debugGump.SetInScreen();
                    }

                    break;

                case Buttons.NetStats:
                    NetworkStatsGump netstatsgump = UIManager.GetGump<NetworkStatsGump>();

                    if (netstatsgump == null)
                    {
                        netstatsgump = new NetworkStatsGump(World, 100, 100);
                        UIManager.Add(netstatsgump);
                    }
                    else
                    {
                        netstatsgump.IsVisible = !netstatsgump.IsVisible;
                        netstatsgump.SetInScreen();
                    }

                    break;

                case Buttons.WorldMap:
                    GameActions.OpenWorldMap(World);

                    break;
            }
        }

        private enum Buttons
        {
            Map,
            Paperdoll,
            Inventory,
            Journal,
            Chat,
            Help,
            WorldMap,
            Cooldowns,
            Info,
            Debug,
            NetStats,
            UOStore,
            GlobalChat,
            
        }

        private class RighClickableButton : Button
        {
            public RighClickableButton(
                int buttonID,
                ushort normal,
                ushort pressed,
                ushort over = 0,
                string caption = "",
                byte font = 0,
                bool isunicode = true,
                ushort normalHue = ushort.MaxValue,
                ushort hoverHue = ushort.MaxValue
            ) : base(buttonID, normal, pressed, over, caption, font, isunicode, normalHue, hoverHue)
            { }

            public RighClickableButton(List<string> parts) : base(parts) { }

            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                base.OnMouseUp(x, y, button);
                Parent?.InvokeMouseUp(new Point(x, y), button);
            }
        }
    }
}
