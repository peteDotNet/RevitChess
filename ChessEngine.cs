using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWREVITADDIN
{
    struct Position
    {
        int Column { get; set; }
        int Row { get; set; }
    }


    class ChessEngine
    {
        public static Process Start()
        {
            string chessEngineFilePath = @"C:\Users\peter.morton\source\repos\ConsoleApp2\bin\Debug\ConsoleApp2.exe";
            Process process = new Process();
            process.StartInfo.FileName = chessEngineFilePath;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            return process;
            // process.StandardInput.WriteLine("uci");
        }


        public static PieceClass CPUMove(string HumanPlayerMove, Process cheessEngine, List<PieceClass> pieceList)
        {
            string filename = @"C:\Users\peter.morton\Documents\Chess\NextMove.txt";
            string previousCPUMove = System.IO.File.ReadAllLines(filename).Last();

            string nextCPUMMove = previousCPUMove;
            cheessEngine.StandardInput.WriteLine(HumanPlayerMove);
            System.Threading.Thread.Sleep(4050);

            while (previousCPUMove == nextCPUMMove)
            {
                nextCPUMMove = System.IO.File.ReadAllLines(filename).Last();
            }

            var initialColumn = nextCPUMMove[0];
            var endColumn = nextCPUMMove[2];
            int initialColumnInt = charToInt(initialColumn);
            int endColumnInt = charToInt(endColumn);
            int initialRow = charToStr(nextCPUMMove[1]);
            int endRow = charToStr(nextCPUMMove[3]);


            foreach (PieceClass piece in pieceList)
            {
                if (piece.Row == initialRow && piece.Col == initialColumnInt)
                {
                    piece.Row = endRow;
                    piece.Col = endColumnInt;
                    return piece;
                }
            }
            return null;
        }

        private static int charToInt(char column)
        {
            if (column == 'a')
                return 1;
            else if (column == 'b')
                return 2;
            else if (column == 'c')
                return 3;
            else if (column == 'd')
                return 4;
            else if (column == 'e')
                return 5;
            else if (column == 'f')
                return 6;
            else if (column == 'g')
                return 7;
            else if (column == 'h')
                return 8;
            else
                return 0;
        }

        private static int charToStr(char column)
        {
            if (column == '1')
                return 1;
            else if (column == '2')
                return 2;
            else if (column == '3')
                return 3;
            else if (column == '4')
                return 4;
            else if (column == '5')
                return 5;
            else if (column == '6')
                return 6;
            else if (column == '7')
                return 7;
            else if (column == '8')
                return 8;
            else
                return 0;
        }

        public static string generateMoveString(int col1, int col2, int row1, int row2)
        {
            string alphabet = "abcdefgh";
            var col1str = alphabet[col1 - 1];
            var col2str = alphabet[col2 - 1];
            return col1str + row1.ToString() + col2str + row2.ToString();
        }


    }
}
