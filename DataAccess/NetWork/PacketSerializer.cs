using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.NetWork
{
    public class PacketSerializer
    {
        private MemoryStream m_buffer = null;
        private int m_offset = 0;

        public PacketSerializer()
        {
            m_buffer = new MemoryStream();
        }

        public void Clear()
        {
            byte[] buffer = m_buffer.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);

            m_buffer.Position = 0;
            m_buffer.SetLength(0);
            m_offset = 0;
        }

        public bool Serialize(bool element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(bool));
        }

        public bool Serialize(char element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(char));
        }

        public bool Serialize(float element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(float));
        }

        public bool Serialize(double element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(double));
        }

        public bool Serialize(byte[] element)
        {
            return WriteBuffer(element, element.Length);
        }


        public bool Serialize(byte element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(byte));
        }

        public bool Serialize(short element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(short));
        }

        public bool Serialize(ushort element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(ushort));
        }

        public bool Serialize(int element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(int));
        }

        public bool Serialize(uint element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(uint));
        }

        public bool Serialize(long element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(long));
        }

        public bool Serialize(ulong element)
        {
            byte[] data = BitConverter.GetBytes(element);

            return WriteBuffer(data, sizeof(ulong));
        }

        public bool Serialize(string element)
        {

            byte[] buffer = Encoding.UTF8.GetBytes(element);
            byte[] data = new byte[buffer.Length];

            //int size = Math.Min(buffer.Length, data.Length);
            Buffer.BlockCopy(buffer, 0, data, 0, buffer.Length);

            return WriteBuffer(data, data.Length);
        }

        public bool Deserialize(ref bool element)
        {
            int size = sizeof(bool);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToBoolean(data, 0);

            return ret;
        }

        public bool Deserialize(ref char element)
        {
            int size = sizeof(char);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToChar(data, 0);

            return ret;
        }

        public bool Deserialize(ref float element)
        {
            int size = sizeof(float);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToSingle(data, 0);

            return ret;
        }

        public bool Deserialize(ref double element)
        {
            int size = sizeof(double);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToDouble(data, 0);

            return ret;
        }

        public bool Deserialize(ref short element)
        {
            int size = sizeof(short);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToInt16(data, 0);

            return ret;
        }

        public bool Deserialize(ref ushort element)
        {
            int size = sizeof(ushort);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToUInt16(data, 0);

            return ret;
        }

        public bool Deserialize(ref int element)
        {
            int size = sizeof(int);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToInt32(data, 0);

            return ret;
        }

        public bool Deserialize(ref uint element)
        {
            int size = sizeof(uint);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToUInt32(data, 0);

            return ret;
        }

        public bool Deserialize(ref long element)
        {
            int size = sizeof(long);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToInt64(data, 0);

            return ret;
        }

        public bool Deserialize(ref ulong element)
        {
            int size = sizeof(ulong);
            byte[] data = new byte[size];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
                element = BitConverter.ToUInt64(data, 0);

            return ret;
        }

        public bool Deserialize(ref byte[] element, int length)
        {
            bool ret = ReadBuffer(ref element, length);

            return ret;
        }

        public bool Deserialize(ref string element, int length)
        {

            byte[] data = new byte[length];

            bool ret = ReadBuffer(ref data, data.Length);

            if (ret)
            {
                string str = Encoding.UTF8.GetString(data);
                element = str.Trim('\0');

                return true;
            }

            return false;
        }

        public bool ReadBuffer(ref byte[] data, int size)
        {
            try
            {
                m_buffer.Position = m_offset;
                m_buffer.Read(data, 0, size);
                m_offset += size;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool WriteBuffer(byte[] data, int size)
        {
            try
            {
                m_buffer.Position = m_offset;
                m_buffer.Write(data, 0, size);
                m_offset += size;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public byte[] GetBytes()
        {
            byte[] data = new byte[m_buffer.Length];
            Buffer.BlockCopy(m_buffer.GetBuffer(), 0, data, 0, m_offset);
            m_offset = 0;
            m_buffer.Flush();

            return data;
        }
    }
}
