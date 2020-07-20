using Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace app
{
    class EnemyTracker
    {
        public EnemyTracker()
        {

        }

        // This needs to be called every step with the GameState
        public void GatherIntel(GameState state)
        {
            
        }

        public Point Predict(long shipId, int numTurns)
        {
            throw new NotImplementedException();
        }

        public int NumTurnsCorrectlyPredicted()
        {
            throw new NotImplementedException();
        }
    }
}
