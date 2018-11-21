﻿using System;
using System.Collections.Generic;
using SETextToSpeechMod.Processing;
using Sandbox.ModAPI;

namespace SETextToSpeechMod
{   
    public abstract class TimelineFactory : ISentenceReset
    {   
        const string SPACE = " ";

        public abstract int ClipLength { get; }
        public abstract int SyllableSize { get; }     
        public abstract int SpaceSize { get; }   

        //external feedback        
        public bool IsBusy { get; private set;}
        public bool HasAnOrder { get; private set;}

        /// <summary>
        /// Filled with input words and their output phonemes if project is debugging.
        /// </summary>
        public DebugOutputContainer PossibleDebugOutput {get; private set;}

        //loading data            
        protected Sentence sentence;
        bool previousWasSpace;
        int syllableMeasure;          
        protected int[] intonationArrayChosen;
        int spaceOffsetTemp;
        List <string> currentResults = new List <string>();

        public IList <TimelineClip> Timeline
        {
            get
            {
                return timelinesField.AsReadOnly();
            }
        }
        public List <TimelineClip> timelinesField = new List <TimelineClip>();    

        //objects     
        public Pronunciation Pronunciation { get; private set; }      
        protected Random rng = new Random();
        private SoundPlayer soundPlayerRef; 

        public TimelineFactory (SoundPlayer inputEmitter, Intonation intonationType)
        {       
            Pronunciation = new Pronunciation (intonationType);
            timelinesField.Capacity = OutputManager.MAX_LETTERS; //lists resize constantly when filling. better to know its limit and prevent that to increase performance;            
            soundPlayerRef = inputEmitter; //the reason im using a pointer is there is no need for a SoundPlayer per SentenceFactory.                                                            
            PossibleDebugOutput = new DebugOutputContainer();
        }

        /// <summary>
        /// Initialises a new sentence.
        /// </summary>
        /// <param name="inputSentence"></param>
        public void FactoryReset (Sentence inputSentence)
        {       
            IsBusy = false;
            HasAnOrder = true;

            sentence = inputSentence;
            
            previousWasSpace = false;
            syllableMeasure = 0;
            intonationArrayChosen = null;

            currentResults.Clear();

            Pronunciation.FactoryReset(inputSentence);
            timelinesField.Clear();            
            PossibleDebugOutput.Clear();
        }

        //this function will extract what phonemes it can from the sentence and save performance by taking its sweet time.
        public void RunAsync()
        {                     
            if (HasAnOrder &&
                IsBusy == false) //prevent factory from being spammed; one run per order.
            {
                IsBusy = true;

                MyAPIGateway.Parallel.For(0, sentence.Length, (i) => { 
                    AddPhonemes (i);
                });
                soundPlayerRef.PlaySentence (timelinesField);
                IsBusy = false;
                HasAnOrder = false;
            }   
        } 

        private void AddPhonemes (int currentIndex)
        {   
            currentResults = Pronunciation.GetLettersPronunciation (currentIndex);           

            for (int i = 0; i < currentResults.Count; i++)
            {             
                if (currentResults[i] == string.Empty) //AdjacentEvaluation() can return an empty string sometimes.
                {
                    currentResults.RemoveAt(i);

                    if (i == currentResults.Count)
                    {
                        break;
                    }
                }

                if (currentResults[i] != SPACE)
                {                                                   
                    AddToTimeline (currentResults[i]);                                            

                    if (syllableMeasure >= SyllableSize - 1)
                    {                            
                        IncrementSyllables();                       
                    }   

                    else
                    {
                        previousWasSpace = false;
                        syllableMeasure++;
                    }
                }

                else
                {
                    if (previousWasSpace == false) //prevents syllables and actual whitespace from combining.
                    {
                        IncrementSyllables();
                    }       
                        
                    else
                    {
                        previousWasSpace = false;
                    }                 
                }
            }

            if (OutputManager.IsDebugging)                
            {
                if (Pronunciation.PreviousProcessUsedDictionary)
                {
                    PossibleDebugOutput.AddDictionaryWord (Pronunciation.WordIsolator.Current, currentResults);
                }

                else
                {
                    PossibleDebugOutput.AddRuleBasedWord (Pronunciation.WordIsolator.Current, currentResults);
                }                
            }
        }

        private void AddToTimeline (string inputSound)
        {
            int startPoint = 0;                                                                      
                        
            if (timelinesField.Count != 0)
            {
                startPoint = timelinesField[timelinesField.Count - 1].StartPoint + spaceOffsetTemp + ClipLength;        
                spaceOffsetTemp = default (int); 
            }  
            timelinesField.Add (new TimelineClip (startPoint, inputSound));
        }

        private void IncrementSyllables()
        {
            previousWasSpace = true; //pronunciation class inserts spaces for low energy letters. i dont want double spaces so thats the purpose of this var.        {
            spaceOffsetTemp = SpaceSize;
            syllableMeasure = 0;
        } 
    }
}
