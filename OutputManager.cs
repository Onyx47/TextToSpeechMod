﻿using System;
using System.Collections.Generic;

namespace SETextToSpeechMod
{
    static class OutputManager
    {
        public const int MAX_LETTERS = 100;
        const int UPDATES_INTERVAL = 60;
        const int TOTAL_SIMULTANEOUS_SPEECHES = 5;
        const int DOESNT_EXIST = -1;

        public static bool RunSpeechPlayback { get; private set;}
        static int timer;
        static int[] typeIndexes;
        static Type localPlayersField = POSSIBLE_OUTPUTS.HawkingType;      

        /// <summary>
        /// The only accepted types are contained in public POSSIBLE_OUTPUTS 
        /// </summary>
        public static Type LocalPlayersVoice
        {
            get
            {
                return localPlayersField;  
            }

            set
            {
                if (GetOutputTypesIndex (value) != DOESNT_EXIST)
                {
                    localPlayersField = value;
                }
            }
        }
        public static bool Debugging { get; set; } //global state which dictates whether current run of project is game dependant.

        /// <summary>
        /// It's important to define the elements of the list only at compilation time.
        /// Same sized groups of sentence types are ordered by their position in POSSIBLE_OUTPUTS.
        /// </summary>
        public static IList <SentenceFactory> Speeches
        {
            get
            {
                return speechesField.AsReadOnly();
            }
        }
        public static List <SentenceFactory> speechesField;

        static OutputManager()
        {
            speechesField = new List <SentenceFactory>();   
            FactoryReset();
        }

        public static void FactoryReset()
        {
            RunSpeechPlayback = false;
            typeIndexes = new int[POSSIBLE_OUTPUTS.AllOptions.Count];
            speechesField.Clear();

            for (int i = 0; i < POSSIBLE_OUTPUTS.AllOptions.Count; i++)
            {
                for (int k = 0; k < TOTAL_SIMULTANEOUS_SPEECHES; k++)
                {     
                    speechesField.Add (Activator.CreateInstance (POSSIBLE_OUTPUTS.AllOptions[i]) as SentenceFactory);
                }                
            }
        }

        public static void Run()
        {
            if (RunSpeechPlayback == true)
            {
                RunSpeechPlayback = false; 

                if (timer <= 0) //im hoping that a little distance will prevent the oscillating position whch annoys ear drums.
                {
                    timer = UPDATES_INTERVAL;
                    SoundPlayer.UpdatePosition();                    
                }

                else
                {
                    timer--;
                }               

                for (int i = 0; i < speechesField.Count; i++) 
                {                    
                    if (speechesField[i].FinishedPlaying == false)
                    {
                        RunSpeechPlayback = true;
                        speechesField[i].Run();
                    }
                }
            }
        }

        /// <summary>
        /// Returns integer indicating the type is indexed in struct POSSIBLE_OUTPUTS.
        /// Can be -1 if no match found.
        /// </summary>
        /// <param name="scrutinizedType"></param>
        /// <returns></returns>
        private static int GetOutputTypesIndex (Type scrutinizedType)
        {
            int outcome = DOESNT_EXIST;

            for (int i = 0; i < POSSIBLE_OUTPUTS.AllOptions.Count; i++)
            {
                if (POSSIBLE_OUTPUTS.AllOptions[i].Equals (scrutinizedType))
                {
                    outcome = i;
                }
            }
            return outcome;
        }

        /// <summary>
        /// The only accepted types are contained in public struct POSSIBLE_OUTPUTS
        /// </summary>
        /// <param name="validVoiceType"></param>
        /// <param name="sentence"></param>
        public static void CreateNewSpeech (Type validVoiceType, string inputSentence)
        {
            int currentTypeIndex = GetOutputTypesIndex (validVoiceType);

            if (currentTypeIndex != DOESNT_EXIST)
            {
                RunSpeechPlayback = true;                
                int firstTypeInstance = currentTypeIndex * TOTAL_SIMULTANEOUS_SPEECHES;

                if (typeIndexes[currentTypeIndex] >= TOTAL_SIMULTANEOUS_SPEECHES)
                {
                    typeIndexes[currentTypeIndex] = 0;                    
                }      
                int NewSpeechIndex = firstTypeInstance + typeIndexes[currentTypeIndex];
                speechesField[NewSpeechIndex].FactoryReset (inputSentence); //The purpose of this method is to reuse instances of sentencefactory instead of instantiating every new sentence.
                typeIndexes[currentTypeIndex]++;                         
            }
        }
    }

    public struct POSSIBLE_OUTPUTS
    {        
        public static Type MarekType { get; private set; }
        public static Type HawkingType { get; }
        public static Type GLADOSType { get; }
        
        public static IList <Type> AllOptions
        {
            get
            {
                return allOptionsField.AsReadOnly();
            }
        }
        private static List <Type> allOptionsField;

        public static int AutoSignatureSize { get; }

        static POSSIBLE_OUTPUTS()
        {
            MarekType = typeof (MarekVoice);
            HawkingType = typeof (HawkingVoice);
            GLADOSType = typeof (GLADOSVoice);
            allOptionsField = new List <Type>();
            allOptionsField.Add (MarekType);
            allOptionsField.Add (HawkingType);
            allOptionsField.Add (GLADOSType);

            for (int i = 0; i < allOptionsField.Count; i++)
            {
                if (allOptionsField[i].ToString().Length > AutoSignatureSize)
                {
                    AutoSignatureSize = allOptionsField[i].ToString().Length;
                }
            }
        }
    } 
}
