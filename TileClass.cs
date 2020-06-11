using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace NEWREVITADDIN
{

    public class TileClass
    {
        public string color { get; set; }
        public int row { get; set; }
        public int column { get; set; }

        public Element element { get; set; }

        public Element parentElement { get; set; }

    }
}