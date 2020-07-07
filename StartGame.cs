using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWREVITADDIN
{
    [Transaction(TransactionMode.Manual)]
    public class StartGame : IExternalCommand
    {
        public FamilySymbol whiteSymbol { get; set; }
        public FamilySymbol blackSymbol { get; set; }
        public FamilySymbol greenSymbol { get; set; }
        public FamilySymbol redSymbol { get; set; }

        public string turn = "White";
        public bool playVsCPU = true;
        string WhiteMoveForEngine;
        public Process chessEngineProcess;

        public List<PieceClass> PieceList = new List<PieceClass>();
        public List<TileClass> Tiles = new List<TileClass>();
        public List<TileClass> HighlightedTiles = new List<TileClass>();
        public IList<Element> families = new List<Element>();
        IList<Element> allFamilies;
        public Document doc;
        public ModelText modelText;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            WelcomeForm welcomeForm = new WelcomeForm();
            welcomeForm.ShowDialog();
            playVsCPU = welcomeForm.vsCPU;

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            PieceClass pieceToRemove = null;
            doc = uidoc.Document;

            findExistingPieces();
            clearBoard(allFamilies);

            if (playVsCPU)
            {
                chessEngineProcess = ChessEngine.Start();
            }

            using (Transaction t = new Transaction(doc, "Set Board"))
            {
                t.Start();
                InitPieces();
                InitBoard();
                t.Commit();
            }

            while (true)
            {
                pieceToRemove = null;
                //isKingInCheck();

                using (Transaction t = new Transaction(doc, "Test"))
                {
                    t.Start();

                    Reference eleid = null;
                    Element ele = null;


                    //CPU Turn
                    if (playVsCPU && turn == "Black")
                    {
                        PieceClass piece =  ChessEngine.CPUMove(WhiteMoveForEngine, chessEngineProcess, PieceList);
                        pieceToRemove = moveCPUPiece(piece, pieceToRemove);
                    }
                   

                    else
                    {
                        //Get player to select an element
                        bool selectionFailed = selectPiece(uidoc, ref eleid, ref ele);

                        if (selectionFailed)
                            break;

                        //If chosen element is a chess piece then highlight its available moves
                        if ((ele as FamilyInstance).Symbol.Family.Name != "ChessTile")
                        {
                            deleteHighlightedTiles();
                            GetPieceClass(ele).HighlightMoveOptions();
                        }
                        //Otherwise element is a tile - therfore move chess piece to the location of the tile
                        else
                        {
                            pieceToRemove = movePiece(ele, pieceToRemove);
                            deleteHighlightedTiles();
                        }
                    }
                    t.Commit();
                    if (pieceToRemove != null)
                        PieceList.Remove(pieceToRemove);
                }
            }
            return Result.Succeeded;
        }

        private bool selectPiece(UIDocument uidoc, ref Reference eleid, ref Element ele)
        {
            try
            {
                if (turn == "White")
                {
                    eleid = uidoc.Selection.PickObject(ObjectType.Element, new WhiteSelectionFilter());
                    ele = doc.GetElement(eleid);
                }
                else
                {
                    eleid = uidoc.Selection.PickObject(ObjectType.Element, new BlackSelectionFilter());
                    ele = doc.GetElement(eleid);
                }

            }
            catch { return true; }
            return false;
        }

        private void findExistingPieces()
        {
            ElementClassFilter FamilyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));
            FilteredElementCollector FamilyInstanceCollector = new FilteredElementCollector(doc);
            allFamilies = FamilyInstanceCollector.WherePasses(FamilyInstanceFilter).ToElements();

            ElementClassFilter TextInstanceFilter = new ElementClassFilter(typeof(ModelText));
            modelText = new FilteredElementCollector(doc).WherePasses(TextInstanceFilter).ToElements().First() as ModelText;

            ElementClassFilter FamilyFilter = new ElementClassFilter(typeof(FamilySymbol));
            families = new FilteredElementCollector(doc).WherePasses(FamilyFilter).ToElements();
        }

        private PieceClass movePiece(Element ele, PieceClass pieceToRemove)
        {
            foreach (TileClass tile in HighlightedTiles)
            {
                if (tile.element.Id == ele.Id)
                {
                    LocationPoint tileLoaction = tile.element.Location as LocationPoint;
                    LocationPoint parentLoaction = tile.parentElement.Location as LocationPoint;
                    parentLoaction.Point = tileLoaction.Point;
                    PieceClass pawn = null;

                    foreach (PieceClass piece in PieceList)
                    {
                        if (piece.Col == tile.column && piece.Row == tile.row)
                        {
                            doc.Delete(piece.element.Id);
                            pieceToRemove = piece;
                           
                        }

                        else if (piece.element.Id == tile.parentElement.Id)
                        {
                            if (turn == "White" && playVsCPU)
                                WhiteMoveForEngine = ChessEngine.generateMoveString(piece.Col, tile.column, piece.Row, tile.row);

                            piece.Col = tile.column;
                            piece.Row = tile.row;
                            if (checkPawnBecomesQueen(piece))
                                pawn = piece;
                        }
                    }

                    //Change turn
                    if (turn == "White")
                        turn = "Black";
                    else
                        turn = "White";

                    modelText.Text = turn + " To Move";

                    if (pawn != null)
                    {
                        doc.Delete(pawn.element.Id);
                        PieceList.Remove(pawn);
                        PieceList.Add(new Queen(pawn.Color, pawn.Row, pawn.Col, this));
                    }
                }
            }

            return pieceToRemove;

        }
        private PieceClass moveCPUPiece(PieceClass piece, PieceClass pieceToRemove)
        {
            LocationPoint pieceOriginalLocation = piece.element.Location as LocationPoint;
            var updatedPoint = new XYZ(piece.Col, piece.Row, 0);
            pieceOriginalLocation.Point = updatedPoint;

            PieceClass pawn = null;

            foreach (PieceClass p in PieceList)
            {
                if (p.Col == piece.Col && p.Row == piece.Col)
                {
                    doc.Delete(p.element.Id);
                    pieceToRemove = p;
                }
            }

            if (checkPawnBecomesQueen(piece))
                pawn = piece;

            //Change turn
            if (turn == "White")
                turn = "Black";
            else
                turn = "White";

            modelText.Text = turn + " To Move";

            if (pawn != null)
            {
                doc.Delete(pawn.element.Id);
                PieceList.Remove(pawn);
                PieceList.Add(new Queen(pawn.Color, pawn.Row, pawn.Col, this));
            }

            return pieceToRemove;
        }

        private void isKingInCheck()
        {
            using (Transaction t = new Transaction(doc, "CheckCheck"))
            {
                t.Start();

                List<PieceClass> allEnemyPieces = null;

                if (turn != "White")
                {
                    allEnemyPieces = PieceList.FindAll(x => x.Color == "White");
                    foreach (PieceClass piece in allEnemyPieces)
                    {
                        piece.HighlightMoveOptions();
                    }
                }
                else
                {
                    allEnemyPieces = PieceList.FindAll(x => x.Color == "Black");
                    foreach (PieceClass piece in allEnemyPieces)
                    {
                        piece.HighlightMoveOptions();
                    }
                }

                PieceClass king = null;
                foreach (PieceClass _king in PieceList)
                {
                    if (_king.Color == turn)
                        king = _king;
                }



                foreach (TileClass tile in HighlightedTiles)
                {
                    if (tile.column == king.Col && tile.row == king.Row)
                        TaskDialog.Show("Check", turn + " King is in check.");
                }

                List<TileClass> tilesToRemove = new List<TileClass>();

                foreach (TileClass tile in HighlightedTiles)
                {
                    string color = GetPieceClass(tile.parentElement).Color;
                    if (color != turn)
                    {
                        doc.Delete(tile.element.Id);
                        tilesToRemove.Add(tile);
                    }
                }
                foreach (TileClass tile in tilesToRemove)
                {
                    HighlightedTiles.Remove(tile);
                }
                t.Commit();
            }
        }

        public void deleteHighlightedTiles()
        {
            if (HighlightedTiles.Count > 0)
            {
                foreach (TileClass fam in HighlightedTiles)
                {
                    if (fam.element is FamilyInstance)
                    {
                        doc.Delete(fam.element.Id);
                    }
                }
                HighlightedTiles.Clear();
            }
        }

        private Element CPURandomMove()
        {
            if (HighlightedTiles.Count > 0)
            {
                foreach (TileClass tile in HighlightedTiles)
                {
                    if (tile.color == "Red")
                        return tile.element;
                }

                return HighlightedTiles.First().element;
            }

            else
            {

                List<PieceClass> allBlackPieces = PieceList.FindAll(x => x.Color == "Black");
                foreach (PieceClass piece in allBlackPieces)
                {
                    piece.HighlightMoveOptions();
                }

                foreach (TileClass tile in HighlightedTiles)
                {
                    if (tile.color == "Red")
                        return tile.parentElement;
                }

                deleteHighlightedTiles();

                int rnd = new Random().Next(1, allBlackPieces.Count());
                return allBlackPieces[rnd].element;
            }
        }

        private void InitBoard()
        {
            whiteSymbol = families.First() as FamilySymbol;
            blackSymbol = families.First() as FamilySymbol;

            foreach (FamilySymbol ele in families)
            {
                if (ele.Family.Name == "ChessTile")
                {
                    if (ele.Name == "White Tile")
                        whiteSymbol = ele;
                    else if (ele.Name == "Black Tile")
                        blackSymbol = ele;
                    else if (ele.Name == "Green Tile")
                        greenSymbol = ele;
                    else if (ele.Name == "Red Tile")
                        redSymbol = ele;
                }
            }


            bool stagger = true;
            for (int ii = 1; ii < 9; ii++)
            {
                for (int i = 1; i < 9; i++)
                {
                    TileClass tc = new TileClass();
                    tc.row = ii;
                    tc.column = i;

                    XYZ xyz = new XYZ(i, ii, 0);
                    if (stagger)
                    {
                        tc.color = "Black";
                        tc.element = doc.Create.NewFamilyInstance(xyz, blackSymbol, StructuralType.NonStructural);
                    }
                    else
                    {
                        tc.color = "White";
                        tc.element = doc.Create.NewFamilyInstance(xyz, whiteSymbol, StructuralType.NonStructural);
                    }
                    Tiles.Add(tc);
                    stagger = !stagger;
                }
                stagger = !stagger;
            }
        }

        private void clearBoard(IList<Element> allFamilies)
        {
            if (allFamilies.Count > 0)
            {
                using (Transaction t = new Transaction(doc, "Clear Board"))
                {
                    t.Start();
                    foreach (Element ele in allFamilies)
                    {
                        doc.Delete(ele.Id);
                    }
                    t.Commit();
                }
            }
        }

        private void InitPieces()
        {
            for (int i = 1; i < 9; i++)
            {
                PieceList.Add(new Pawn("Black", 2, i, this));
                PieceList.Add(new Pawn("White", 7, i, this));
            }

            int row = 1;
            string color = "Black";
            for (int i = 0; i < 2; i++)
            {
                PieceList.Add(new Rook(color, row, 1, this));
                PieceList.Add(new Knight(color, row, 2, this));
                PieceList.Add(new Bishop(color, row, 3, this));
                PieceList.Add(new King(color, row, 4, this));
                PieceList.Add(new Queen(color, row, 5, this));
                PieceList.Add(new Bishop(color, row, 6, this));
                PieceList.Add(new Knight(color, row, 7, this));
                PieceList.Add(new Rook(color, row, 8, this));
                row = 8;
                color = "White";
            }
        }

        bool checkPawnBecomesQueen(PieceClass piece)
        {
            if (piece is Pawn)
            {
                if (piece.Color == "White" && piece.Row == 1)
                    return true;
                else if (piece.Color == "Black" && piece.Row == 8)
                    return true;
            }

            return false;

        }

        public class WhiteSelectionFilter : ISelectionFilter
        {

            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                if (!(elem is FamilyInstance)) return false;

                BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

                if (builtInCategory == BuiltInCategory.OST_GenericModel)
                {

                    if ((elem as FamilyInstance).Symbol.Name == "White")
                    {
                        return true;
                    }
                    else if ((elem as FamilyInstance).Symbol.Name == "Green Tile")
                        return true;
                    else if ((elem as FamilyInstance).Symbol.Name == "Red Tile")
                        return true;
                }

                return false;

                int GetCategoryIdAsInteger(Element element)
                {
                    return element?.Category?.Id?.IntegerValue ?? -1;
                }
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return true;
            }
        }

        public class BlackSelectionFilter : ISelectionFilter
        {

            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                if (!(elem is FamilyInstance)) return false;

                BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

                if (builtInCategory == BuiltInCategory.OST_GenericModel)
                {

                    if ((elem as FamilyInstance).Symbol.Name == "Black")
                    {
                        return true;
                    }
                    else if ((elem as FamilyInstance).Symbol.Name == "Green Tile")
                        return true;
                    else if ((elem as FamilyInstance).Symbol.Name == "Red Tile")
                        return true;
                }

                return false;

                int GetCategoryIdAsInteger(Element element)
                {
                    return element?.Category?.Id?.IntegerValue ?? -1;
                }
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return true;
            }
        }

        public PieceClass GetPieceClass(Element ele)
        {
            foreach (PieceClass piece in PieceList)
            {
                if (piece.element.Id == ele.Id)
                {
                    return piece;
                }
            }
            return null;
        }
    }
}

