using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Foundation;
using UIKit;
using ReactiveUI;
using Splat;

namespace RxUIReactiveTableViewSourceRepro
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;

        //
        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Logger.Initialize();
            Locator.CurrentMutable.Register(() => new Logger(), typeof(ILogger));

            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            // If you have defined a root view controller, set it here:
            // window.RootViewController = myViewController;

            window.RootViewController = new UINavigationController(new ScenariosView());
            //window.RootViewController = new UINavigationController(new AltView());
            // make the window visible
            window.MakeKeyAndVisible();
            
            return true;
        }

    }

    public class Logger : ILogger
    {
        private static readonly string path = Path.Combine(Environment.CurrentDirectory, "LogTastic.txt");
        private static StreamWriter writer;
        private readonly object sync;
        private LogLevel level;

        public Logger()
        {
            this.sync = new object();
        }

        public static void Initialize()
        {
            // if running in simulator, you can use:
            //    tail -F "path" 
            // to monitor the log
            System.Diagnostics.Debug.WriteLine("LOGTASTIC:");
            System.Diagnostics.Debug.WriteLine("tail -F " + path);
            EnsureWriter(true);
        }

        public static void Clear()
        {
            EnsureWriter(true);
        }

        public LogLevel Level
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
            }
        }

        public void Write([Localizable(false)]string message, LogLevel logLevel)
        {
            lock (sync)
            {
                writer.Write("#");
                writer.Write(Environment.CurrentManagedThreadId);
                writer.Write(" [");
                writer.Write(logLevel);
                writer.Write("]");
                writer.WriteLine(message);
                writer.Flush();
            }
        }

        private static void EnsureWriter(bool force = false)
        {
            if (writer == null || force)
            {
                if (writer != null)
                {
                    writer.Dispose();
                }

                writer = new StreamWriter(path, false);
            }
        }
    }
}

