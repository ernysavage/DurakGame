using System.ComponentModel.Design;
using System.Threading.Tasks.Dataflow;

namespace DurakGame.Core
{
    public class Game
    {
        public readonly Guid guid;
        public bool isFinished = false;
        public bool isTrumpTaked = false;

        public Card? TrumpCard;
        public Player Attacker;
        public Player? Winner;
        public List<Card> Deck = new List<Card>();
        public List<Move> Moves = new List<Move>();
        public List<Player> Players = new List<Player>();

        private int GameIteration = 0;

        public Game(string creatorName, string connectionId)
        {
            if (!Player.ValidateUserName(creatorName))
                throw new ArgumentException("Некорректное имя");

            guid = Guid.NewGuid();

            Player creator = new Player(creatorName, connectionId);

            Players.Add(creator);
            Attacker = creator;
        }

        public void JoinGame(string userName, string connectionId)
        {
            if (!Player.ValidateUserName(userName))
                throw new ArgumentException("Некорректное имя");

            if (!this.CheckSlots())
                throw new Exception("В игре нет мест!");

            Player player = new Player(userName, connectionId);

            Players.Add(player);

            this.StartGame();
        }

        public void LeaveGame(string connectionId)
        {
            bool isValidPlayer = false;

            foreach (Player player in Players)
            {
                if (player.ConnectionId == connectionId)
                {
                    isValidPlayer = true;
                }
            }

            if (!isValidPlayer)
                throw new Exception("Вы не участвуете в данной игре!");

            isFinished = true;
        }

        private bool CheckSlots()
        {
            return Players.Count != 2;
        }


        private void StartGame()
        {
            List<Suit> suits = new List<Suit> { new Suit("Hearts"), new Suit("Diamonds"), new Suit("Clubs"), new Suit("Spades") };

            foreach (Suit suit in suits)
            {
                foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
                {
                    Deck.Add(new Card(value, suit));
                }
            }

            List<Card> player1Cards = Card.GetRandomCards(Deck, 6);
            foreach (Card card in player1Cards)
                Players[0].Cards.Add(card);

            List<Card> player2Cards = Card.GetRandomCards(Deck, 6);
            foreach (Card card in player2Cards)
                Players[1].Cards.Add(card);

            TrumpCard = Card.GetRandomCards(Deck, 1)[0];
        }

        public void ThrowFirstCard(Card card, Player attacker)
        {
            if (attacker != Attacker)
                throw new Exception("Вы не являетесь атакующим!");

            attacker.Cards.Remove(card);

            if (attacker.Cards.Count == 0 && Deck.Count == 0)
            {
                Winner = attacker;
                isFinished = true;
            }
            
            Move newMove = new Move(GameIteration);
            Step newStep = new Step(card);

            newMove.Steps.Add(newStep);
            Moves.Add(newMove);
        }

        public void ThrowCard(Card card, Player attacker)
        {
            if (attacker != Attacker)
                throw new Exception("Вы не являетесь атакующим!");

            Move currentMove = GetCurrentMove();

            if (!Move.ValidateThrowMove(card, currentMove.Steps))
                throw new Exception("Нельзя подкинуть эту карту!");

            int notDefendedCards = 0;

            foreach (Step step in currentMove.Steps)
                if (step.DefendCard == null)
                    notDefendedCards += 1;

            Player defender = GetDefender();

            // Console.WriteLine("У дефендера {0} карт. Неотбитых карт {1}", defender.Cards.Count, notDefendedCards);

            if (currentMove.Steps.Count == 6 || defender.Cards.Count == notDefendedCards)
                throw new Exception("Нельзя больше подкидывать карты");

            attacker.Cards.Remove(card);

            if (attacker.Cards.Count == 0 && Deck.Count == 0)
            {
                SetWinner(attacker);
            }

            Step newStep = new Step(card);
            currentMove.Steps.Add(newStep);
        }

        public void DefendFromCard(Card attack, Card defend, Player defender)
        {
            if (!Move.ValidateDefendMove(attack, defend, TrumpCard))
                throw new Exception("Некорректный ход");

            Move currentMove = GetCurrentMove();

            defender.Cards.Remove(defend);

            foreach (Step step in currentMove.Steps)
                if (step.AttackCard == attack)
                    step.DefendCard = defend;

            if (defender.Cards.Count == 0 && Deck.Count == 0)
                SetWinner(defender);
        }

        public void TakeCards(Player defender)
        {
            Move move = GetCurrentMove();

            defender.AddCardsFromMoveToPlayer(move);

            move.FinishMove(this);
            GameIteration += 1;
        }

        public void DropCards()
        {
            Move move = GetCurrentMove();

            foreach (Step step in move.Steps)
                if (step.DefendCard == null)
                    throw new Exception("Не все карты побиты!");

            move.FinishMove(this);
            GameIteration += 1;
        }

        private void SetWinner(Player player)
        {
            Winner = player;
            isFinished = true;
        }

        public Move GetCurrentMove()
        {
            foreach (Move move in Moves)
                if (move.GameIteration == GameIteration)
                    return move;

            throw new Exception("Ход не найден");
        }

        public Player GetDefender()
        {
            Player defender = null;

            foreach (Player player in Players)
            {
                if (player != Attacker)
                {
                    defender = player;
                }
            }

            return defender;
        }
    }
}
