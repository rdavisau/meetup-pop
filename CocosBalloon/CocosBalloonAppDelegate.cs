using CocosSharp;

namespace CocosBalloon
{
    public class CocosBalloonAppDelegate : CCApplicationDelegate
    {
        public override void ApplicationDidFinishLaunching(CCApplication app, CCWindow mainWindow)
        {
            app.ContentRootDirectory = "Content";
            app.ContentSearchPaths.Add("sounds");

            mainWindow.RunWithScene(IntroLayer.Scene(mainWindow));
        }
    }
}