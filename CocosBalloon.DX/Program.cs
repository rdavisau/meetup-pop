using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosBalloon.DX
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CCApplication(false, new CCSize(768f, 1024f).MultiplyBy(.8f))
            {
                ApplicationDelegate = new CocosBalloonAppDelegate()
            };

            app.StartGame();
        }
    }
}
