﻿using System;
using SETextToSpeechMod.Processing;
using SETextToSpeechMod.VoiceCode.GladosVoice;

namespace SETextToSpeechMod
{
    public sealed class GladosVoice : TimelineFactory
    {
        public GladosVoice (SoundPlayer inputEmitter) : base(inputEmitter, new GladosIntonation()) { }

        public override int ClipLength
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public override int SpaceSize
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public override int SyllableSize
        {
            get
            {
                throw new InvalidOperationException();
            }
        }
    } 
}
