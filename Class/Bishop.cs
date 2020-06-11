namespace NEWREVITADDIN
{
    public class Bishop : PieceClass
    {
        
        public Bishop ( string _color, int _row, int _col,  StartGame _startGame) : base(  _color,  _row,  _col,   _startGame)
        {
            Type = "Bishop";
            rank = 4;
            InitPiece();
        }

        public override void HighlightMoveOptions()
        {
            HighlightDiagonal();
        }

    }
}
