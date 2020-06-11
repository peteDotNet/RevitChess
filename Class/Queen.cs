using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWREVITADDIN
{
    public class Queen : PieceClass
    {

        public Queen(string _color, int _row, int _col,  StartGame _startGame) : base(_color, _row, _col,  _startGame)
        {
            Type = "Queen";
            rank = 1;
            InitPiece();
        }

        public override void HighlightMoveOptions()
        {
            HighlightDiagonal();
            HighlightSide();
            HighlightForward();
        }

    }
}
