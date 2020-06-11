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
        public bool vsCPU = true;

        public List<PieceClass> PieceList = new List<PieceClass>();
        public List<TileClass> Tiles = new List<TileClass>();
        public List<TileClass> HighlightedTiles = new List<TileClass>();
        public IList<Element> families = new List<Element>();
        public Document doc;
        public ModelText modelText;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            WelcomeForm welcomeForm = new WelcomeForm();
            welcomeForm.ShowDialog();
            vsCPU = welcomeForm.vsCPU;

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            PieceClass pieceToRemove = null;
            doc = uidoc.Document;


            string filename = @"C:\Users\peter.morton\Documents\Chess\NextMove.txt";
            Process process = new Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.StandardInput.WriteLine("uci");
            


            ElementClassFilter FamilyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));
            FilteredElementCollector FamilyInstanceCollector = new FilteredElementCollector(doc);
            IList<Element> allFamilies = FamilyInstanceCollector.WherePasses(FamilyInstanceFilter).ToElements();

            ElementClassFilter TextInstanceFilter = new ElementClassFilter(typeof(ModelText));
            modelText = new FilteredElementCollector(doc).WherePasses(TextInstanceFilter).ToElements().First() as ModelText;

            ElementClassFilter FamilyFilter = new ElementClassFilter(typeof(FamilySymbol));
            families = new FilteredElementCollector(doc).WherePasses(FamilyFilter).ToElements();


            clearBoard();

            using (Transaction t = new Transaction(doc, "Set Board"))
            {
                t.Start();
                InitPieces();
                InitBoard();
                t.Commit();
            }





            for (int i = 0; i < 1;)
            {
                pieceToRemove = null;

                isKingInCheck();


                using (Transaction t = new Transaction(doc, "Test"))
                {
                    t.Start();


                    Reference eleid = null;
                    Element ele;
                    try
                    {
                        if (turn == "White")
                        {
                            eleid = uidoc.Selection.PickObject(ObjectType.Element, new WhiteSelectionFilter());
                            ele = doc.GetElement(eleid);
                        }
                        else if (vsCPU)
                            ele = CPUMove();
                        else
                        {
                            eleid = uidoc.Selection.PickObject(ObjectType.Element, new BlackSelectionFilter());
                            ele = doc.GetElement(eleid);
                        }
                    }
                    catch { break; }


                    if ((ele as FamilyInstance).Symbol.Family.Name != "ChessTile")
                    {
                        deleteHighlightedTiles();
                        GetPieceClass(ele).HighlightMoveOptions();
                    }
                    else
                    {
                        movePiece(ele);
                        deleteHighlightedTiles();
                    }

                    t.Commit();
                    if (pieceToRemove != null)
                        PieceList.Remove(pieceToRemove);
                }
            }

            void movePiece(Element ele)
            {
                foreach (TileClass tile in HighlightedTiles)
                {
                    if (tile.element.Id == ele.Id)
                    {
                        LocationPoint tileLoaction = tile.element.Location as LocationPoint;
                        LocationPoint parentLoaction = tile.parentElement.Location as LocationPoint;
                        parentLoaction.Point = tileLoaction.Point;

                        //Change turn
                        if (turn == "White")
                            turn = "Black";
                        else
                            turn = "White";

                        modelText.Text = turn + " To Move";

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
                                piece.Col = tile.column;
                                piece.Row = tile.row;
                                if (checkPawnBecomesQueen(piece))
                                    pawn = piece;
                            }
                        }

                        if (pawn != null)
                        {
                            doc.Delete(pawn.element.Id);
                            PieceList.Remove(pawn);
                            PieceList.Add(new Queen(pawn.Color, pawn.Row, pawn.Col, this));
                        }
                    }
                }

            }

            void deleteHighlightedTiles()
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

            void clearBoard()
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

            void InitPieces()
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


            void InitBoard()
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

            Element CPUMove()
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


            void isKingInCheck()
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

            return Result.Succeeded;

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

