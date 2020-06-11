using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NEWREVITADDIN
{
    public class PieceClass
    {

        public Element element { get; set; }

        public int Col { get; set; }
        public int Row { get; set; }
        public string Color { get; set; }
        public string Type { get; set; }
        public int rank { get; set; }

        public Document doc { get; set; }

        public StartGame startGame { get; set; }


        public PieceClass(string _color, int _row, int _col, StartGame _startGame)
        {
            startGame = _startGame;
            Color = _color;
            Row = _row;
            Col = _col;
            doc = _startGame.doc;
        }



        public void InitPiece()
        {
            XYZ xyz = new XYZ();
            foreach (FamilySymbol ele in startGame.families)
            {
                if (ele.Family.Name == Type)
                {
                    if (ele.Name == Color)
                    {
                        xyz = new XYZ(Col, Row, 0);
                        this.element = doc.Create.NewFamilyInstance(xyz, ele, StructuralType.NonStructural);
                       
                            
                    }
                }
            }
        }

        public bool CheckForClashandHighlight(int TileRow, int TileCol)
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

            if (!clash)
                InitRectangle(TileRow, TileCol, false);

            else if (opponentColor != this.Color)
            {
                if (this is Pawn)
                {
                }
                else
                    InitRectangle(TileRow, TileCol, true);
            }
            return clash;
        }

        public void InitRectangle(int recRow, int recCol, bool clash)
        {
            XYZ xyz = new XYZ(recCol, recRow, 0.01);
            TileClass t = new TileClass();
            Element e;

            if (clash)
            {
                e = doc.Create.NewFamilyInstance(xyz, startGame.redSymbol, StructuralType.NonStructural);
                t.color = "Red";
            }
            else
            {
                e = doc.Create.NewFamilyInstance(xyz, startGame.greenSymbol, StructuralType.NonStructural);
                t.color = "Green";
            }

            t.column = recCol;
            t.row = recRow;
            t.element = e;
            t.parentElement = this.element;
            startGame.HighlightedTiles.Add(t);

        }

        public virtual void HighlightMoveOptions()
        {

        }

        public void HighlightForward(int i)
        {
            int recRow = Row + i;

            if (recRow < 9 && recRow > 0)
            {
                CheckForClashandHighlight(recRow, Col);
            }
        }

        public void HighlightForward()
        {
            for (int i = 1; i < 8; i++)
            {
                int recRow = Row + i;

                if (recRow < 9 && recRow > 0)
                {
                    if (CheckForClashandHighlight(recRow, Col))
                        break;
                }
            }
            for (int i = 1; i < 8; i++)
            {
                int recRow = Row - i;

                if (recRow < 9 && recRow > 0)
                {
                    if (CheckForClashandHighlight(recRow, Col))
                        break;

                }
            }
        }

        public void HighlightSide(int i)
        {
            int recCol = Col + i;

            if (recCol < 9 && recCol > 0)
            {
                CheckForClashandHighlight(Row, recCol);
            }
        }


        public void HighlightSide()
        {
            for (int i = 1; i < 8; i++)
            {
                int recCol = Col + i;

                if (recCol < 9 && recCol > 0)
                {
                    if (CheckForClashandHighlight(Row, recCol))
                        break;

                }
            }
            for (int i = 1; i < 8; i++)
            {
                int recCol = Col - i;

                if (recCol < 9 && recCol > 0)
                {
                    if (CheckForClashandHighlight(Row, recCol))
                        break;
                }
            }
        }



        public void HighlightDiagonal()
        {
            for (int i = 1; i < 8; i++)
            {
                if (Row + i == 9 || Col + i == 9)
                    break;

                if (CheckForClashandHighlight(Row + i, Col + i))
                    break;
            }

            ////

            for (int i = 1; i < 8; i++)
            {
                if (Row - i == 0 || Col + i == 9)
                    break;

                if (CheckForClashandHighlight(Row - i, Col + i))
                    break;
            }

            ///

            for (int i = 1; i < 8; i++)
            {
                if (Row + i == 9 || Col - i == 0)
                    break;

                if (CheckForClashandHighlight(Row + i, Col - i))
                    break;
            }

            ///
            for (int i = 1; i < 8; i++)
            {
                if (Row - i == 0 || Col - i == 0)
                    break; ;


                if (CheckForClashandHighlight(Row - i, Col - i))
                    break;
            }
        }


    }
}
