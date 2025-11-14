public enum Suit { Clubs = 1, Diamonds = 2, Hearts = 3, Spades = 4 }
public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

public enum HandRank
{
    HighCard,
    OnePair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    RoyalFlush
}

public enum GameState
{
    Idle,
    PreFlop,
    Flop,
    Turn,
    River,
    Showdown
}