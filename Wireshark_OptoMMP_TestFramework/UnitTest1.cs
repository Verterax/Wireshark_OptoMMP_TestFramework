using NUnit.Framework;
using System.Diagnostics;
using Opto22;
using System.Threading;
using System.IO;
using System.Timers;
using System;
using System.Text.RegularExpressions;

namespace Wireshark_OptoMMP_TestFramework
{
    public class MainTestBlock
    {

        #region CONSTS and Variables

        public const string myIp = "10.192.26.85";
        public const string epicIp = "10.192.25.228";
        public const string TSHARK_PATH = @"C:\Development\wsbuild64\run\RelWithDebInfo\tshark.exe";
        public const string TSHARK_SCRIPT_FILE = "tsharkTest.bat";
        public const string TRACE_FILENAME = "output.txt";
        public const string INTERFACE_NAME = "Ethernet"; // This is the name of the interface as seen in Wireshark on startup.
        public const double RECORD_FOR_N_SECONDS = 300.0;
        public const int SHORT_DELAY = 100;
        public const int MEDIUM_DELAY = 500;
        public const int LONG_DELAY = 1000;

        public static string traceOutputPath = "";

        /// <summary>
        /// The handle for the commandline interface to tshark.
        /// </summary>
        public static Process cmd;

        /// <summary>
        /// For keeping track of how long we've been recording packets. 
        /// Ticks every second, calling the Timer_Elapsed method.
        /// </summary>
        public static System.Timers.Timer timer = new System.Timers.Timer(1000); 
        public static DateTime startedAt;


        /// <summary>
        /// Our handle for talking to the controller.
        /// </summary>
        public static mmpController controller;

        public static int timesRun = 0;
        public static bool isTraceCompleted = false;

        #endregion

        #region Init / Load
        /// <summary>
        /// Initialize the entire test strategy. This method runs once at testing start. 
        /// (Not each time before a test. Just once at the start of testing.)
        /// 
        /// 0. Tells tshark to start recording the packet trace.
        /// 
        /// 1. Sends all optommp commands to the controller.
        /// 
        /// 1.5 Expects all command responses to be recieved as well.
        /// 
        /// 2. tshark saves the packet trace log to a file in JSON format.
        /// 
        /// 3. Loads the packet trace log into an object the rest of the tests can access.
        /// 
        /// </summary>
        /// <param name="context"></param>
        [OneTimeSetUp]
        public static void GloablInit()
        {
            if (timesRun > 0)
                Assert.Fail("This method should only be run once during testing.");
            timesRun++;

            // Delete the old capture file.
            if (File.Exists(TRACE_FILENAME))
                File.Delete(TRACE_FILENAME);

            // Init timer. Register the Timer_Elapsed callback.
            timer.Elapsed += Timer_Elapsed;

            // Init the controller.
            // Must init controller before beginning packet trace, otherwise start-up packets
            // will be captured, which is probably not what we want.
            InitController();

            // Initalization code goes here
            BeginPacketTrace();

            // Init Controller Send all OptoMMP commands to the controller for capture.
            SendAllCommands();

            // End the trace and write to file.
            //  ----> Check for this functionality in the Timer_Elapsed event.
            while(!isTraceCompleted) Thread.Sleep(SHORT_DELAY); // Wait for trace to conclude.

            // See the trace log.
            Process.Start(TRACE_FILENAME);
        }

        /// <summary>
        /// Starts tshark and begins recording packets send on the local ethernet connection.
        /// </summary>
        public static void BeginPacketTrace()
        {
            // Start the tracer and wait for it to start up.
            cmd = Process.Start(@"C:\Users\ccaldwell\source\repos\Wireshark_OptoMMP_TestFramework\Wireshark_OptoMMP_TestFramework\bin\Debug\net45\" + TSHARK_SCRIPT_FILE);
            Thread.Sleep(2000);
            timer.Start();
        }

        /// <summary>
        /// Initialize the groov EPIC controller interface with the IP of this machine, and
        /// the IP address of the EPIC controller.
        /// </summary>
        public static void InitController()
        {
            // Init the Controller.
            controller = new mmpController(myIp, epicIp);
        }

        /// <summary>
        /// Sends all the commands that we want to capture and validate outputs for.
        /// </summary>
        public static void SendAllCommands()
        {
            controller.Toggle();
            Thread.Sleep(MEDIUM_DELAY);
            controller.Toggle();
            Thread.Sleep(MEDIUM_DELAY);
            controller.Toggle();
            Thread.Sleep(MEDIUM_DELAY);
            controller.Toggle();
            Thread.Sleep(MEDIUM_DELAY);
            controller.Toggle();
            Thread.Sleep(MEDIUM_DELAY);
            controller.Toggle();
            Thread.Sleep(MEDIUM_DELAY);
        }

        #endregion

        #region Test Methods

        [Test]
        public void Packet_Log_Successfully_Written()
        {
            // The trace file should exist.
            Assert.IsTrue(File.Exists(TRACE_FILENAME));

            // It should be new.
            DateTime lastWrite = File.GetLastWriteTime(TRACE_FILENAME);
            double timeDiff = DateTime.Now.Subtract(lastWrite).TotalSeconds;
            Assert.IsTrue(timeDiff < 15, "The packet capture file is stale at " + timeDiff + " seconds old. The capture failed.");
        }


        #endregion

        #region Helper Methods

        #region Events

        /// <summary>
        /// An event that is fired every second to check if the trace has ended and we can view the log now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Get how long it's been since the packet trace/timer started running.
            double secondsElapsed = e.SignalTime.Subtract(startedAt).TotalSeconds;

            // If it's more than 1 second past when we would expect the trace to be done, signal trace completed.
            if (secondsElapsed > RECORD_FOR_N_SECONDS + 1.0)
                isTraceCompleted = true;
        }

        #endregion

        #endregion

    }
}