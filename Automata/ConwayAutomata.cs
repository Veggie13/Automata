using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automata
{
    class ConwayAutomata : Automata
    {
        public ConwayAutomata(uint rows, uint cols)
            : base(rows, cols, 2)
        {
        }

        static int[] LookupDR;
        static int[] LookupDC;

        static ConwayAutomata()
        {
            LookupDR = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
            LookupDC = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        }

        protected override void advance()
        {
            uint[,] curGrid = CurGrid;
            uint[,] nextGrid = NextGrid;

            for (uint r = 0; r < Rows; r++)
            {
                for (uint c = 0; c < Cols; c++)
                {
                    uint count = 0;
                    for (int p = 0; p < 8; p++)
                    {
                        int dr = LookupDR[p];
                        int dc = LookupDC[p];

                        int nr = dr + (int)r;
                        int nc = dc + (int)c;

                        uint dState;
                        if (nr < 0 || nc < 0 || nr >= Rows || nc >= Cols)
                            dState = 0;
                        else
                            dState = curGrid[nr, nc];

                        if (dState == 1)
                            count++;
                    }

                    if (count == 3)
                        nextGrid[r, c] = 1;
                    else if (count != 2)
                        nextGrid[r, c] = 0;
                    else
                        nextGrid[r, c] = curGrid[r, c];
                }
            }
        }
    }
}
