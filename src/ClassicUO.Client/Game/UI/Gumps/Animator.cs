#region license

// Copyright (c) 2023, OpenUO
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
//    This product includes software developed by ultima-tony - https://github.com/ultima-tony
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

using System.Collections.Generic;
using ClassicUO.Assets;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;

namespace ClassicUO.Game.UI.Gumps;

internal class AnimatorGump : Gump
{
    private ushort m_ID;
    private MobileAnimatedGumpPic m_Animation;
    private byte m_AnimationGroup;
    private byte m_Delay;
    private int m_FrameCount = -1;
    private bool m_Forward = true;
    private ArrowNumbersTextBox m_FrameCountTB;
    private BorderControl m_BCAnimate;
    private GumpPicTiled m_TransAnimate;
    private Label m_LabelAnimate;
    
    public AnimatorGump(World world, ushort id) : base(world, 0, 0)
    {
        m_ID = id;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;

        Draw();
    }

    public void Draw()
    {
        Clear();
        Width = 385;
        Height = 490;
        
        Add
        (
            new BorderControl
            (
                0,
                0,
                Width,
                Height,
                4
            )
        );

        Add
        (
            new GumpPicTiled
            (
                4,
                4,
                Width- 8,
                Height - 8,
                0x0A40
            )
            {
                Alpha = 0.5f
            }
        );
        
        Add( 
            m_Animation = 
                new MobileAnimatedGumpPic(
                    new List<AnimationInfo>()
                    {
                        new AnimationInfo(m_ID, m_AnimationGroup, (byte)Direction.Down, 0, m_Forward, m_FrameCount)
                    }, 50, 50, 250, 450, true, 1, 0, true)
        );

        Add(new Label($"Animation:", true, 0xFFFF, 0, 255, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT) { X = 10, Y = 10});

        var tb = new ArrowNumbersTextBox
        (
            140, 10, 80, 1, 1, 2000
        ) { Text = m_ID.ToString() };

        tb.OnChange += (textBox, i) =>
        {
            m_ID = (ushort)i;
            m_FrameCount = -1;
            BuildAnimation();
        };
        Add(tb);

        Add(new Label($"Animation Number:", true, 0xFFFF, 0, 255, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT) { X = 10, Y = 40});
        tb = new ArrowNumbersTextBox
        (
            140, 40, 80, 1, 0, 200
        ) { Text = m_AnimationGroup.ToString() };

        tb.OnChange += (textBox, i) =>
        {
            m_FrameCount = -1;
            var change = i - m_AnimationGroup;

            if (change > 0)
            {
                m_AnimationGroup = 0;
                for (int j = i; j < 200; j++)
                {
                    var frames = Client.Game.UO.Animations.GetAnimationFrames
                    (
                        m_ID, (byte)j, 0, out ushort hue,
                        out bool useUOP
                    );

                    if (frames.Length > 0)
                    {
                        m_AnimationGroup = (byte)j;
                        break;
                    }
                }
            }
            else
            {
                m_AnimationGroup = 0;
                for (int j = i; j > 0; j--)
                {
                    var frames = Client.Game.UO.Animations.GetAnimationFrames
                    (
                        m_ID, (byte)j, 0, out ushort hue,
                        out bool useUOP
                    );

                    if (frames.Length > 0)
                    {
                        m_AnimationGroup = (byte)j;
                        break;
                    }
                }
            }

            textBox.Text = $"{m_AnimationGroup}";
            BuildAnimation();
        };
        Add(tb);
        Add(new Label($"Frame Count:", true, 0xFFFF, 0, 255, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT) { X = 10, Y = 70});

        m_FrameCountTB = new ArrowNumbersTextBox
        (
            140, 70, 80, 1,
            0, 200
        );
        
        var frames = Client.Game.UO.Animations.GetAnimationFrames
        (
            m_ID, (byte)m_AnimationGroup, 0, out ushort hue,
            out bool useUOP
        );

        m_FrameCountTB.Text = $"{frames.Length}";

        m_FrameCountTB.OnChange += (box, i) =>
        {
            m_FrameCount = i;
            BuildAnimation();
        };
        
        
        Add(m_FrameCountTB);
        
        
        Add(new Label($"Delay:", true, 0xFFFF, 0, 255, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT) { X = 10, Y = 100});

        tb = new ArrowNumbersTextBox
        (
            140, 100, 80, 1, 0, 50
        ) { Text = m_AnimationGroup.ToString() };

        tb.OnChange += (textBox, i) =>
        {
            m_Delay = (byte)i;
            BuildAnimation();
        };
        
        
        Add(tb);
        
        var _myCheckbox = new Checkbox(0x0867, 0x0869)
        {
            X = 240,
            Y = 5,
            IsChecked = m_Forward
        };

        _myCheckbox.ValueChanged += (sender, args) =>
        {
            m_Forward = ((Checkbox)sender).IsChecked;
            BuildAnimation();
        };
        Add(_myCheckbox);
        
        _myCheckbox = new Checkbox(0x0867, 0x0869)
        {
            X = 240,
            Y = 35,
            IsChecked = m_Animation.Loop
        };

        _myCheckbox.ValueChanged += (sender, args) =>
        {
            m_Animation.Loop = ((Checkbox)sender).IsChecked;
            BuildAnimation();

            if (m_Animation.Loop)
            {
                m_BCAnimate.IsVisible = false;
                m_TransAnimate.IsVisible = false;
                m_LabelAnimate.IsVisible = false;
            }
            else
            {
                m_BCAnimate.IsVisible = true;
                m_TransAnimate.IsVisible = true;
                m_LabelAnimate.IsVisible = true;
            }
        };
        Add(_myCheckbox);
        
        Add(new Label($"Forward", true, 0xFFFF, 0, 255, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT) { X = 280, Y = 10});
        Add(new Label($"Loop", true, 0xFFFF, 0, 255, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT) { X = 280, Y = 40});

        Add
        (
            m_BCAnimate = new BorderControl
            (
                280,
                80,
                80,
                30,
                4
            )
            {
                IsVisible = false
            }
        );

        var borderButton = new GumpPicTiled
        (
            284, 84, 80 - 8, 30 - 8,
            0x0A40
        )
        {
            Alpha = 0.5f,
            IsVisible = false,
        };

        m_TransAnimate = borderButton;
        
        borderButton.MouseDown += (sender, args) =>
        {
            
            var frames = Client.Game.UO.Animations.GetAnimationFrames
            (
                m_ID, (byte)m_AnimationGroup, 0, out ushort hue,
                out bool useUOP
            );
            
            m_Animation.Index = m_Forward ? 0 : frames.Length - 1;
            m_Animation.AnimIndex = 0;
            m_Animation.FrameLimit = m_FrameCount == -1 ? frames.Length : m_FrameCount;
        };

        Add
        (
            borderButton
        );
        
        Add( m_LabelAnimate = 
                 new Label($"Animate", true, 0xFFFF, 0, 255, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT) { X = 296, Y = 87, IsVisible = false});
        
        //var new Button()
        //Add();

    }

    public void BuildAnimation()
    {
        
        var frames = Client.Game.UO.Animations.GetAnimationFrames
        (
            m_ID, (byte)m_AnimationGroup, 0, out ushort hue,
            out bool useUOP
        );

        if (frames.Length == 0)
            m_AnimationGroup = 0;

        if (m_FrameCount == -1)
            m_FrameCountTB.Text = $"{frames.Length}";

        if (m_FrameCount >= frames.Length)
        {
            m_FrameCount = frames.Length;
            m_FrameCountTB.Text = $"{frames.Length}";
        }

        m_Animation.Index = m_Forward ? 0 : frames.Length - 1;
        m_Animation.AnimIndex = 0;
        m_Animation.Delay = m_Delay;
        m_Animation.FrameLimit = m_FrameCount == -1 ? frames.Length : m_FrameCount;
        m_Animation.CurrentAnimation = new AnimationInfo(m_ID, m_AnimationGroup, (byte)Direction.Down, 0, m_Forward, m_FrameCount);
        m_Animation.Animations = new List<AnimationInfo>() { m_Animation.CurrentAnimation };
    }
}