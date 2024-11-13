namespace DurakGame.Core
{
    public class Player
    {
        public readonly Guid guid;
        public string Name;
        public List<Card> Cards = new List<Card>();

        public string ConnectionId;

        public Player(string userName, string connectionId)
        {
            guid = Guid.NewGuid();
            Name = userName;
            ConnectionId = connectionId;
        }

        public void AddCardsFromMoveToPlayer(Move move)
        {
            foreach (Card card in move.GetMoveCards())
                Cards.Add(card);
        }

        public static bool ValidateUserName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }
    }
}
