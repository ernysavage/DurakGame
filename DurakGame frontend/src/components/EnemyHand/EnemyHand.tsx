import React from 'react';
import './EnemyHand.css';

import backSide from '../../assets/cards/back.svg';

interface IEnemyHandProps {
  cardsCount: number;
  name: string;
}

const EnemyHand: React.FC<IEnemyHandProps> = ({ cardsCount, name }) => {
    const cardNumbers = Array.from({ length: cardsCount }, (_, index) => index + 1);

    return (
        <div className="enemyhand">
            <p className="enemy__name">{name}</p>
            <ul className="hand">
                {cardNumbers.map((number) => 
                    <li style={{ left: `${number*30}px`}} key={number}>
                        <img src={backSide} />
                    </li>
                )}
            </ul>
        </div>
        
    )
};

export default EnemyHand;
