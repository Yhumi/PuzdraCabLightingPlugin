using Dalamud.Bindings.ImGui;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Lumina.Data;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PuzdraLighting.Helpers
{
    internal unsafe class FastIOLightingHelper : IDisposable
    {
        public State FastIOState = State.Closed;
        public int OpenAttempts = 0;

        private int FileHandle = -1;

        [DllImport("iDmacDrv64")]
        public static extern uint iDmacDrvOpen(int deviceId, int* pOutFileHandle, int* pSomeFlag);

        [DllImport("iDmacDrv64")]
        public static extern uint iDmacDrvClose(int deviceId, int* pSomeFlag);

        [DllImport("iDmacDrv64")]
        public static extern int iDmacDrvRegisterWrite(int deviceId, uint CommandCode, int data, int* pOutDeviceResult);

        internal FastIOLightingHelper()
        {
        }

        public void Dispose()
        {
            int outHandle = -1;
            uint success = iDmacDrvClose(FileHandle, &outHandle);
        }

        public void OpenDriver()
        {
            int fileHandle = -1;
            int someFlags = 0;

            uint success = iDmacDrvOpen(1, &fileHandle, &someFlags);
            Svc.Log.Debug($"iDmacDrvOpen Response Code: {success}");

            if (success == 0) 
            {
                FileHandle = fileHandle;

                FastIOState = State.Open;
                OpenAttempts = 0;

                Svc.Log.Debug($"iDmacDrvOpen Driver open successfully.");
                Svc.Log.Debug($"Initialising default lights");
                
                WriteRGBColourValues(
                    new FastIOColour(0xFF, 0x00, 0xFF),
                    new FastIOColour(0xFF, 0x00, 0xFF),
                    new FastIOColour(0xFF, 0x00, 0xFF)
                );

                return;
            }
  
            OpenAttempts += 1;
            Svc.Log.Debug($"Failed to open, returning attempts: {OpenAttempts}");

            if (OpenAttempts >= 5)
                FastIOState = State.Error;
        }

        public void WriteRGBColourValues(FastIOColour frontLights, FastIOColour leftLights, FastIOColour rightLights)
        {
            int redLighting = HandleBitShifting(frontLights.Red, leftLights.Red, rightLights.Red);
            int greenLighting = HandleBitShifting(frontLights.Green, leftLights.Green, rightLights.Green);
            int blueLighting = HandleBitShifting(frontLights.Blue, leftLights.Blue, rightLights.Blue);

            WriteToRegister((uint) Registers.RedRegisters, redLighting);
            WriteToRegister((uint) Registers.GreenRegisters, greenLighting);
            WriteToRegister((uint) Registers.BlueRegisters, blueLighting);
        }

        public int HandleBitShifting(byte front, byte left, byte right)
        {
            int colorValue = front;
            colorValue = (colorValue << 8) + left;
            colorValue = (colorValue << 8) + right;

            return colorValue;
        }

        private void WriteToRegister(uint register, int data)
        {
            if (FastIOState != State.Open)
                return;

            Svc.Log.Debug($"Writing {data:X8} to {register:X4}.");

            int deviceResult = 0;
            int resp = iDmacDrvRegisterWrite(FileHandle, register, data, &deviceResult);
            Console.WriteLine($"iDmacDrvRegisterWrite: {resp}, DeviceResult: {deviceResult}");

            if (resp != 0)
                FastIOState = State.Closed;
        }
    }

    internal struct FastIOColour
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public FastIOColour(byte r, byte g, byte b)
        {
            Red = r; Green = g; Blue = b;
        }
    }

    internal enum State
    {
        Open,
        Closed,
        Error
    }

    internal enum Registers : uint
    {
        RedRegisters = 0x410C,
        GreenRegisters = 0x4108,
        BlueRegisters = 0x4104,
    }
}
