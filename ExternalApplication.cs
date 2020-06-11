using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NEWREVITADDIN
{
    public class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Chess Game";
            string path = Assembly.GetExecutingAssembly().Location;
            application.CreateRibbonTab(tabName);
            RibbonPanel panel = application.CreateRibbonPanel(tabName, tabName);
            PushButtonData button = new PushButtonData("button1", "Start Match", path, "NEWREVITADDIN.StartGame");
            PushButtonData button1 = new PushButtonData("button2", "Ball Bounce", path, "NEWREVITADDIN.BounceBall");
            BitmapImage image = new BitmapImage(new Uri("pack://application:,,,/NewRibbon;component/favicon.ico"));

            button.LargeImage = image;

            panel.AddItem(button);
            panel.AddItem(button1);
            return Result.Succeeded;
        }
    }
}
