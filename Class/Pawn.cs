

namespace NEWREVITADDIN
{
    public class Pawn : PieceClass
    {

        public Pawn(string _color, int _row, int _col,  StartGame _startGame) : base(_color, _row, _col,  _startGame)
        {
            rank = 5;
            Type = "Pawn";
            InitPiece();
        }
        public override void HighlightMoveOptions()
        {
            if (Color == "Black")
            {
                HighlightForward(1);
                if (Row == 2)
                {
                    HighlightForward(2);
                }

                CheckForDiagonalClash(Row + 1, Col + 1);
                CheckForDiagonalClash(Row + 1, Col - 1);

            }
            else
            {
                HighlightForward(-1);
                if (Row == 7)
                {
                    HighlightForward(-2);
                }

                CheckForDiagonalClash(Row - 1, Col + 1);
                CheckForDiagonalClash(Row - 1, Col - 1);

            }


            void CheckForDiagonalClash(int TileRow, int TileCol)
            {
                bool clash = false;
                string opponentColor = string.Empty;
                foreach (PieceClass piece in startGame.PieceList)
                {
                    if (piece.Row == TileRow && piece.Col == TileCol)
                    {
                        clash = true;
                        opponentColor = piece.Color;
                    }
                }

                if (opponentColor != this.Color && clash)
                {
                    InitRectangle(TileRow, TileCol, true);
                }
            }



        }
    }
}
