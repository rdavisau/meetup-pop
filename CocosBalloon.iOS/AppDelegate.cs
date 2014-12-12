using System;
using System.Collections.Generic;
using System.Linq;
using CocosBalloon;
using CocosSharp;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CocosBalloon.iOS
{

    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        public override void FinishedLaunching(UIApplication iosapp)
        {
            var app = new CCApplication
            {
                ApplicationDelegate = new CocosBalloonAppDelegate()
            };

            app.StartGame();
        }
    }
}