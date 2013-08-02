using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automata
{
    abstract class Automata
    {
        public delegate void Renderer(uint row, uint col, uint state);

        private uint[][,] _grids;
        private uint _allowed = 1;
        private uint _turn = 0;
        private object[] _locks = { new object(), new object() };
        public Automata(uint rows, uint cols, uint allowedStates)
        {
            _grids = new uint[2][,];
            _grids[0] = new uint[rows, cols];
            _grids[1] = new uint[rows, cols];
            _allowed = allowedStates;
        }

        public uint Rows
        {
            get { return (uint)_grids[0].GetLength(0); }
        }

        public uint Cols
        {
            get { return (uint)_grids[0].GetLength(1); }
        }

        protected uint[,] CurGrid
        {
            get { return _grids[_turn]; }
        }

        protected uint[,] NextGrid
        {
            get { return _grids[1 - _turn]; }
        }

        public bool SetState(uint row, uint col, uint state)
        {
            if (row >= Rows || col >= Cols || state >= _allowed)
                return false;
            lock (_locks[0])
            {
                CurGrid[row, col] = state;
            }
            return true;
        }

        public void AdvanceGrid()
        {
            lock (_locks[0])
            {
                advance();
                _turn = 1 - _turn;
            }
        }

        protected abstract void advance();

        public void Render(Renderer renderer)
        {
            uint[,] curGrid = new uint[Rows, Cols];
            lock (_locks[0])
            {
                Array.Copy(CurGrid, curGrid, curGrid.Length);
            }
            for (uint r = 0; r < Rows; r++)
                for (uint c = 0; c < Cols; c++)
                    renderer(r, c, curGrid[r, c]);
        }
    }
}
