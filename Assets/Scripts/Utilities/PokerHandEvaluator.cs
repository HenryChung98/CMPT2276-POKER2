using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public static class PokerHandEvaluator
{
    public sealed class HandResult
    {
        public HandRank Rank;
        public List<Rank> Tiebreakers; // ordered high��low for comparisons
        public string Description;

        public List<Rank> highlightedRank;
        public List<Rank> highlightedKickers;
    }

    public static HandResult EvaluateBestHand(List<CardData> cards)
    {
        // rank counts
        var rankGroups = cards
            .GroupBy(c => c.rank)
            .Select(g => new { Rank = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ThenByDescending(g => (int)g.Rank)
            .ToList();

        
        // Four of a Kind
        var fourofaKind = rankGroups.FirstOrDefault(g => g.Count == 4);
        if (fourofaKind != null)
        {
            var kickers = cards
                .Where(c => c.rank != fourofaKind.Rank)
                .Select(c => c.rank)
                .Distinct()
                .OrderByDescending(r => (int)r)
                .Take(1)
                .ToList();


            var desc = $"Four of a Kind ({fourofaKind.Rank}s) with kickers {string.Join(", ", kickers)}";
            return new HandResult
            {
                Rank = HandRank.FourOfAKind,
                Tiebreakers = new List<Rank> { fourofaKind.Rank }.Concat(kickers).ToList(),
                Description = desc,

                highlightedRank = new List<Rank> { fourofaKind.Rank },
                highlightedKickers = kickers.ToList(),

            };
        }

        // Three of a kind
        var threeofaKind = rankGroups.FirstOrDefault(g => g.Count == 3);
        if (threeofaKind != null)
        {
            // Full House
            var aPairforFullHouse = rankGroups.FirstOrDefault(f => f.Count >= 2 && f.Rank != threeofaKind.Rank);
            if (aPairforFullHouse != null)
            {
                var desC = $"Full House ({threeofaKind.Rank}s) with Pair ({aPairforFullHouse.Rank})";
                return new HandResult
                {
                    Rank = HandRank.FullHouse,
                    Tiebreakers = new List<Rank> { threeofaKind.Rank }
                    .Concat(new List<Rank> { aPairforFullHouse.Rank }).ToList(),
                    Description = desC,

                    highlightedRank = new List<Rank> { threeofaKind.Rank, aPairforFullHouse.Rank },
                    highlightedKickers = null,
                };
            }

            var kickers = cards
                .Where(c => c.rank != threeofaKind.Rank)
                .Select(c => c.rank)
                .Distinct()
                .OrderByDescending(r => (int)r)
                .Take(2)
                .ToList();

            var desc = $"Three of a Kind ({threeofaKind.Rank}s) with kickers {string.Join(", ", kickers)}";
            return new HandResult
            {
                Rank = HandRank.ThreeOfAKind,
                Tiebreakers = new List<Rank> { threeofaKind.Rank }.Concat(kickers).ToList(),
                Description = desc,


                highlightedRank = new List<Rank> { threeofaKind.Rank },
                highlightedKickers = kickers,
            };
        }
        
        // making linq table for straight cards
        var uniqueCards = cards
            .Select(g =>(int)g.rank)
            .Distinct()
            .OrderBy(r => r)
            .ToList();

        if (uniqueCards.Contains(14))
        {
            uniqueCards.Insert(0, 1);
        }

        // Straight
        for (int i = 0; i <= uniqueCards.Count - 5; i++) // prevent out of range
        {
            bool isConsecutive = true;

            for (int j = 0; j < 4; j++)
            {
                if (uniqueCards[i + j + 1] - uniqueCards[i + j] != 1) // This i + j + 1 makes the out of bound error
                {
                    isConsecutive = false;
                    break;
                }
            }

            if (isConsecutive)
            {
                var highest = uniqueCards[i + 4];
                var lowest = uniqueCards[i];

                var highestCard = (Rank)highest;
                var lowestCard = (Rank)lowest;

                var straightRanks = uniqueCards
                    .Skip(i)
                    .Take(5)
                    .Select(r => (Rank)r)
                    .ToList();

                var desc = $"Straight ({lowestCard}s) up to {highestCard}";

                return new HandResult
                {
                    Rank = HandRank.Straight,
                    Tiebreakers = uniqueCards
                        .Skip(i)
                        .Take(5)
                        .OrderByDescending(r => r)
                        .Select(r => (Rank)r)
                        .ToList(),
                    Description = desc,

                   highlightedRank = straightRanks,
                   highlightedKickers = null,
                };
            }
        }


        // making Linq table for organizing suit
        var suitGroups = cards
           .GroupBy(s => s.suit)
           .Select(g => new { Suit = g.Key, Count = g.Count()  })
           .OrderByDescending(g => g.Count)
           .ThenByDescending(g => (int)g.Suit)
           .ToList();

        // Flush
        var isFlush = suitGroups.FirstOrDefault(g => g.Count >= 5);
        if (isFlush != null)
        {
            var flushCards = cards
                .Where(c => c.suit == isFlush.Suit)
                .Select(c => (int)c.rank)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            // Ace-low handling
            if (flushCards.Contains(14))
                flushCards.Insert(0, 1);

            // Straight flush checking
            for (int i = 0; i <= flushCards.Count - 5; i++)
            {
                bool consecutive = true;
                for (int j = 0; j < 4; j++)
                {
                    if (flushCards[i + j + 1] - flushCards[i + j] != 1)
                    {
                        consecutive = false;
                        break;
                    }
                }

                if (consecutive)
                {
                    // Straight Flush found
                    var highest = flushCards[i + 4];
                    var lowest = flushCards[i];

                    //Royal Flush
                    if (highest == 14 && lowest == 10)
                    {
                        var royalFlushCards = flushCards
                            .Skip(i)
                            .Take(5)
                            .Select(r => (Rank)r)
                            .ToList();

                        var deSC = $"Royal Flush ({isFlush.Suit}s)";
                        return new HandResult
                        {
                            Rank = HandRank.RoyalFlush,
                            Tiebreakers = flushCards
                                .Skip(i)
                                .Take(5)
                                .OrderByDescending(r => r)
                                .Select(r => (Rank)r)
                                .ToList(),
                            Description = deSC,

                            highlightedRank = royalFlushCards, 
                            highlightedKickers = null,
                        };
                    }

                    // Straight Flush
                    var straightFlushCards = flushCards
                        .Skip(i)
                        .Take(5)
                        .Select(r => (Rank)r)
                        .ToList();

                    var desC = $"Straight Flush({isFlush.Suit}s) up to {highest}";
                    return new HandResult
                    {
                        Rank = HandRank.StraightFlush,
                        Tiebreakers = flushCards
                            .Skip(i)
                            .Take(5)
                            .OrderByDescending(r => r)
                            .Select(r => (Rank)r)
                            .ToList(),
                        Description = desC,

                        highlightedRank = straightFlushCards, 
                        highlightedKickers = null,
                    };
                }
            }

            // Flush
            var flushRanks = flushCards
                .OrderByDescending(r => r)
                .Take(5)
                .Select(r => (Rank)r)
                .ToList();

            var desc = $"Flush ({isFlush.Suit}s)";
            return new HandResult
            {
                Rank = HandRank.Flush, // representing what card rank
                Tiebreakers = flushCards
                    .OrderByDescending(r => r)
                    .Take(5)
                    .Select(r => (Rank)r) 
                    .ToList(),
                Description = desc,

                highlightedRank = flushRanks,
                highlightedKickers = null,
            };

        }

        // ONE PAIR?
        var pair = rankGroups.FirstOrDefault(g => g.Count == 2);
        if (pair != null)
        {
            // Two pairs
            var twoPair = rankGroups.FirstOrDefault(f => f.Count == 2 && f.Rank != pair.Rank);
            if (twoPair != null)
            {
                //Need this one for a very specific scenario. Will decide later to ignore the logic.
                var kIckers = cards
               .Where(c => c.rank != pair.Rank)
               .Select(c => c.rank)
               .Distinct()
               .OrderByDescending(r => (int)r)
               .Take(1)
               .ToList();

                var desC = $"Two Pairs ({pair.Rank}) with another pair ({twoPair.Rank}) and kickers {string.Join(", ", kIckers)}";
                return new HandResult
                {
                    Rank = HandRank.TwoPair,
                    Tiebreakers = new List<Rank> { pair.Rank }.Concat(new List<Rank> { twoPair.Rank }).Concat(kIckers).ToList(),
                    Description = desC,

                    highlightedRank = new List<Rank> { twoPair.Rank },
                    highlightedKickers = kIckers,
                };
            }

            // pair rank first, then best three kickers
            var kickers = cards
                .Where(c => c.rank != pair.Rank)
                .Select(c => c.rank)
                .Distinct()
                .OrderByDescending(r => (int)r)
                .Take(3)
                .ToList();

            var desc = $"One Pair ({pair.Rank}s) with kickers {string.Join(", ", kickers)}";
            return new HandResult
            {
                Rank = HandRank.OnePair,
                Tiebreakers = new List<Rank> { pair.Rank }.Concat(kickers).ToList(),
                Description = desc,

                highlightedRank = new List<Rank> { pair.Rank },
                highlightedKickers = kickers,
            };
        }

        // HIGH CARD: take top five ranks
        if (pair == null)
        {
            var topFive = cards
                .Select(c => c.rank)
                .Distinct()
                .OrderByDescending(r => (int)r)
                .Take(5)
                .ToList();

            var high = topFive.First();
            var highDesc = $"High Card ({high}) with kickers {string.Join(", ", topFive.Skip(1))}";
            return new HandResult
            {
                Rank = HandRank.HighCard,
                Tiebreakers = topFive,
                Description = highDesc,
                highlightedRank = new List<Rank> { topFive.First() }, // Highest card green
                highlightedKickers = topFive.Skip(1).ToList(), // Rest yellow
            };
        }

        return new HandResult { }; // this will never get executed but need this for compilation sake.

    }

    public static int Compare(HandResult a, HandResult b)
    {
        if (a.Rank != b.Rank) return a.Rank.CompareTo(b.Rank);
        // same category �� compare tiebreakers
        int n = System.Math.Min(a.Tiebreakers.Count, b.Tiebreakers.Count);
        for (int i = 0; i < n; i++)
        {
            int cmp = a.Tiebreakers[i].CompareTo(b.Tiebreakers[i]);
            if (cmp != 0) return cmp;
        }
        return 0;
    }

}



// Might need another variable for tiebreaker before concatenation
// If both get the same rank, compare the kickers. (done)