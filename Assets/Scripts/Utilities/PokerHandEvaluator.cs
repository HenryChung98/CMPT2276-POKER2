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
                    //Tiebreakers = new List<Rank> { pair.Rank }.Concat(new List<Rank> { twoPair.Rank }).ToList(),
                    Description = desC
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
                Description = desc
            };
        }

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
                Description = desc
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
                    Description = desC
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
                Description = desc
            };
        }
        
        // making linq table for straight cards
        var uniqueCards = cards
            .Select(g =>(int)g.rank)
            .Distinct()
            .Orderby (r => r)
            .ToList();

        if (uniqueCards.Contains(14))
        {
            uniqueCards.Insert(0, 1);
        }
        
        // Straight
        for (int i = 0; i <= uniqueCards.Count; i++)
        {
            bool isConsecutive = true;
            for (int j = 0; j < 4; j++)
            {
                if (uniqueCards[i + j + 1] - uniqueCards[i + j] != 1)
                {
                    isConsecutive = false;
                    break;
                }
            }

            if (isConsecutive)
            {
                var highest = uniqueCards[i + 4];
                var lowest = uniqueCards[i];

                if (highest == 14 && lowest == 10)
                {
                    var desc = $"Straight ({uniqueCards.Rank}s) up to {highest}";
                    return new HandResult
                    {
                        Rank = HandRank.Straight,
                        Tiebreakers = flushCards
                            .Skip(i)
                            .Take(5)
                            .OrderByDescending(r => r)
                            .Select(r => (Rank)r)
                            .ToList(),
                        Description = desc
                    };
                }
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
                            Description = deSC
                        };
                    }


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
                        Description = desC
                    };
                }
            }

            var desc = $"Flush ({isFlush.Suit}s)";
            return new HandResult
            {
                Rank = HandRank.Flush, // representing what card rank
                Tiebreakers = flushCards
                    .OrderByDescending(r => r)
                    .Take(5)
                    .Select(r => (Rank)r) 
                    .ToList(),
                Description = desc
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
                Description = highDesc
            };
        }

        return new HandResult { }; // this will never get execute but need this for compilation sake.

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



/*
public static bool isFlush(List<CardData> holeCard, List<CardData> communityCard)
{
    List<CardData> allcardData = new List<CardData>();
    allcardData.AddRange(holeCard);
    allcardData.AddRange(communityCard);

    var occurence = new Dictionary<Suit, int>();

    foreach (var i in allcardData)
    {
        if (!occurence.ContainsKey(i.suit))
        {
            occurence[i.suit] = 0; // Set the value of the that key to 0.
        }
        occurence[i.suit]++;
    }

    foreach (var j in occurence) // check again.
    {
        if (occurence[j].value >= 5)
            return true;
    }
    return false;
}

public static bool isFourofaKind(List<CardData> holeCard, List<CardData> communityCard)
{
    List<CardData> allcardData = new List<CardData>();
    allcardData.AddRange(holeCard);
    allcardData.AddRange(communityCard);

    var occurence = new Dictionary<Rank, int>();

    foreach (var i in allcardData)
    {
        if (!occurence.ContainsKey(i.rank))
        {
            occurence[i.rank] = 0;
        }
        occurence[i.rank]++;
    }

    foreach (var j in occurence.Values)
    {
        if (j >= 4)
            return true;
    }
    return false;
}

public static bool isFullhouse(List<CardData> holeCard, List<CardData> communityCard)
{
    return isThreeofaKind(holeCard, communityCard) && isaPair(holeCard, communityCard);
}
public static bool isThreeofaKind(List<CardData> holeCard, List<CardData> communityCard)
{
    List<CardData> allcardData = new List<CardData>();
    allcardData.AddRange(holeCard);
    allcardData.AddRange(communityCard);

    var occurence = new Dictionary<Rank, int>();

    foreach (var i in allcardData)
    {
        if (!occurence.ContainsKey(i.rank))
        {
            occurence[i.rank] = 0;
        }
        occurence[i.rank]++;
    }

    foreach (var j in occurence.Values)
    {
        if (j == 3)
            return true;
    }
    return false;
}

public static bool isTwoPair(List<CardData> holeCard, List<CardData> communityCard)
{
    List<CardData> allcardData = new List<CardData>();
    allcardData.AddRange(holeCard);
    allcardData.AddRange(communityCard);

    var occurence = new Dictionary<Rank, int>();

    foreach (var i in allcardData)
    {
        if (!occurence.ContainsKey(i.rank))
        {
            occurence[i.rank] = 0;
        }
        occurence[i.rank]++;
    }

    int pairCount = 0;
    foreach (var j in occurence.Values)
    {
        if (j == 2)
        {
            pairCount++;
        }
    }
    return pairCount >= 2;
}

public static bool isaPair(List<CardData> holeCard, List<CardData> communityCard)
{
    List<CardData> allcardData = new List<CardData>();
    allcardData.AddRange(holeCard);
    allcardData.AddRange(communityCard);

    var occurence = new Dictionary<Rank, int>();

    foreach (var i in allcardData)
    {
        if (!occurence.ContainsKey(i.rank))
        {
            occurence[i.rank] = 0;
        }
        occurence[i.rank]++;
    }

    foreach (var j in occurence.Values)
    {
        if (j == 2)
            return true;
    }
    return false;
}
}
*/