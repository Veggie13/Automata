using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automata
{
    class RPSAutomata : Automata
    {
        public RPSAutomata(uint rows, uint cols)
            : base(rows, cols, 31)
        {
        }

        static int[] LookupDR;
        static int[] LookupDC;
        static int[] LookupColor;
        static int[] LookupLevel;
        static uint[,] LookupGlob;

        static RPSAutomata()
        {
            LookupDR = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
            LookupDC = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
            LookupColor = new int[] { 0,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
            LookupLevel = new int[] { 9,
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            LookupGlob = new uint[31, 31];
            for (uint tState = 0; tState < 31; tState++)
                for (uint dState = 0; dState < 31; dState++)
                {
                    int dLevel = LookupLevel[dState];
                    int tLevel = LookupLevel[tState];

                    int dColor = LookupColor[dState];
                    int tColor = LookupColor[tState];

                    int nColor, nLevel;
                    if (dColor == tColor)
                    {
                        nColor = tColor;
                        nLevel = tLevel;
                    }
                    else
                    {
                        if (tColor == 0)
                        {
                            if (dLevel < 9)
                            {
                                nColor = dColor;
                                nLevel = dLevel + 1;
                            }
                            else
                            {
                                nColor = 0;
                                nLevel = 9;
                            }
                        }
                        else
                        {
                            if (dColor == (tColor % 3) + 1)
                            {
                                nColor = tColor;
                                nLevel = tLevel - 1;
                                if (nLevel < 0) nLevel = 0;
                            }
                            else if (dColor != 0)
                            {
                                nLevel = tLevel + 1;
                                nColor = (nLevel > 9) ? dColor : tColor;
                                if (nLevel > 9) nLevel = 5;
                            }
                            else
                            {
                                nLevel = tLevel;
                                nColor = tColor;
                            }
                        }
                    }

                    LookupGlob[tState, dState] = (uint)(10 * nColor + nLevel - 9);
                }
        }

        Random _rand = new Random();
        protected override void advance()
        {
            uint[,] curGrid = CurGrid;
            uint[,] nextGrid = NextGrid;

            for (uint r = 0; r < Rows; r++)
            {
                for (uint c = 0; c < Cols; c++)
                {
                    int partner = _rand.Next(8);
                    int dr = LookupDR[partner];
                    int dc = LookupDC[partner];

                    int nr = dr + (int)r;
                    int nc = dc + (int)c;

                    uint dState;
                    if (nr < 0 || nc < 0 || nr >= Rows || nc >= Cols)
                        dState = 0;
                    else
                        dState = curGrid[nr, nc];

                    nextGrid[r, c] = LookupGlob[curGrid[r, c], dState];
                }
            }
        }
    }
}
