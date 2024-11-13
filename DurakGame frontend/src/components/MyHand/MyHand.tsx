import React, { useState } from 'react';
import './MyHand.css';

interface IMyHandProps {
  cards: ICard[];
  selectedCard: ICard | null;
  setSelectedCard: React.Dispatch<React.SetStateAction<ICard | null>>
}

interface ICard {
    suit: string;
    value: number;
}


const MyHand: React.FC<IMyHandProps> = ({ cards, selectedCard, setSelectedCard }) => {
    return (
        <ul className="myhand">
            {cards.map((card, index) => 
                <li 
                    className={(selectedCard?.suit === card.suit && selectedCard?.value == card.value) ? `selected` : ''}
                    style={{ left: `${index*75}px` }} 
                    onClick={(e) => setSelectedCard(card)}
                    key={`${card.suit}${card.value}`}
                >
                    <img 
                        src={`./cards/${card.suit}/${card.value}.svg`} 
                        
                    />
                </li>
            )}
        </ul>
    )
};

export default MyHand;
