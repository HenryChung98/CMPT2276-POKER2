using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PokerHandEvaluator
{
    public sealed class HandResult
    {
        public HandRank Rank;
        public List<Rank> Tiebreakers; // ordered high¡÷low for comparisons
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

        // HIGH CARD: take top five ranks
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


    public static int Compare(HandResult a, HandResult b)
    {
        if (a.Rank != b.Rank) return a.Rank.CompareTo(b.Rank);
        // same category ¡÷ compare tiebreakers
        int n = System.Math.Min(a.Tiebreakers.Count, b.Tiebreakers.Count);
        for (int i = 0; i < n; i++)
        {
            int cmp = a.Tiebreakers[i].CompareTo(b.Tiebreakers[i]);
            if (cmp != 0) return cmp;
        }
        return 0;
    }
}
