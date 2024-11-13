namespace DurakGame.Core
{
    public class Move
    {
        public Guid guid;
        public List<Step> Steps = new List<Step>();
        public int GameIteration;
        
        public Move(int gameIteration)
        {
            GameIteration = gameIteration;
            guid = Guid.NewGuid();
        }

        public static bool ValidateThrowMove(Card throwCard, List<Step> steps)
        {
            foreach (Step step in steps)
                if (step.AttackCard.Value == throwCard.Value || (step.DefendCard != null && step.DefendCard.Value == throwCard.Value))
                    return true;
            
            return false;
        }

        public static bool ValidateDefendMove(Card attack, Card defend, Card trump)
        {
            bool isDefendTrump = defend.Suit.Name == trump.Suit.Name;
            bool isAttackTrump = attack.Suit.Name == trump.Suit.Name;

            if (defend.Suit.Name == attack.Suit.Name)
                return defend.Value > attack.Value;
            
            if (isDefendTrump && !isAttackTrump)
                return true;
            
            return false;
        }

        public List<Card> GetMoveCards()
        {
            List <Card> cards = new List<Card>();

            foreach (Step step in Steps)
            {
                cards.Add(step.AttackCard);

                if (step.DefendCard != null)
                    cards.Add(step.DefendCard);
            }

            return cards;
        }

        public void FinishMove(Game game)
        {
            Player defender = null;

            foreach (Player player in game.Players)
            {
                if (player != game.Attacker)
                    defender = player;

                if (player.Cards.Count < 6)
                {
                    int delta = 6 - player.Cards.Count;

                    List<Card> cardsForPlayer = new List<Card>();

                    if (game.Deck.Count >= delta)
                    {
                        cardsForPlayer = Card.GetRandomCards(game.Deck, delta);
                    }
                    else if (game.Deck.Count > 0)
                    {
                        cardsForPlayer = Card.GetRandomCards(game.Deck, game.Deck.Count);
                    }

                    foreach (Card card in cardsForPlayer)
                        player.Cards.Add(card);

                    if (!game.isTrumpTaked && game.Deck.Count == 0 && player.Cards.Count < 6)
                    {
                        player.Cards.Add(game.TrumpCard);
                        game.isTrumpTaked = true;
                    }
                       
                }
            }

            game.Attacker = defender;
        }
    }
}
