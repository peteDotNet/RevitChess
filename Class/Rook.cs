using Autodesk.Revit.DB;

namespace NEWREVITADDIN
{
    public class Rook : PieceClass
    {

        public Rook( string _color, int _row, int _col, StartGame _startGame) : base( _color, _row, _col,  _startGame)
        {
            Type = "Rook";
            rank = 2;
            InitPiece();
        }

        public override void HighlightMoveOptions()
        {
          
                HighlightForward();
                HighlightSide();

        }
    }
}
