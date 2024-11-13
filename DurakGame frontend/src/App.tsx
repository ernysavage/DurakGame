import React, { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";

import "./App.css";
import Game from "./pages/Game/Game";

interface IGame {
    guid: string;
    creator: string;
}

interface ICard {
    suit: string;
    value: number;
}

interface IStep {
    attackCard: ICard;
    defendCard?: ICard;
}

/*
{
            attackCard: {suit: 'Hearts', value: 4},
            defendCard: {suit: 'Clubs', value: 3},
        },
        {
            attackCard: {suit: 'Hearts', value: 6},
            defendCard: {suit: 'Clubs', value: 8},
        },
        {
            attackCard: {suit: 'Hearts', value: 3},
            defendCard: {suit: 'Clubs', value: 2},
        },
        {
            attackCard: {suit: 'Hearts', value: 4},
        },
*/

const App: React.FC = () => {
    const [onGame, setOnGame] = useState<boolean>(false);
    const [isGameStarted, setIsGameStarted] = useState<boolean>(false);
    const [isAttacker, setIsAttacker] = useState<boolean>(false);
    const [enemyName, setEnemyName] = useState<string>('');
    const [currentGameId, setCurrentGameId] = useState<string>('');
    const [deckCardsCount, setDeckCardsCount] = useState<number>(36);
    const [cards, setCards] = useState<ICard[]>([])
    const [enemyCardsCount, setEnemyCardsCount] = useState<number>(0);
    const [trump, setTrump] = useState<ICard | null>(null);
    const [steps, setSteps] = useState<IStep[]>([]);
    const [currentMoveId, setCurrentMoveId] = useState<string>('');
    const [isTrumpTaked, setIsTrumpTaked] = useState<boolean>(false);

    const [name, setName] = useState("");
    const [games, setGames] = useState<IGame[]>([]);
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5211/game")
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            connection
                .start()
                .then(() => {
                    console.log("Connected!");

                    getGames()

                    connection.on("GameCreatedByUser", (data) => {
                        setOnGame(true);
                        setCurrentGameId(data)
                    });

                    connection.on("GameCreated", (data) => {
                        setGames(games => [...games, {guid: data.gameID, creator: data.userName}]);
                        setCurrentGameId(data.gameID)
                    });

                    connection.on("GameRemoved", (gameId) => {
                        console.log(gameId)
                        setGames(games => games.filter(game => game.guid != gameId));
                    });
                    
                    connection.on("PlayerJoined", (data) => {
                        setOnGame(true)

                        if (connection.connectionId !== data.connectionId) {
                            setEnemyName(data.name)
                            setIsAttacker(true) 
                        }
                        else {
                            setEnemyName(data.creatorName)
                            setIsAttacker(false)
                        }
                    });

                    connection.on("EnemyCardsCount", (data) => {
                        setEnemyCardsCount(data);
                    })

                    connection.on("GameStarted", (data) => {
                        setDeckCardsCount(data.deckCardsCount)
                        setIsGameStarted(true)
                        setTrump({ suit: data.trump.suit, value: data.trump.value })

                        console.log(data)
                        
                        connection.invoke("GetMyCards", data.gameId);
                        connection.invoke("GetEnemyCardsCount", data.gameId);
                    })

                    connection.on("YourCards", (data) => {
                        setCards(data);
                        console.log('Прилетели карты')
                        console.log(data)
                    })

                    connection.on("UpdateSteps", (data) => {
                        setCurrentMoveId(data.moveId)
                        setSteps(data.steps)

                        connection.invoke("GetMyCards", data.gameId);
                        connection.invoke("GetEnemyCardsCount", data.gameId);
                    })

                    connection.on("MoveFinished", (data) => {
                        console.log(data)

                        setIsAttacker(data.attackerConnectionId === connection.connectionId);
                        setSteps([])
                        setDeckCardsCount(data.deckCardsCount)
                        setIsTrumpTaked(data.isTrumpTaked)

                        connection.invoke("GetMyCards", data.gameId);
                        connection.invoke("GetEnemyCardsCount", data.gameId);

                        alert('Начался следующий ход')
                    })

                    connection.on("ReceiveGames", (gamesList) => {
                        setGames(gamesList);
                    });

                    connection.on("Error", (error) => {
                        alert(error);
                    })

                    connection.on("GameFinished", (leaverConnectionId) => {
                        clearData();

                        leaverConnectionId === connection.connectionId ? 
                            alert("Вы покинули игру. Игра завершена.") : alert("Соперник покинул игру. Игра завершена.")
                    });

                    connection.on("GameFinishedWithWinner", (winnerConnectionId) => {
                        if (connection.connectionId === winnerConnectionId) {
                            alert('Поздравляем! Вы выиграли!')
                        }
                        else {
                            alert(`Соперник выиграл! Игра завершена!`)
                        }
                        
                        clearData();
                    })
                })
                .catch((e) => console.log("Connection failed: ", e));
        }
    }, [connection]);

    const createGame = async (userName: string) => {
        await connection?.invoke("CreateGame", userName);
    };

    const getGames = async () => {
        await connection?.invoke("GetGames");
    };

    const joinGame = async (gameID : string, userName: string) => {
        await connection?.invoke("JoinGame", gameID, userName)
    };

    const clearData = () => {
        setOnGame(false)
        setIsGameStarted(false)
        setIsAttacker(false)
        setEnemyName('')
        setCurrentGameId('')
        setDeckCardsCount(36)
        setCards([])
        setEnemyCardsCount(0)
        setTrump(null)
        setSteps([])
        setCurrentMoveId('')
        setIsTrumpTaked(false)
    }

    if (onGame) {
        return <Game 
            connection={connection} 
            currentGameId={currentGameId}
            enemyName={enemyName}
            deckCardsCount={deckCardsCount}
            clearData={clearData}
            cards={cards}
            enemyCardsCount={enemyCardsCount}
            trump={trump}
            steps={steps}
            isAttacker={isAttacker}
            currentMoveId={currentMoveId}
            isGameStarted={isGameStarted}
            isTrumpTaked={isTrumpTaked}
        />
    };

    return (
        <div className="startPage">
            <h1>Durak Online</h1>
            
            <div className="form__group">
                <label htmlFor="name">Ваше имя</label>
                <input
                    type="text"
                    id="name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Введите своё имя"
                />
            </div>

            <button onClick={() => createGame(name)}>Создать игру</button>

            <h2>Список игр</h2>
            {games.length === 0 ? (
                <p>Игры отсутсвуют</p>
            ) : (
                <ul>
                    {games.map((game) => (
                        <li className="game__join" key={game.guid}>
                            <p>Игра игрока "{game.creator}"</p>
                            <button onClick={() => joinGame(game.guid, name)}>Присоединиться</button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default App;
