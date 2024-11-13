namespace DurakGame.Core
{
    public class Step
    {
        public Guid guid;
        public Card AttackCard;
        public Card DefendCard = null;

        public Step(Card attackCard)
        {
            AttackCard = attackCard;
        }
    }
}
