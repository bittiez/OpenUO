using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ClassicUO.Assets;
using ClassicUO.Game.GameObjects;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls
{
    internal class AnimationInfo
    {
        public readonly ushort BodyValue;
        public readonly byte AnimationGroup;
        public readonly byte Direction;
        public readonly ushort Hue;
        public readonly bool Mirror;
        public readonly bool Forward;
        public readonly int FrameCount;

        public AnimationInfo(ushort bodyValue, byte animationGroup, byte direction, ushort hue, bool forward, int frameCount = -1)
        {
            BodyValue = bodyValue;
            AnimationGroup = animationGroup;
            Direction = direction;
            Mirror = true;
            Forward = forward;
            FrameCount = frameCount;
            AnimationsLoader.Instance.GetAnimDirection(ref Direction, ref Mirror);
            Hue = hue;
        }
    }
    
    internal class MobileAnimatedGumpPic : GumpPicBase
    {
        public bool IsPartialHue { get; set; }
        public ushort[] BodyValues;
        public int Index = 0;
        public int AnimIndex = 0;
        public uint LastChanged;
        public bool Loop;
        public bool IsUOP;
        public bool Forward;
        public int Delay = 0;
        public float Scale = 1.25f;
        public int FrameLimit;
        public bool Centered = false;
        public List<AnimationInfo> Animations;
        public AnimationInfo CurrentAnimation;
        
        public MobileAnimatedGumpPic(List<string> parts)
        {
            Forward = true;
            Loop = true;
            X = int.Parse(parts[1]);
            Y = int.Parse(parts[2]);
            Width = int.Parse(parts[3]);
            Height = int.Parse(parts[4]);
            Scale = float.Parse(parts[5]);
            Centered = bool.Parse(parts[6]);

            int remaining = (parts.Count - 7) / 4;

            Animations = new List<AnimationInfo>();

            for (int i = 0; i < remaining; i++)
            {
                int offset = 7 + (i * 4);
                ushort bodyValue = UInt16.Parse(parts[offset]);
                byte animationGroup = byte.Parse(parts[offset + 1]);
                byte direction = byte.Parse(parts[offset + 2]);
                ushort hue = UInt16.Parse(parts[offset + 3]);
                
                Animations.Add(new AnimationInfo(bodyValue, animationGroup, direction, hue, true, -1));
            }

            IsFromServer = true;
            
            
            if (Animations.Count == 0)
            {
                Console.WriteLine("Unable to find animation for declared gump mobile animation");
                Dispose();
                return;
            }

            CurrentAnimation = Animations[0];

        }

        public MobileAnimatedGumpPic(List<AnimationInfo> animations, int x, int y, int width, int height, bool loop, float scale = 1, int delay = 0, bool centered = false)
        {
            LastChanged = Time.Ticks;
            Delay = delay;
            X = x;
            Y = y;
            Animations = animations;
            Loop = loop;
            Scale = scale;
            Centered = centered;

            Width = width;
            Height = height;

            if (animations == null || animations.Count == 0)
            {
                Console.WriteLine("Unable to find animation for declared gump mobile animation");
                Dispose();
                return;
            }

            CurrentAnimation = animations[0];
            
            var frames = Client.Game.UO.Animations.GetAnimationFrames
            (
                CurrentAnimation.BodyValue, CurrentAnimation.AnimationGroup, CurrentAnimation.Direction, out ushort hue,
                out IsUOP, false
            );
            FrameLimit = CurrentAnimation.FrameCount == -1 ? frames.Length : CurrentAnimation.FrameCount;
            if (!CurrentAnimation.Forward && CurrentAnimation.FrameCount > 0)
                Index = CurrentAnimation.FrameCount;
            else if (!Forward)
            {
                FrameLimit = Index = frames.Length;
            }
            
        }
        public override void Dispose()
        {
            //Client.Game.Scene.Audio.StopMusic();
            base.Dispose();

        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            
            
            ushort hue = CurrentAnimation.Hue;
            var frames = Client.Game.UO.Animations.GetAnimationFrames
            (
                CurrentAnimation.BodyValue, CurrentAnimation.AnimationGroup, CurrentAnimation.Direction, out hue,
                out IsUOP, false
            );
            
            if (IsDisposed)
                return false;

            if (AnimIndex >= Animations.Count)
            {
                if (!Loop)
                    return false;
            }

            //if (DateTime.UtcNow - LastChanged > TimeSpan.FromSeconds((double) 1 / (IsUOP ? 18 : 12.5)))
            if (LastChanged + (80 * (Delay + 2)) < Time.Ticks)
            {
                LastChanged = Time.Ticks;

                if (CurrentAnimation.Forward)
                    Index++;
                else
                    Index--;

                int tries = 0;

                while (tries < 100 && (frames.Length == 0 || Index >= FrameLimit || Index < 0))
                {
                    tries++;
                    if (Index >= FrameLimit || Index < 0)
                    {
                        Index = 0;
                        AnimIndex++;

                        if (AnimIndex >= Animations.Count)
                        {
                            if (!Loop)
                                return false;
                            AnimIndex = 0;
                        }

                        CurrentAnimation = Animations[AnimIndex];

                        frames = Client.Game.UO.Animations.GetAnimationFrames
                        (
                            CurrentAnimation.BodyValue, CurrentAnimation.AnimationGroup, CurrentAnimation.Direction, out hue,
                            out IsUOP, false
                        );
                        
                        if (!CurrentAnimation.Forward)
                        {
                            if (CurrentAnimation.FrameCount == -1)
                            {
                                FrameLimit = Index = frames.Length - 1;
                            }
                            else
                            {
                                FrameLimit = Index = CurrentAnimation.FrameCount;
                            }
                        }
                        else
                        {
                            if (CurrentAnimation.FrameCount == -1)
                                FrameLimit = frames.Length - 1;
                            else
                                FrameLimit = CurrentAnimation.FrameCount;

                        }
                    }
                }

                if (tries >= 100)
                {
                    //Dispose();
                    return false;
                }
            }

            if (frames.Length == 0)
                return false;

            if (Index >= frames.Length || Index < 0)
                Index = 0;
            Hue = CurrentAnimation.Hue;

            var spriteInfo = frames[Index];
            
            Vector3 hueVec = ShaderHueTranslator.GetHueVector
            (
                Hue,
                IsPartialHue,
                Alpha,
                true
            );
            
            Rectangle rect = spriteInfo.UV;
            bool charIsSitting = false;

            int diffY = (spriteInfo.UV.Height + spriteInfo.Center.Y) - 0;

            int value = Math.Max(1, diffY);
            int count = Math.Max((spriteInfo.UV.Height / value) + 1, 2);

            rect.Height = Math.Min(value, rect.Height);
            int remains = spriteInfo.UV.Height - rect.Height;

            int tiles = CurrentAnimation.Direction % 2 == 0 ? 2 : 2;

            if (spriteInfo.Texture == null)
                return false;

            Vector2 pos;
            if (Centered)
            {
                pos = new Vector2(x + ((Width - rect.Width) / 2), y + ((Height - rect.Height) / 2));
            }
            else
            {
                int posX = x;
                int posY = y;

                if (CurrentAnimation.Mirror)
                {
                    posX -= spriteInfo.UV.Width - spriteInfo.Center.X;
                }
                else
                {
                    posX -= spriteInfo.Center.X;
                }

                posY -= spriteInfo.UV.Height + spriteInfo.Center.Y;
                pos = new Vector2(posX, posY);
            }

            for (int i = 0; i < count; ++i)
            {
                batcher.Draw
                (
                    spriteInfo.Texture,
                    pos,
                    rect,
                    hueVec,
                    0f,
                    Vector2.Zero,
                    Scale,
                    CurrentAnimation.Mirror ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f + (i * tiles)
                );

                pos.Y += rect.Height;
                rect.Y += rect.Height;
                rect.Height = remains;
                remains -= rect.Height;
            }
            


            return base.Draw(batcher, x, y);
        }
    }
}