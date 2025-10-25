public enum Suit { Clubs = 1, Diamonds, Hearts, Spades }
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