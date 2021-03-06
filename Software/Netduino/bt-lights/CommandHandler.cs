using System;
using System.Text;
#if TARGET
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
#endif
using System.Threading;
using bt_light_framework;

namespace BTLights
{
    public class CommandHandler
    {
#if TARGET
        public event BTEvent ChannelRequest, GlobalRequest, ExternalRequest;
        public event NativeEventHandler InternalCommandReceived;
        public static uint CommandCounter = 0;

        private Package mPackage;
        private LightStringCollection mChannels;
        private delegate void SendAfterSplit();
#endif
        public enum Command : byte
        {
            SetMode,                // 0x00: Set the mChannelID mode
            GetMode,                // 0x01: Get the mChannelID mode
            SetValue,               // 0x02: Set current value
            GetValue,               // 0x03: Get current value  
            SetFunction,            // 0x04: Set channels function
            GetFunction,            // 0x05: Get channels function
            SetMaximum,             // 0x06: Set the maximum value
            GetMaximum,             // 0x07: Get the maximum value
            SetMinimum,             // 0x08: Set the minimum value
            GetMinimum,             // 0x09: Get the minimum value
            SetDelay,               // 0x0A: Set the timer delay
            GetDelay,               // 0x0B: Get the timer delay
            SetPeriod,              // 0x0C: Set the timer period
            GetPeriod,              // 0x0D: Get the timer period
            SetRise,                // 0x0E: Set rise modifier
            GetRise,                // 0x0F: Get rise modifier
            SetOffset,              // 0x10: Set offset modifier
            GetOffset,              // 0x11: Set offset modifier
            ResetChannel,           // 0x12: Reset the mChannelID timer    
            GetCommandCounter,      // 0x13: Get the command counter
            ResetCommandCounter,    // 0x14: Reset the command counter
            GetErrorLog,            // 0x15: trace out the error log
            ResetSystem,            // 0x16: Do a hardware reset
            GetSystemTime,          // 0x17: Get the time on the board
            GetSystemVersion,       // 0x18: Get the hardware version
            ResetBluetooth,         // 0x19: Reset the BT module only
            Acknowledge,            // 0x1A: Acknowdlege received command
            ChannelTracer,          // 0x1B: Activate or Deactivate channel value tracer
            ResetAllTimer,          // 0x1C: Resets all the channel timers
            NumberOfCommands,       // Number of commands must be end of enum
        }
#if TARGET
        public CommandHandler(LightStringCollection channels)
        {
            mChannels = channels;
        }

        public void SplitCommand(object sender, BTEventArgs e)
        {
            mPackage = e.Data;
            //Thread sc = new Thread(new ThreadStart(SplitCommand));
            //sc.Start();        
            MainProgram.RestartBluetoothTimeout();
            SendAfterSplit SendErrorLogDelegate = null;

            for (int i = 1; i < mPackage.PackageHeader.PackageLength; i++)
            {
                CommandCounter++;
                Frame curFrame = mPackage.PackageFrames[i];
                Frame replyFrame = curFrame;
                uint channel = curFrame.ContentByte[1];
                int value = curFrame.ContentByte[3];
                switch (curFrame.Command)
                {
                    /**
                     * Channel command block
                     **/
                    case (byte)Command.SetValue:
                        mChannels.SetChannelValue(channel, value);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (byte)Command.GetValue:
                        replyFrame.ContentByte[3] = (byte)mChannels.GetChannelValue(channel);
                        break;
                    case (byte)Command.SetMode:
                        mChannels.SetChannelMode(channel, value);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (byte)Command.GetMode:
                        replyFrame.ContentByte[3] = (byte)mChannels.GetChannelMode(channel);
                        break;
                    case (byte)Command.SetFunction:
                        mChannels.SetChannelFunction(channel, value);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (byte)Command.GetFunction:
                        replyFrame.ContentByte[3] = (byte)mChannels.GetChannelFunction(channel);
                        break;
                    case (byte)Command.SetMaximum:
                        this.mChannels.SetChannelLimits(channel, value, false);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (byte)Command.GetMaximum:
                        replyFrame.ContentByte[3] = (byte)mChannels.GetChannelLimits(channel, false);
                        break;
                    case (byte)Command.SetMinimum:
                        this.mChannels.SetChannelLimits(channel, value, true);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (byte)Command.GetMinimum:
                        replyFrame.ContentByte[3] = (byte)mChannels.GetChannelLimits(channel, true);
                        break;
                    case (int)CommandHandler.Command.SetDelay:
                        this.mChannels.SetChannelDelay(channel, value);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (int)CommandHandler.Command.GetDelay:
                        replyFrame.ContentByte[3] = (byte)this.mChannels.GetChannelDelay(channel);
                        break;
                    case (int)CommandHandler.Command.SetPeriod:
                        this.mChannels.SetChannelPeriod(channel, value);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (int)CommandHandler.Command.GetPeriod:
                        replyFrame.ContentByte[3] = (byte)this.mChannels.GetChannelPeriod(channel);
                        break;
                    case (int)CommandHandler.Command.SetRise:
                        this.mChannels.SetChannelCurve(channel, value, (int)Command.SetRise);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (int)CommandHandler.Command.SetOffset:
                        this.mChannels.SetChannelCurve(channel, value, (int)Command.SetOffset);
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (int)CommandHandler.Command.GetRise:
                        replyFrame.ContentByte[3] = (byte)this.mChannels.GetChannelRise(channel);
                        break;
                    case (int)CommandHandler.Command.GetOffset:
                        replyFrame.ContentByte[3] = (byte)this.mChannels.GetChannelOffset(channel);
                        break;
                    case (int)CommandHandler.Command.ResetChannel:
                        // block from other threads
                        lock (new object())
                        {
                            this.mChannels.RestartChannelTimer(channel);
                        }
                        break;

                    /**
                     * General command block
                     **/
                    case (byte)Command.GetCommandCounter:
                        replyFrame.Payload = CommandCounter;
                        break;
                    case (byte)Command.ResetCommandCounter:
                        CommandCounter = 0;
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        break;
                    case (byte)Command.GetErrorLog:
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Payload = curFrame.Command;
                        SendErrorLogDelegate = new SendAfterSplit(MainProgram.SendErrorLog);
                        break;
                    case (byte)Command.ResetSystem:
                        MainProgram.DebugOut(mPackage.GetPackageBytes());
                        Thread.Sleep(1000);
                        MainProgram.ResetSystem();
                        break;
                    case (byte)Command.GetSystemTime:
                        replyFrame.Payload = (uint)MainProgram.SystemClock.TotalTime;
                        break;
                    case (byte)Command.GetSystemVersion:
                        replyFrame.ContentByte[2] = MainProgram.Version;
                        replyFrame.ContentByte[3] = MainProgram.Build;
                        break;
                    case (byte)Command.ResetBluetooth:
                        MainProgram.ResetBluetooth();
                        break;
                    case (byte)Command.ChannelTracer:
                        replyFrame.Command = (byte)Command.Acknowledge;
                        MainProgram.ChannelTracer(replyFrame.Payload);
                        break;
                    case (byte)Command.ResetAllTimer:
                        replyFrame.Command = (byte)Command.Acknowledge;
                        replyFrame.Value = curFrame.Command;
                        this.mChannels.RestartAllTimer();
                        break;
                    default:
                        MainProgram.RegisterError(MainProgram.ErrorCodes.CommandUnknown, "Command unknown");
                        break;
                }
            }
            MainProgram.DebugOut(mPackage.GetPackageBytes());
            if (SendErrorLogDelegate != null)
            {
                SendErrorLogDelegate();
            }
        }
#endif
    }
}
