using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucid_Dream_Explorer
{
    static class Variable
    {
        public static int? map { get { return getInt32(Offset.map); } }
        public static int? map_pos { get { return getInt32(Offset.map_pos); } }

        public static int? chart_draw_x { get { return getInt32(Offset.chart_draw_x); } }
        public static int? chart_draw_y { get { return getInt32(Offset.chart_draw_y); } }

        public static int? chart_return_x { get { return getInt16(Offset.chart_return_x); } }
        public static int? chart_return_y { get { return getInt16(Offset.chart_return_y); } }

        public static int? event_x { get { return getInt32(Offset.event_x); } }
        public static int? event_y { get { return getInt32(Offset.event_y); } }
        public static int? events { get { return getInt32(Offset.events); } }

        public static int? gray_enabled
        {
            get
            {
                int? enabled = getInt32ByPtr(Offset.gray_ptr, Offset.gray_flag_enabled);
                if (enabled == null || (uint)enabled > 1) return null; //Boolean
                return enabled;
            }
        }

        private static int last_chart_x = 0;
        public static int? chart_x
        {
            get
            {
                int? x = chart_draw_x;
                if (x != null && last_chart_x != x)
                {
                    int? xret = chart_return_x;
                    if (xret != null && x == xret)
                    {
                        return x;
                    }
                }
                return null; //Unchanged or error
            }
        }

        private static int last_chart_y = 0;
        public static int? chart_y
        {
            get
            {
                int? y = chart_draw_y;
                if (y != null && last_chart_y != y)
                {
                    int? yret = chart_return_y;
                    if (yret != null && y == yret)
                    {
                        return y;
                    }
                }
                return null; //Unchanged or error
            }
        }

        private static int? getInt32(int offset)
        {
            IntPtr? addr = Emulator.baseAddr;
            if (Emulator.baseAddr == null)
            {
                return null; //Fatal error
            }
            byte[] readBuf = new byte[4];

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return null;

            IntPtr pointer = new IntPtr((int)addr + offset);
            mio.MemoryRead(pointer, readBuf);
            int value = BitConverter.ToInt32(readBuf, 0);
            return value;
        }

        //return pointerOffset[offset], get a value by using a relative pointer
        private static int? getInt32ByPtr(uint pointerOffset, int offset)
        {
            IntPtr? addr = Emulator.baseAddr;
            if (Emulator.baseAddr == null)
            {
                return null; //Fatal error
            }
            byte[] readBuf = new byte[4], readBuf2 = new byte[4];

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return null;

            IntPtr pointerPointer = (IntPtr)((int)addr + pointerOffset);
            mio.MemoryRead(pointerPointer, readBuf);
            uint pointer = BitConverter.ToUInt32(readBuf, 0);
            pointer &= ~0xA0000000; //Unset virtual memory cached/uncached flags
            if (pointer == 0) return null;

            mio.MemoryRead((IntPtr)((int)addr + pointer + offset), readBuf2);
            int value = BitConverter.ToInt32(readBuf2, 0);
            return value;
        }

        private static Int16? getInt16(int offset)
        {
            IntPtr? addr = Emulator.baseAddr;
            if (Emulator.baseAddr == null)
            {
                return null; //Fatal error
            }
            byte[] readBuf = new byte[2];

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return null;

            IntPtr pointer = new IntPtr((int)addr + offset);
            mio.MemoryRead(pointer, readBuf);
            Int16 value = BitConverter.ToInt16(readBuf, 0);
            return value;
        }
    }
}
