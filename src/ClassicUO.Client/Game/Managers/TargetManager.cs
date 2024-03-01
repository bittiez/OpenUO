#region license

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
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Network;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers
{
    [Flags]
    internal enum HighlightType : ushort
    {
        Item = 0x1,
        Land = 0x2,
        Mobile = 0x4,
        Multi = 0x8,
        Static = 0x10,
    }
    internal enum CursorTarget
    {
        Invalid = -1,
        Object = 0,
        Position = 1,
        MultiPlacement = 2,
        SetTargetClientSide = 3,
        Grab,
        SetGrabBag,
        HueCommandTarget,
        IgnorePlayerTarget,
        FriendTarget,
        Dummy
    }

    internal class CursorType
    {
        public static readonly uint Target = 6983686;
    }

    internal enum TargetType
    {
        Neutral,
        Harmful,
        Beneficial,
        Cancel
    }

    internal class MultiTargetInfo
    {
        public MultiTargetInfo(ushort model, ushort x, ushort y, short z, ushort hue)
        {
            Model = model;
            XOff = x;
            YOff = y;
            ZOff = z;
            Hue = hue;
        }

        public short ZOff;
        public ushort XOff, YOff, Model, Hue;
    }

    internal class LastTargetInfo
    {
        public bool IsEntity => SerialHelper.IsValid(Serial);
        public bool IsStatic => !IsEntity && Graphic != 0 && Graphic != 0xFFFF;
        public bool IsLand => !IsStatic;
        public ushort Graphic;
        public uint Serial;
        public ushort X, Y;
        public sbyte Z;


        public void SetEntity(uint serial)
        {
            Serial = serial;
            Graphic = 0xFFFF;
            X = Y = 0xFFFF;
            Z = sbyte.MinValue;
        }

        public void SetStatic(ushort graphic, ushort x, ushort y, sbyte z)
        {
            Serial = 0;
            Graphic = graphic;
            X = x;
            Y = y;
            Z = z;
        }

        public void SetLand(ushort x, ushort y, sbyte z)
        {
            Serial = 0;
            Graphic = 0xFFFF;
            X = x;
            Y = y;
            Z = z;
        }

        public void Clear()
        {
            Serial = 0;
            Graphic = 0xFFFF;
            X = Y = 0xFFFF;
            Z = sbyte.MinValue;
        }
    }
    
    
    internal class StoredTarget
    {
        public CursorTarget TargetingState { get; private set; }
        public uint TargetCursorID { get; private set; }
        public MultiTargetInfo MultiTargetInfo { get; private set; }
        public TargetType TargetingType { get; private set; }
        public bool IsAoE { get; private set; }
        public object AoERange { get; private set; }
        public int AoEType { get; private set; }
        public uint Preview { get; private set; }
        public ushort PreviewHue { get; private set; }

        public StoredTarget(CursorTarget targetingState, uint targetCursorID, MultiTargetInfo multiTargetInfo, 
                            TargetType targetingType, bool isAoE, object AOERange, int AOEType, uint preview,
                            ushort previewHue)
        {
            TargetingState = targetingState;
            TargetCursorID = targetCursorID;
            MultiTargetInfo = multiTargetInfo;
            TargetingType = targetingType;
            IsAoE = isAoE;
            AoERange = AOERange;
            AoEType = AOEType;
            Preview = preview;
            PreviewHue = previewHue;
        }
    }

    internal sealed class TargetManager
    {
        private uint _targetCursorId;
        private readonly World _world;
        private readonly byte[] _lastDataBuffer = new byte[19];


        public TargetManager(World world) { _world = world; }

        public uint LastAttack, SelectedTarget;

        public readonly LastTargetInfo LastTargetInfo = new LastTargetInfo();

        public readonly LastTargetInfo LastBeneficialTargetInfo = new LastTargetInfo();

        public MultiTargetInfo MultiTargetInfo { get; set; }

        public CursorTarget TargetingState { get; set; } = CursorTarget.Invalid;

        public bool IsTargeting { get; private set; }

        public TargetType TargetingType { get; private set; }
        public object ExtraDetails { get; private set; }
        public bool IsAoE { get; private set; }
        public int AoEType { get; private set; }
        public uint Preview { get; private set; }
        public ushort PreviewHue { get; private set; }

        public List<StoredTarget> StoredTargets { get; set; } = new List<StoredTarget>();

        public void StoreTarget()
        {
            if (IsTargeting)
            {
                StoredTargets.Add(
                    new StoredTarget(TargetingState, _targetCursorId, MultiTargetInfo, 
                                     TargetingType, IsAoE, ExtraDetails, AoEType, Preview, PreviewHue));
                ClearTargetingWithoutTargetCancelPacket(false);
            }
            
        }

        public void RestoreTarget()
        {
            if (StoredTargets.Count > 0)
            {
                var targ = StoredTargets[0];
                StoredTargets.RemoveAt(0);

                TargetingState = targ.TargetingState;
                _targetCursorId = targ.TargetCursorID;
                TargetingType = targ.TargetingType;
                MultiTargetInfo = targ.MultiTargetInfo;
                IsAoE = targ.IsAoE;
                ExtraDetails = targ.AoERange;
                AoEType = targ.AoEType;
                Preview = targ.Preview;
                PreviewHue = targ.PreviewHue;
                
                IsTargeting = targ.TargetingType < TargetType.Cancel;
            }
        }

        private void ClearTargetingWithoutTargetCancelPacket(bool restore = true)
        {
            if (TargetingState == CursorTarget.MultiPlacement)
            {
                MultiTargetInfo = null;
                TargetingState = 0;
                _world.HouseManager.Remove(0);
            }

            IsTargeting = false;
            
            IsAoE = false;
            ExtraDetails = 0;
            AoEType = 0;
            Preview = 0;
            PreviewHue = 0;
            
            if (restore)
                RestoreTarget();
        }

        public void Reset()
        {
            ClearTargetingWithoutTargetCancelPacket();

            TargetingState = 0;
            _targetCursorId = 0;
            MultiTargetInfo = null;
            TargetingType = 0;
            IsAoE = false;
            ExtraDetails = 0;
            AoEType = 0;
            Preview = 0;
            PreviewHue = 0;
            
            RestoreTarget();
        }
        
        public void SetExtra(uint cursorID, ushort type, object details, uint preview, ushort previewHue)
        {
            IsAoE = true;
            AoEType = type;
            ExtraDetails = details;
            Preview = preview;
            PreviewHue = previewHue;
        }


        public void SetTargeting(CursorTarget targeting, uint cursorID, TargetType cursorType)
        {
            if (targeting == CursorTarget.Invalid)
            {
                return;
            }

            bool lastTargetting = IsTargeting;
            IsTargeting = cursorType < TargetType.Cancel;
            TargetingState = targeting;
            TargetingType = cursorType;

            if (IsTargeting)
            {
                //UIManager.RemoveTargetLineGump(LastTarget);
            }
            else if (lastTargetting)
            {
                CancelTarget();
            }

            // https://github.com/andreakarasho/ClassicUO/issues/1373
            // when receiving a cancellation target from the server we need
            // to send the last active cursorID, so update cursor data later

            _targetCursorId = cursorID;
        }
        
        private ushort zHue = 0x20;
        private DateTime LastChange;
        private Rectangle m_OldRect;

        public bool AreaOfEffectHighlight(int x, int y, HighlightType highlightType, out ushort hue)
        {
            hue = 0;

            if (ProfileManager.CurrentProfile.ShowAoEArea && IsAoE)
            {
                int locX = -200, locY = -200;

                if (SelectedObject.Object is GameObject gameObject)
                {
                    locX = gameObject.X;
                    locY = gameObject.Y;
                }
                else if (SelectedObject.Object is Land land)
                {
                    locX = land.X;
                    locY = land.Y;
                }
                else if (SelectedObject.Object is Mobile m)
                {
                    locX = m.X;
                    locY = m.Y;
                }
                else if (SelectedObject.Object is Item i)
                {
                    if (i.Container == 0xFFFF_FFFF)
                    {
                        locX = i.X;
                        locY = i.Y;
                    }
                }

                if (locX != -200)
                {
                    
                    bool inRange = false;
                    if (AoEType == 0)
                    {
                        var range = (int)(ushort)ExtraDetails;
                        var rect = new Rectangle(-range, -range, range * 2 + 1, range * 2 + 1);

                        if (rect.Contains(locX - x, locY - y))
                        {
                            inRange = true;
                        }
                    } 
                    else if (AoEType == 1)
                    {
                        var range = (int)(ushort)ExtraDetails;
                        double dist = Math.Sqrt(Math.Pow(x - locX,2) + Math.Pow(y - locY,2));
                        dist = (int) dist;
                        inRange = dist <= range;
                    }
                    else if (AoEType == 2) // rect from point to mouse
                    {
                        var pos = (Vector3)ExtraDetails;
                        int topLeftX = Math.Min(locX, (int)pos.X) - 1;
                        int topLeftY = Math.Min(locY, (int)pos.Y) - 1;
                        int bottomRightX = Math.Max(locX, (int)pos.X) - topLeftX;
                        int bottomRightY = Math.Max(locY, (int)pos.Y) - topLeftY;
                        var rect = new Rectangle(topLeftX, topLeftY, bottomRightX, bottomRightY);

                        if (rect.Contains(x - 1, y - 1))
                            inRange = true;
                    }
                    else if (AoEType == 3)
                    {
                        var pos = (Vector3)ExtraDetails;
                        var posZ = (int) pos.Z;
                        int topLeftX = Math.Min(locX, (int)pos.X);
                        int topLeftY = Math.Min(locY, (int)pos.Y);
                        int bottomRightX = Math.Max(locX, (int)pos.X) - topLeftX + 1;
                        int bottomRightY = Math.Max(locY, (int)pos.Y) - topLeftY + 1;
                        var rect = new Rectangle(topLeftX, topLeftY, bottomRightX, bottomRightY);

                        if (!rect.Equals(m_OldRect))
                        {
                            m_OldRect = rect;
                            MultiTargetInfo.XOff = (ushort)topLeftX;
                            MultiTargetInfo.YOff = (ushort)topLeftY;
                            //if (rect.Contains(x - 1, y - 1))
                            //    inRange = true;

                            if (!_world.HouseManager.TryGetHouse(0, out House house))
                            {
                                house = new House(_world, 0, 0, false);
                                _world.HouseManager.Add(0, house);
                            }
                            else
                            {
                                house.ClearComponents();
                                house.Revision++;
                                house.IsCustom = true;
                            }
    
                            for (int rectX = 0; rectX < rect.Width; rectX++)
                            {
                                for (int rectY = 0; rectY < rect.Height; rectY++)
                                {
                                    /*ushort tempColor = color;

                                    if (x == StartPos.X || y == StartPos.Y)
                                    {
                                        tempColor++;
                                    }*/

                                    Multi mo = house.Add
                                    (
                                        (ushort)Preview,
                                        PreviewHue,
                                        (ushort)rectX,
                                        (ushort)rectY,
                                        (sbyte) posZ,
                                        true,
                                        false
                                    );

                                    mo.MultiOffsetX = rectX;
                                    mo.MultiOffsetY = rectY;
                                    //mo.AlphaHue = 0xFF;

                                    mo.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;

                                    mo.AddToTile();
                                }
                            }
                        }
                    }
                    else if (AoEType == 0x99) // fields..
                    {
                        var range = (int)(ushort)ExtraDetails;
                        int dx = _world.Player.X - locX;
                        int dy = _world.Player.Y - locY;
                        int rx = (dx - dy) * 44;
                        int ry = (dx + dy) * 44;

                        bool eastToWest;

                        if (rx >= 0 && ry >= 0)
                        {
                            eastToWest = false;
                        }
                        else if (rx >= 0)
                        {
                            eastToWest = true;
                        }
                        else if (ry >= 0)
                        {
                            eastToWest = true;
                        }
                        else
                        {
                            eastToWest = false;
                        }
                        
                        var rect = new Rectangle(eastToWest ? -range : 0, eastToWest ? 0 : -range, eastToWest ? range * 2 + 1 : 1, eastToWest ? 1: range * 2 + 1);

                        if (rect.Contains(locX - x, locY - y))
                            inRange = true;
                    }

                    if (inRange)
                    {
                       /* if (LastChange <= DateTime.UtcNow)
                        {
                            LastChange = DateTime.UtcNow + TimeSpan.FromMilliseconds(50);

                            if (zHue > 1060)
                                zHue = 0;

                            zHue = 980;
                            World.Player.AddMessage(MessageType.Label, $"{zHue}", TextType.CLIENT);
                        }*/
                       
                       Profile currentProfile = ProfileManager.CurrentProfile;

                       if (currentProfile != null)
                       {

                           ushort highlightHue = currentProfile.NeutralAOEHue;

                           if (TargetingType == TargetType.Beneficial)
                               highlightHue = currentProfile.BeneficialAOEHue;
                           else if (TargetingType == TargetType.Harmful)
                               highlightHue = currentProfile.HarmfulAOEHue;

                           hue = highlightHue;
                       }

                       return true;
                    }
                }
            }
            
            
            if (UIManager.HighlightedAreas.Count > 0)
            {
                foreach (var i in UIManager.HighlightedAreas)
                {
                    if (!i.Type.HasFlag(highlightType))
                        continue;
                    if (i.Rectangle.Contains(x, y))
                    {
                        hue = i.Hue;
                        return true;
                    }
                }
            }

            return false;
        }



        public void CancelTarget()
        {
            if (TargetingState == CursorTarget.MultiPlacement)
            {
                _world.HouseManager.Remove(0);

                if (_world.CustomHouseManager != null)
                {
                    _world.CustomHouseManager.Erasing = false;
                    _world.CustomHouseManager.SeekTile = false;
                    _world.CustomHouseManager.SelectedGraphic = 0;
                    _world.CustomHouseManager.CombinedStair = false;

                    UIManager.GetGump<HouseCustomizationGump>()?.Update();
                }
            }

            if (IsTargeting || TargetingType == TargetType.Cancel)
            {
                NetClient.Socket.Send_TargetCancel(TargetingState, _targetCursorId, (byte)TargetingType);
                IsTargeting = false;
            }

            Reset();
        }

        public void SetTargetingMulti
        (
            uint deedSerial,
            ushort model,
            ushort x,
            ushort y,
            short z,
            ushort hue
        )
        {
            SetTargeting(CursorTarget.MultiPlacement, deedSerial, TargetType.Neutral);

            //if (model != 0)
            MultiTargetInfo = new MultiTargetInfo
            (
                model,
                x,
                y,
                z,
                hue
            );
        }

        public bool SplitLastTargets => _world.Settings.ClientOptionFlags.AllowSplitTargetsOptions && 
                                                  ProfileManager.CurrentProfile != null && 
                                                  ProfileManager.CurrentProfile.SplitLastTarget;
        
        public void Target(uint serial)
        {
            if (!IsTargeting)
            {
                return;
            }

            Entity entity = _world.InGame ? _world.Get(serial) : null;

            if (entity != null)
            {
                switch (TargetingState)
                {
                    case CursorTarget.Invalid: return;

                    case CursorTarget.MultiPlacement:
                    case CursorTarget.Position:
                    case CursorTarget.Object:
                    case CursorTarget.HueCommandTarget:
                    case CursorTarget.SetTargetClientSide:
                    case CursorTarget.Dummy:

                        if (entity != _world.Player)
                        {
                            if (SplitLastTargets && TargetingType == TargetType.Beneficial)
                                LastBeneficialTargetInfo.SetEntity(serial);
                            else
                                LastTargetInfo.SetEntity(serial);
                        }

                        if (SerialHelper.IsMobile(serial) && serial != _world.Player && (_world.Player.NotorietyFlag == NotorietyFlag.Innocent || _world.Player.NotorietyFlag == NotorietyFlag.Ally))
                        {
                            Mobile mobile = entity as Mobile;

                            if (mobile != null)
                            {
                                bool showCriminalQuery = false;

                                if (TargetingType == TargetType.Harmful && ProfileManager.CurrentProfile.EnabledCriminalActionQuery && mobile.NotorietyFlag == NotorietyFlag.Innocent)
                                {
                                    showCriminalQuery = true;
                                }
                                else if (TargetingType == TargetType.Beneficial && ProfileManager.CurrentProfile.EnabledBeneficialCriminalActionQuery && (mobile.NotorietyFlag == NotorietyFlag.Criminal || mobile.NotorietyFlag == NotorietyFlag.Murderer || mobile.NotorietyFlag == NotorietyFlag.Gray))
                                {
                                    showCriminalQuery = true;
                                }

                                if (showCriminalQuery && UIManager.GetGump<QuestionGump>() == null)
                                {
                                    QuestionGump messageBox = new QuestionGump
                                    (
                                        _world,
                                        "This may flag\nyou criminal!",
                                        s =>
                                        {
                                            if (s)
                                            {
                                                NetClient.Socket.Send_TargetObject(entity,
                                                                                   entity.Graphic,
                                                                                   entity.X,
                                                                                   entity.Y,
                                                                                   entity.Z,
                                                                                   _targetCursorId,
                                                                                   (byte)TargetingType);

                                                ClearTargetingWithoutTargetCancelPacket();

                                                if (LastTargetInfo.Serial != serial)
                                                {
                                                    GameActions.RequestMobileStatus(_world, serial);
                                                }
                                            }
                                        }
                                    );

                                    UIManager.Add(messageBox);

                                    return;
                                }
                            }
                        }

                        if (TargetingState != CursorTarget.SetTargetClientSide && TargetingState != CursorTarget.Dummy)
                        {
                            _lastDataBuffer[0] = 0x6C;

                            _lastDataBuffer[1] = 0x00;

                            _lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
                            _lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
                            _lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
                            _lastDataBuffer[5] = (byte)_targetCursorId;

                            _lastDataBuffer[6] = (byte) TargetingType;

                            _lastDataBuffer[7] = (byte)(entity.Serial >> 24);
                            _lastDataBuffer[8] = (byte)(entity.Serial >> 16);
                            _lastDataBuffer[9] = (byte)(entity.Serial >> 8);
                            _lastDataBuffer[10] = (byte)entity.Serial;

                            _lastDataBuffer[11] = (byte)(entity.X >> 8);
                            _lastDataBuffer[12] = (byte)entity.X;

                            _lastDataBuffer[13] = (byte)(entity.Y >> 8);
                            _lastDataBuffer[14] = (byte)entity.Y;

                            _lastDataBuffer[15] = (byte)(entity.Z >> 8);
                            _lastDataBuffer[16] = (byte)entity.Z;

                            _lastDataBuffer[17] = (byte)(entity.Graphic >> 8);
                            _lastDataBuffer[18] = (byte)entity.Graphic;


                            NetClient.Socket.Send_TargetObject(entity,
                                                               entity.Graphic,
                                                               entity.X,
                                                               entity.Y,
                                                               entity.Z,
                                                               _targetCursorId,
                                                               (byte)TargetingType);

                            if (SerialHelper.IsMobile(serial) && LastTargetInfo.Serial != serial)
                            {
                                GameActions.RequestMobileStatus(_world,serial);
                            }
                        }

                        ClearTargetingWithoutTargetCancelPacket();

                        Mouse.CancelDoubleClick = true;

                        break;

                    case CursorTarget.Grab:

                        if (SerialHelper.IsItem(serial))
                        {
                            GameActions.GrabItem(_world, serial, ((Item) entity).Amount);
                        }

                        ClearTargetingWithoutTargetCancelPacket();

                        return;

                    case CursorTarget.SetGrabBag:

                        if (SerialHelper.IsItem(serial))
                        {
                            ProfileManager.CurrentProfile.GrabBagSerial = serial;
                            GameActions.Print(_world, string.Format(ResGeneral.GrabBagSet0, serial));
                        }

                        ClearTargetingWithoutTargetCancelPacket();

                        return;
                    case CursorTarget.IgnorePlayerTarget:
                        if (SelectedObject.Object is Entity pmEntity)
                        {
                            _world.IgnoreManager.AddIgnoredTarget(pmEntity);
                        }
                        CancelTarget();
                        return;
                    
                    case CursorTarget.FriendTarget:
                        if (SelectedObject.Object is Entity mEntity)
                        {
                            _world.FriendManager.AddFriendTarget(mEntity);
                        }
                        CancelTarget();
                        return;
                }
            }
            else if (_world.Settings.ClientOptionFlags.AllowOffscreenTargeting &&
                     ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.OffscreenTargeting)
            {
                if (_world.Settings.ClientOptionFlags.AllowSplitTargetsOptions && 
                    SplitLastTargets && TargetingType == TargetType.Beneficial)
                {
                    LastBeneficialTargetInfo.SetEntity(serial);
                    _world.Player.AddMessage(MessageType.Label, "beneficial last target set", TextType.CLIENT);
                }
                else
                {
                    LastTargetInfo.SetEntity(serial);
                    _world.Player.AddMessage(MessageType.Label, "last target set", TextType.CLIENT);
                }

            }
        }

        public void Target(ushort graphic, ushort x, ushort y, short z, bool wet = false)
        {
            if (!IsTargeting)
            {
                return;
            }

            if (graphic == 0)
            {
                if (TargetingState == CursorTarget.Object)
                {
                    return;
                }
            }
            else
            {
                if (graphic >= TileDataLoader.Instance.StaticData.Length)
                {
                    return;
                }

                ref StaticTiles itemData = ref TileDataLoader.Instance.StaticData[graphic];

                if (Client.Game.UO.Version >= ClientVersion.CV_7090 && itemData.IsSurface)
                {
                    z += itemData.Height;
                }
            }

            LastTargetInfo.SetStatic(graphic, x, y, (sbyte) z);

            TargetPacket(graphic, x, y, (sbyte) z);
        }

        public void SendMultiTarget(ushort x, ushort y, sbyte z)
        {
            TargetPacket(0, x, y, z);
            MultiTargetInfo = null;
        }

        public void TargetLast()
        {
            if (!IsTargeting)
            {
                return;
            }

            _lastDataBuffer[0] = 0x6C;
            _lastDataBuffer[1] = (byte) TargetingState;
            _lastDataBuffer[2] = (byte) (_targetCursorId >> 24);
            _lastDataBuffer[3] = (byte) (_targetCursorId >> 16);
            _lastDataBuffer[4] = (byte) (_targetCursorId >> 8);
            _lastDataBuffer[5] = (byte) _targetCursorId;
            _lastDataBuffer[6] = (byte) TargetingType;

            NetClient.Socket.Send(_lastDataBuffer);
            Mouse.CancelDoubleClick = true;
            ClearTargetingWithoutTargetCancelPacket();
        }

        private void TargetPacket(ushort graphic, ushort x, ushort y, sbyte z)
        {
            if (!IsTargeting)
            {
                return;
            }

            _lastDataBuffer[0] = 0x6C;

            _lastDataBuffer[1] = 0x01;

            _lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
            _lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
            _lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
            _lastDataBuffer[5] = (byte)_targetCursorId;

            _lastDataBuffer[6] = (byte)TargetingType;

            _lastDataBuffer[7] = (byte)(0 >> 24);
            _lastDataBuffer[8] = (byte)(0 >> 16);
            _lastDataBuffer[9] = (byte)(0 >> 8);
            _lastDataBuffer[10] = (byte)0;

            _lastDataBuffer[11] = (byte)(x >> 8);
            _lastDataBuffer[12] = (byte)x;

            _lastDataBuffer[13] = (byte)(y >> 8);
            _lastDataBuffer[14] = (byte)y;

            _lastDataBuffer[15] = (byte)(z >> 8);
            _lastDataBuffer[16] = (byte)z;

            _lastDataBuffer[17] = (byte)(graphic >> 8);
            _lastDataBuffer[18] = (byte)graphic;



            NetClient.Socket.Send_TargetXYZ(graphic,
                                            x,
                                            y,
                                            z,
                                            _targetCursorId,
                                            (byte)TargetingType);


            Mouse.CancelDoubleClick = true;
            ClearTargetingWithoutTargetCancelPacket();
        }
    }
}