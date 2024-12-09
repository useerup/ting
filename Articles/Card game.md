

    Suit = enum Hearts, Clubs, Spades, Diamonds

    CardValue = enum 
        Two, Three, Four, Five, Six, Seven, 
        Eight, Nine, Ten, Jack, Queen, King, Ace

    Cards = class CardValue * Suit

    Deck = man Cards