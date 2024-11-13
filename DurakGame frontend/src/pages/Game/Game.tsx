import './Game.css'

import * as signalR from "@microsoft/signalr";

import { useState, useEffect } from 'react';

import Deck from "../../components/Deck/Deck";
import EnemyHand from "../../components/EnemyHand/EnemyHand";
import MyHand from '../../components/MyHand/MyHand';

interface ICard {
    suit: string;
    value: number;
}

interface IStep {
    attackCard: ICard;
    defendCard?: ICard;
}

interface GameProps {
    connection: signalR.HubConnection | null;
    currentGameId: string;
    enemyName: string;
    deckCardsCount: number;
    cards: ICard[]
    clearData: () => void;
    enemyCardsCount: number;
    trump: ICard | null;
    steps: IStep[];
    isAttacker: boolean;
    currentMoveId: string;
    isGameStarted: boolean;
    isTrumpTaked: boolean;
}

const Game : React.FC<GameProps> = ({ connection, currentGameId, enemyName, deckCardsCount, clearData, cards, enemyCardsCount, trump, steps, isAttacker, isGameStarted, isTrumpTaked }) => {
    const [selectedCard, setSelectedCard] = useState<ICard | null>(null);
    const [canBeFinished, setCanBeFinished] = useState<boolean>(false);
    const [canBeTaked, setCanBeTaked] = useState<boolean>(false);

    useEffect(() => {   
        if (steps.length === 0) {
            setCanBeFinished(false)
            setCanBeTaked(false)
            return
        }

        if (isAttacker) {
            for (const step of steps) {
                if (step.defendCard == null) {
                    setCanBeFinished(false)
                    return
                }
            }

            setCanBeFinished(true)
        }
        else {
            let allDefend = true

            for (const step of steps) {
                if (step.defendCard == null) {
                    allDefend = false
                    break
                }
            }

            setCanBeTaked(!allDefend)
        }
    }, [steps])

    const leaveGame = async () => {
        await connection?.invoke("LeaveGame", currentGameId)
        clearData()
    };

    const defendCard = (step : IStep) => {
        if (step.defendCard) {
            return
        }

        if (selectedCard == null) {
            return
        }

        connection?.invoke("DefendFromCard", currentGameId, step.attackCard.value, step.attackCard.suit, selectedCard.value, selectedCard.suit)
    }

    const throwCard = () => {
        if (selectedCard == null) {
            return
        }

        if (steps.length !== 0) {
            connection?.invoke("ThrowCard", currentGameId, selectedCard?.value, selectedCard?.suit)
        }
        else {
            connection?.invoke("ThrowFirstCard", currentGameId, selectedCard?.value, selectedCard?.suit)
        }
    }

    const takeCards = () => {
        if (isAttacker)
            return

        connection?.invoke("TakeCards", currentGameId)
    }

    const dropCards = () => {
        if (!isAttacker)
            return

        connection?.invoke("DropCards", currentGameId)
    }

    return (
        <div className="game">
            <aside>
                {trump && !isTrumpTaked && 
                    <div className="trump__container">
                        <h3>Козырь</h3>
                        <img className="trump" src={`./cards/${trump?.suit}/${trump?.value}.svg`} /> 
                    </div>
                }
                
                <Deck cardsCount={deckCardsCount} />

                <div className="actions">
                    <button onClick={leaveGame}>Покинуть игру</button>
                    {isAttacker && canBeFinished && <button onClick={dropCards}>Бито</button>}
                    {!isAttacker && canBeTaked && <button onClick={takeCards}>Взять карты</button>}
                    {isGameStarted && <p className="gameStatus">{isAttacker ? 'Вы атакуете' : 'Вы защищаетесь'}</p>}
                </div>
            </aside>
            
            <div className="board">
                <EnemyHand
                    name={enemyName}
                    cardsCount={enemyCardsCount} 
                />
                
                <div 
                    className="move"
                    onClick={(e) => isAttacker && throwCard()}
                >
                    {steps.map((step, index) => 
                        <div key={index} className="step">
                            <div className="attacker">
                                <img
                                    className={!step.defendCard && !isAttacker ? 'isDefendable' : ''}
                                    src={`./cards/${step.attackCard.suit}/${step.attackCard.value}.svg`} 
                                    onClick={() => !isAttacker && defendCard(step)}
                                /> 
                            </div>
                            {step.defendCard && 
                                <div className="defender">
                                    <img src={`./cards/${step.defendCard.suit}/${step.defendCard.value}.svg`} /> 
                                </div>
                            }
                        </div>
                    )}
                </div>
                
                <MyHand 
                    cards={cards}
                    selectedCard={selectedCard}
                    setSelectedCard={setSelectedCard}
                />
            </div>
        </div>
    );
};

export default Game;
