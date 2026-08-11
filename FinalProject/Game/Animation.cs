#region File Description
//-----------------------------------------------------------------------------
// Animation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
    class Animation
    {
        public Texture2D Texture { get; }

        public float FrameTime { get; }

        public bool IsLooping { get; }

        public int FrameWidth { get; }

        public int FrameCount => Texture.Width / FrameWidth;

        public int FrameHeight => Texture.Height;

        public Animation(Texture2D texture, float frameTime, int frameWidth, bool isLooping)
        {
            Texture = texture;
            FrameTime = frameTime;
            FrameWidth = frameWidth;
            IsLooping = isLooping;
        }

        public Animation(Texture2D texture, float frameTime, bool isLooping)
        : this(texture, frameTime, texture.Height, isLooping) {}
    }
}