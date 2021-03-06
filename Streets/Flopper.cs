﻿using BluffinMuffin.HandEvaluator;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Samus
{
    public class Flopper
    {
        private static string CasinoToBot = Program.CasinoToBot;
        private static string BotToCasino = Program.BotToCasino;
        //  private static string DebugBotPath = Program.DebugBotPath;

        private static int TurnCard;
        private static int Rank;

        public static void Start(int[] cardNumbers, int preFlopRank, int dealerPosition, char action)
        {
            //  File.AppendAllText(debugBotPath, "\nEntered flop." + System.Environment.NewLine);
            //dealerPosition 2 goes first
            //read flop
            //sort hand
            //get rank
            //check draws
            //check or bet
            //go turning

            string[] cards = cardNumbers.Select(x => x.ToString()).ToArray();

            FileManipulation.CardTransform.Flop(cards, ref Program.CommunityCards); //format = KJQ123 -> now K1J2Q3 -> community cards being set inside using ref keyword.

            //File.AppendAllText(debugBotPath, string.Format("Read Flop cards: {0} {1} {2}", Program.CommunityCards[0], Program.CommunityCards[1], Program.CommunityCards[2]) + System.Environment.NewLine);
            
            //hand evaluator logic
            IStringCardsHolder[] players =
            {
                new Program.Player("Samus", Program.Samus.FirstCard.ToString(), Program.Samus.SecondCard.ToString(), Program.CommunityCards[0], Program.CommunityCards[1], Program.CommunityCards[2])
            }; 
            HandEvaluationResult bestFiveCarder = null;
            foreach (var p in HandEvaluators.Evaluate(players).SelectMany(x => x))
            {
                bestFiveCarder = HandEvaluators.Evaluate(p.CardsHolder.PlayerCards, p.CardsHolder.CommunityCards); // gets current best hand
            }

            Program.Samus.Hand = bestFiveCarder.Hand.ToString();
            //File.AppendAllText(debugBotPath, string.Format("Best five card hand post flop: {0}", bestFiveCarder + System.Environment.NewLine));
            HandStrategies.Draws.CheckForDraws(Program.Samus, Program.CommunityCards);
            // File.AppendAllText(debugBotPath, "Checked for Draws!" + System.Environment.NewLine);

            Rank = HandStrategies.PotOddsTolerance.GetEnhancedRankings(Program.Samus, bestFiveCarder);

            if (Rank == 10)
            {
                File.WriteAllText(BotToCasino, "c");
                //    File.AppendAllText(debugBotPath, "Changed bot file to 'c'." + System.Environment.NewLine);
            }
            else if (Rank > 10)
            {
                File.WriteAllText(BotToCasino, "r");
                //   File.AppendAllText(debugBotPath, "Changed bot file to 'r'." + System.Environment.NewLine);
            }
            else if (action == 'r')
            {
                File.WriteAllText(BotToCasino, "c");
                //  File.AppendAllText(debugBotPath, "Changed bot file to 'c'. because i raised pre-flop." + System.Environment.NewLine);
            }
            if(FileManipulation.Extractions.RaiseFound())
            {

                File.WriteAllText(BotToCasino, "f"); // f
                //  File.AppendAllText(debugBotPath, "Changed bot file to 'f'. I missed the flop COMPLETELY. + System.Environment.NewLine);
                Program.Folded = true;
                return;
            }
            else
            {
                File.WriteAllText(BotToCasino, "c"); // f
            }

            while (true)
            {
                if (FileManipulation.Listeners.BotFileChanged)
                {
                    FileManipulation.Listeners.BotFileChanged = false;
                    if (TurnFound()) //looking for turn card
                    {
                        // File.AppendAllText(DebugBotPath, "Turn Found" + System.Environment.NewLine);
                        return;
                    }
                }
            }
        }

        private static bool TurnFound()
        {
            string text = null;

            while (true)
            {
                if (FileManipulation.Extractions.IsFileReady(CasinoToBot))
                {
                    try
                    {
                        text = System.IO.File.ReadAllText(CasinoToBot);
                        break;
                    }
                    catch
                    {
                        continue;
                    }

                }
            }

            int index = 0;
            if (text.Contains("T")) // if T is found so has the turn card
            {
                foreach (var digit in text)
                {
                    ++index;
                    if (digit == 'T')
                    {
                        //File.AppendAllText(DebugBotPath, "Turn found here =  " + text + System.Environment.NewLine);
                        TurnCard = Convert.ToInt32(Regex.Match(text.Substring(index), @"\d+").Value);
                        FileManipulation.CardTransform.WriteCommunityCards(TurnCard, 3);
                        break;
                    }
                }
                return true;
            }
            else
                return false;
        }
    }
}
