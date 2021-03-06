using Android.Views;
using System;

namespace ControllerHandlerServerApp
{
    public static class Helpers
    {
        public const int GAS_MIDDLE_VALUE = 1500; // number representing "0", eg. no input
        public const int GAS_RANGE = 400; // absolute difference between max/min value and middle value
        public const int TURN_MIDDLE_VALUE = 90;
        public const int TURN_RANGE = 90;
        private static int currentGasPower = 100;
        private static int currentTurnPower = 60;
        private static float GetCenteredAxis(MotionEvent e,
        InputDevice device, Axis axis, int historyPos)
        {
            InputDevice.MotionRange range =
           device.GetMotionRange(axis, e.Source);

            // A joystick at rest does not always report an absolute position of
            // (0,0). Use the Flat property to determine the range of values
            // bounding the joystick axis center.
            if (range != null)
            {
                float flat = range.Flat;
                float value =
                historyPos < 0 ? e.GetAxisValue(axis) :
                e.GetHistoricalAxisValue(axis, historyPos);

                // Ignore axis values that are within the 'flat' region of the
                // joystick axis center.
                if (Math.Abs(value) > flat)
                {
                    return value;
                }
            }
            return 0;
        }

        public static int GetGasValue(MotionEvent e, int historyIndex)
        {
            int gasVal = Helpers.GAS_MIDDLE_VALUE;
            if (e != null && e.IsFromSource(InputSourceType.Joystick))
            {
                if (e.GetAxisValue(Axis.Rtrigger) > 0)
                {
                    gasVal = Helpers.GAS_MIDDLE_VALUE + Convert.ToInt32(currentGasPower * GetCenteredAxis(e, e.Device, Axis.Rtrigger, historyIndex));
                }
                else if (e.GetAxisValue(Axis.Ltrigger) > 0)
                {
                    gasVal = Helpers.GAS_MIDDLE_VALUE - Convert.ToInt32(currentGasPower * GetCenteredAxis(e, e.Device, Axis.Ltrigger, historyIndex));
                }


            }
            return gasVal;
        }

        public static int GetTurnValue(MotionEvent e, int historyIndex)
        {
            int turnVal = Helpers.TURN_MIDDLE_VALUE;
            if (e != null && e.IsFromSource(InputSourceType.Joystick))
            {
                turnVal =  TURN_MIDDLE_VALUE + Convert.ToInt32(currentTurnPower * GetCenteredAxis(e, e.Device, Axis.X, historyIndex));
            }
            return turnVal;

        }
        public static void IncreaseGasValue()
        {
            if (currentGasPower < GAS_RANGE)
                currentGasPower += 10;
        }
        public static void DecreaseGasValue()
        {
            if (currentGasPower > 0)
                currentGasPower -= 10;
        }
        public static int GetCurrentGasValue()
        {
            return currentGasPower;
        }
        public static void IncreaseTurnValue()
        {
            if (currentTurnPower < TURN_RANGE)
                currentTurnPower += 5;
        }
        public static void DecreaseTurnValue()
        {
            if (currentTurnPower > 0)
                currentTurnPower -= 5;
        }
        public static int GetCurrentTurnValue()
        {
            return currentTurnPower;
        }
    }
}