using System;

namespace NEWREVITADDIN
{
    public class King : PieceClass
    {

        public King(string _color, int _row, int _col, StartGame _startGame) : base(_color, _row, _col, _startGame)
        {
            Type = "King";
            InitPiece();
        }

        public override void HighlightMoveOptions()
        {
                HighlightSide(1);
                HighlightSide(-1);
                HighlightForward(-1);
                HighlightForward(1);

            if(Row < 8 && Col < 8)
                CheckForClashandHighlight(Row + 1, Col + 1);

            if (Row > 1 && Col < 8)
                CheckForClashandHighlight(Row - 1, Col + 1);

            if (Row < 8 && Col > 1)
                CheckForClashandHighlight(Row + 1, Col - 1);

            if (Row > 1 && Col > 1)
                CheckForClashandHighlight(Row - 1, Col - 1);

        }


    }
}
