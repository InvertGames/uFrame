using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            
        }
    }


    public static class BitConverter
    {
        public static int ToInt32(byte[] value, int startIndex)
        {
            if (System.BitConverter.IsLittleEndian)
                return System.BitConverter.ToInt32(value, startIndex);
            Array.Reverse((Array)value, startIndex, 4);
            return System.BitConverter.ToInt32(value, startIndex);
        }

        public static float ToSingle(byte[] value, int startIndex)
        {
            if (System.BitConverter.IsLittleEndian)
                return System.BitConverter.ToSingle(value, startIndex);
            Array.Reverse((Array)value, startIndex, 4);
            return System.BitConverter.ToSingle(value, startIndex);
        }

        public static bool ToBoolean(byte[] value, int startIndex)
        {
            return System.BitConverter.ToBoolean(value, startIndex);
        }

        public static byte[] GetBytes(bool value)
        {
            if (System.BitConverter.IsLittleEndian)
                return System.BitConverter.GetBytes(value);
            byte[] bytes = System.BitConverter.GetBytes(value);
            Array.Reverse((Array)bytes);
            return bytes;
        }

        public static byte[] GetBytes(int value)
        {
            if (System.BitConverter.IsLittleEndian)
                return System.BitConverter.GetBytes(value);
            byte[] bytes = System.BitConverter.GetBytes(value);
            Array.Reverse((Array)bytes);
            return bytes;
        }

        public static byte[] GetBytes(float value)
        {
            if (System.BitConverter.IsLittleEndian)
                return System.BitConverter.GetBytes(value);
            byte[] bytes = System.BitConverter.GetBytes(value);
            Array.Reverse((Array)bytes);
            return bytes;
        }
    }
}
