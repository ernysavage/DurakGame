namespace DurakGame.Core
{
    public enum CardValue
    {
        Six = 6,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    public class Card
    {
        public Suit Suit;
        public CardValue Value;

        public Card(CardValue value, Suit suit)
        {
            Value = value;
            Suit = suit;
        }

        public static List<Card> GetRandomCards(List<Card> cards, int count)
        {
            Random random = new Random();
            List<Card> selectedCards = new List<Card>();

            for (int i = 0; i < count; i++)
            {
                int index = random.Next(cards.Count);
                selectedCards.Add(cards[index]);
                cards.RemoveAt(index);
            }

            return selectedCards;
        }
    }
}
