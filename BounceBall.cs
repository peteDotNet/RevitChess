using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWREVITADDIN
{

    [Transaction(TransactionMode.Manual)]

    class BounceBall : IExternalCommand
    {

        public Document doc;

        public Result Execute(  ExternalCommandData commandData,  ref string message,  ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Reference eleId = uiapp.ActiveUIDocument.Selection.PickObject(ObjectType.Element);
            Element ele = doc.GetElement(eleId);

            var xStart = (ele.Location as LocationPoint).Point.X;
            var yStart = (ele.Location as LocationPoint).Point.Y;
            var zStart = (ele.Location as LocationPoint).Point.Z;



            uiapp.Idling += new EventHandler<IdlingEventArgs>(OnIdling);

            return Result.Succeeded;

            void OnIdling(object sender, IdlingEventArgs e)
            {

                using (Transaction t = new Transaction(doc, "Bounce"))
                {
                    t.Start();

                    LocationPoint location = ele.Location as LocationPoint;

                    XYZ point = new XYZ(xStart, yStart, zStart);

                    xStart = xStart + 0.1;
                    location.Point = point;
                    t.Commit();
                }

            }

        }

      
        //public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        //{
        //    doc = commandData.Application.ActiveUIDocument.Document;



        //    for (int i = 0; i < 2; i++)
        //    {
        //     
        //    }

        //    idle


        //    return Result.Succeeded;
        //}
    }
}
