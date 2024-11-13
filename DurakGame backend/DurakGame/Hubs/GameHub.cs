using Microsoft.AspNetCore.SignalR;
using DurakGame.Core;

namespace DurakGame.Hubs
{
    public class GameHub : Hub
    {
        private static List<Game> Games = new List<Game>();
        public async Task GetGames()
        {
            var gameSummaries = Games.Select(game => new
            {
                game.guid,
                Creator = game.Players.FirstOrDefault()?.Name
            }).ToList();

            await Clients.Caller.SendAsync("ReceiveGames", gameSummaries);
        }

        public async Task GetEnemyCardsCount(string gameId)
        {
            try
            {
                foreach (Game game in Games)
                {
                    if (game.guid.ToString() == gameId)
                    {
                        foreach (Player player in game.Players)
                        {
                            if (player.ConnectionId != Context.ConnectionId)
                            {
                                await Clients.Caller.SendAsync("EnemyCardsCount", player.Cards.Count);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task GetMyCards(string gameId)
        {
            try
            {
                foreach (Game game in Games)
                {
                    if (game.guid.ToString() == gameId)
                    {
                        foreach (Player player in game.Players)
                        {
                            if (player.ConnectionId == Context.ConnectionId)
                            {
                                var cards = player.Cards.Select(card => new
                                {
                                    suit = card.Suit.Name,
                                    value = (int)card.Value
                                }).ToArray();

                                await Clients.Caller.SendAsync("YourCards", cards);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task CreateGame(string userName)
        {
            try 
            {
                Game game = new Game(userName, Context.ConnectionId);
                Games.Add(game);

                string gameID = game.guid.ToString();

                await Groups.AddToGroupAsync(Context.ConnectionId, gameID);
                await Clients.Group(gameID).SendAsync("GameCreatedByUser", gameID);
                await Clients.All.SendAsync("GameCreated", new { gameID, userName });
            }
            catch (Exception ex) 
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task JoinGame(string gameId, string userName)
        {
            try 
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

                Game targetGame = GetCurrentGame(gameId);

                Player? creator = targetGame.Players[0];

                targetGame.JoinGame(userName, Context.ConnectionId);

                await Clients.Group(gameId).SendAsync("PlayerJoined", new { name = userName, connectionId = Context.ConnectionId, creatorName = creator?.Name });
                await Clients.Group(gameId).SendAsync("GameStarted", 
                    new { 
                        deckCardsCount = targetGame?.Deck.Count, 
                        gameId = gameId, 
                        trump = new {
                            suit = targetGame?.TrumpCard?.Suit.Name,
                            value = (int)targetGame?.TrumpCard?.Value
                        } 
                    });

            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }
        
        public async Task LeaveGame(string gameId)
        {
            try
            {
                Game game = GetCurrentGame(gameId);
                
                game.LeaveGame(Context.ConnectionId);

                if (game.isFinished)
                {
                    int gameIndex = Games.IndexOf(game);
                    Games.RemoveAt(gameIndex);

                    await Clients.All.SendAsync("GameRemoved", game.guid.ToString());
                    await Clients.Group(gameId).SendAsync("GameFinished", Context.ConnectionId);     
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task ThrowFirstCard(string gameId, CardValue cardValue, string cardSuit) 
        {
            try
            {
                Game currentGame = GetCurrentGame(gameId);
                Player currentPlayer = GetCurrentPlayer(currentGame, Context.ConnectionId);

                foreach (Card card in currentPlayer.Cards)
                {
                    if (card.Value == cardValue && cardSuit == card.Suit.Name)
                    {
                        currentGame.ThrowFirstCard(card, currentPlayer);

                        if (currentGame.isFinished && currentGame.Winner != null)
                        {
                            Games.Remove(currentGame);

                            await Clients.Group(gameId).SendAsync("GameFinishedWithWinner", currentGame.Winner?.ConnectionId);
                            await Clients.All.SendAsync("GameRemoved", currentGame.guid.ToString());
                        }
                        else
                        {
                            Move currentMove = currentGame.GetCurrentMove();

                            var message = new
                            {
                                moveId = currentMove.guid.ToString(),
                                gameId = currentGame.guid.ToString(),
                                steps = currentMove.Steps.Select(step => new {
                                    attackCard = new { suit = step.AttackCard.Suit.Name, value = (int)step.AttackCard.Value },
                                    defendCard = step.DefendCard == null ? null : new { suit = step.DefendCard?.Suit.Name, value = (int)step.DefendCard.Value }
                                }).ToArray()
                            };

                            await Clients.Group(gameId).SendAsync("UpdateSteps", message);
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task ThrowCard(string gameId, CardValue cardValue, string cardSuit)
        {
            try
            {
                Game currentGame = GetCurrentGame(gameId);
                Player currentPlayer = GetCurrentPlayer(currentGame, Context.ConnectionId);

                foreach (Card card in currentPlayer.Cards)
                {
                    if (card.Value == cardValue && cardSuit == card.Suit.Name)
                    {
                        currentGame.ThrowCard(card, currentPlayer);

                        if (currentGame.isFinished && currentGame.Winner != null)
                        {
                            Games.Remove(currentGame);

                            await Clients.Group(gameId).SendAsync("GameFinishedWithWinner", currentGame.Winner?.ConnectionId);
                            await Clients.All.SendAsync("GameRemoved", currentGame.guid.ToString());
                        }
                        else
                        {
                            Move currentMove = currentGame.GetCurrentMove();

                            var message = new
                            {
                                moveId = currentMove.guid.ToString(),
                                gameId = currentGame.guid.ToString(),
                                steps = currentMove.Steps.Select(step => new {
                                    attackCard = new { suit = step.AttackCard.Suit.Name, value = (int)step.AttackCard.Value },
                                    defendCard = step.DefendCard == null ? null : new { suit = step.DefendCard?.Suit.Name, value = (int)step.DefendCard.Value }
                                }).ToArray()
                            };

                            await Clients.Group(gameId).SendAsync("UpdateSteps", message);
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task DefendFromCard(string gameId, CardValue attackValue, string attackSuit, CardValue defendValue, string defendSuit)
        {
            try
            {
                Game currentGame = GetCurrentGame(gameId);
                Player currentPlayer = GetCurrentPlayer(currentGame, Context.ConnectionId);
                Move currentMove = currentGame.GetCurrentMove();

                Card attackCard = null;
                Card defendCard = null;

                foreach (Card card in currentPlayer.Cards)
                    if (card.Value == defendValue && defendSuit == card.Suit.Name)
                        defendCard = card;
               
                foreach (Step step in currentMove.Steps)
                    if (step.AttackCard.Value == attackValue && attackSuit == step.AttackCard.Suit.Name)
                        attackCard = step.AttackCard;

                if (attackCard == null || defendCard == null)
                    throw new Exception("Карта не найдена");

                currentGame.DefendFromCard(attackCard, defendCard, currentPlayer);

                if (currentGame.isFinished && currentGame.Winner != null)
                {
                    Games.Remove(currentGame);

                    await Clients.Group(gameId).SendAsync("GameFinishedWithWinner", currentGame.Winner?.ConnectionId);
                    await Clients.All.SendAsync("GameRemoved", currentGame.guid.ToString());
                }
                else
                {
                    var message = new
                    {
                        moveId = currentMove.guid.ToString(),
                        gameId = currentGame.guid.ToString(),
                        steps = currentMove.Steps.Select(step => new {
                            attackCard = new { suit = step.AttackCard.Suit.Name, value = (int)step.AttackCard.Value },
                            defendCard = step.DefendCard == null ? null : new { suit = step.DefendCard?.Suit.Name, value = (int)step.DefendCard.Value }
                        }).ToArray()
                    };

                    await Clients.Group(gameId).SendAsync("UpdateSteps", message);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task TakeCards(string gameId)
        {
            try
            {
                Game currentGame = GetCurrentGame(gameId);
                Player currentPlayer = GetCurrentPlayer(currentGame, Context.ConnectionId);

                currentGame.TakeCards(currentPlayer);

                var message = new
                {
                    isTrumpTaked = currentGame.isTrumpTaked,
                    attackerConnectionId = currentGame.Attacker.ConnectionId,
                    gameId = currentGame.guid.ToString(),
                    deckCardsCount = currentGame.Deck.Count
                };

                await Clients.Group(gameId).SendAsync("MoveFinished", message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task DropCards(string gameId)
        {
            try
            {
                Game currentGame = GetCurrentGame(gameId);

                currentGame.DropCards();

                var message = new
                {
                    isTrumpTaked = currentGame.isTrumpTaked,
                    attackerConnectionId = currentGame.Attacker.ConnectionId,
                    gameId = currentGame.guid.ToString(),
                    deckCardsCount = currentGame.Deck.Count
                };

                await Clients.Group(gameId).SendAsync("MoveFinished", message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        } 

        private static Player GetCurrentPlayer(Game game, string connectionId)
        {
            foreach (Player player in game.Players)
            {
                if (player.ConnectionId == connectionId)
                {
                    return player;
                }
            }

            throw new Exception("Игрок не найден");
        }

        private static Game GetCurrentGame(string gameId)
        {
            foreach (Game game in Games)
            {
                if (game.guid.ToString() == gameId)
                {
                    return game;
                }
            }

            throw new Exception("Игра не найдена");
        }
    }
}
