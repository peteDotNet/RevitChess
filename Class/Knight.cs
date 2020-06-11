

namespace NEWREVITADDIN
{
    public class Knight : PieceClass
    {

        public Knight ( string _color, int _row, int _col,  StartGame _startGame) : base( _color, _row, _col,  _startGame)
        {
            Type = "Knight";
            rank = 3;
            InitPiece();
        }

        public override void HighlightMoveOptions()
        {

            Move(1, 2);
            Move(2, 1);
            Move(-1, 2);
            Move(-2, 1);
            Move(1, -2);
            Move(2, -1);
            Move(-1, -2);
            Move(-2, -1);


            void Move(int up, int side)
            {
                if (Row + up < 9 && Row + up > 0 && Col + side < 9 && Col + side > 0)
                {

                    CheckForClashandHighlight(Row + up, Col + side);
                }
            }





        }
    }
}
