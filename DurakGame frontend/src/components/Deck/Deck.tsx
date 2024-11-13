import React from 'react'
import './Deck.css'

import backSide from '../../assets/cards/back.svg' 

interface IDeckProps {
    cardsCount: number
}

const Deck : React.FC<IDeckProps> = ({ cardsCount }) => {
    const cardNumbers = Array.from({ length: cardsCount }, (_, index) => index + 1);

    return (
        <div className="deck__container">
            <p className="count">{cardsCount}</p>
            <ul className="deck">
                {cardNumbers.map((number) => 
                    <li style={{ left: `${number*3}px`}} key={number}>
                        <img src={backSide} />
                    </li>
                )}
            </ul>
        </div>
       
    )
}

export default Deck