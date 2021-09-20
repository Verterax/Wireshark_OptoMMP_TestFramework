using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Opto22.OptoMMP4;
using Opto22.StreamsHelper;

namespace Wireshark_OptoMMP_TestFramework
{
    public class mmpController
    {

        // On the groo epic learning center, I want to turn on the green LED
        // The LED is connected to module 1, channel 8.
        private int DEFAULT_MODULE = 1;
        private int DEFAULT_CHANNEL = 8;
        private int defaultPort = 2001;

        private string myIp;
        private string epicIp;

        private OptoMMP mmp = new OptoMMP();
        

        public mmpController(string myIp, string epicIp)
        {
            this.myIp = myIp;
            this.epicIp = epicIp;

            Int32 i32Result = mmp.Open(epicIp, defaultPort, OptoMMP.Connection.Tcp, 1000, true);
            if (i32Result != 0)
            {
                throw new Exception("Unable to open connection.");
            }
        }

        public bool Toggle()
        {
            bool currentState = readDigitalPoint(DEFAULT_MODULE, DEFAULT_CHANNEL);
            bool newState = writeDigitalPoint(DEFAULT_MODULE, DEFAULT_CHANNEL, !currentState);

            return newState;
        }

        public bool readDigitalPoint(int moduleN, int channelN)
        {
            OptoMMP.EpicDigitalChannel oReadState;
            int result = mmp.EpicReadDigitalChannel(moduleN, channelN, out oReadState);

            if (result != 0)
                throw new Exception("Unable to read channel");

            return oReadState.bState;
        }

        public bool writeDigitalPoint(int moduleN, int channelN, bool booleanState)
        {
            int successCode = mmp.EpicWriteDigitalState(moduleN, channelN, booleanState);

            if (successCode != 0)
                throw new Exception("Unable to read channel");

            return booleanState;
        }

    }
}
