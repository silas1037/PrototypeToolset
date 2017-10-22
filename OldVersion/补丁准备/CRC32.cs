using System;
using System.Collections.Generic;
using System.Text;

namespace ����׼��
{
    /// <summary>
    /// CRC32
    /// </summary>
    public static class Crc32
    {
        private static uint[] crc32table;

        static void GetCrc32Table()
        {
            crc32table = new uint[256];
            for (int i = 0; i < crc32table.Length; i++)
            {
                uint crc = (uint)i;
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ 0xEDB88320;
                    else
                        crc >>= 1;
                }
                crc32table[i] = crc;
            }
        }

        static Crc32()
        {
            GetCrc32Table();
        }

        /// <summary>
        /// ��ȡ�ֽ������CRC32
        /// </summary>
        /// <param name="b">��Ҫ����CRC32���ֽ�����</param>
        /// <param name="start">���</param>
        /// <param name="count">�ֽ���</param>
        /// <returns></returns>
        public static uint GetCrc32FromByteArray(byte[] b, int start, int count)
        {
            uint value = 0xFFFFFFFF;
            for (int i = 0; i < count; i++)
            {
                value = (value >> 8) ^ crc32table[(value ^ b[start + i]) & 0xFF];
            }

            value ^= 0xFFFFFFFF;
            return value;
        }

        /// <summary>
        /// ��ȡ�ֽ������CRC32
        /// </summary>
        /// <param name="b">��Ҫ����CRC32���ֽ�����</param>
        /// <returns></returns>
        public static uint GetCrc32FromByteArray(byte[] b)
        {
            return GetCrc32FromByteArray(b, 0, b.Length);
        }

        /// <summary>
        /// ��������ָ���ֽ�����CRC32
        /// </summary>
        /// <param name="stream">��Ҫ����CRC32����</param>
        /// <param name="count">��Ҫ������ֽ���</param>
        /// <returns></returns>
        public static uint GetCrc32FromStream(System.IO.Stream stream, int count)
        {
            //�����С
            const int buffsize = 4 * 1024;

            uint value = 0xFFFFFFFF;
            while (count > 0)
            {
                int size = count > buffsize ? buffsize : count;
                byte[] buff = new byte[size];
                stream.Read(buff, 0, size);
                for (int i = 0; i < size; i++)
                {
                    value = (value >> 8) ^ crc32table[(value ^ buff[i]) & 0xFF];
                }
                count -= buffsize;
            }

            value ^= 0xFFFFFFFF;
            return value;
        }
    }
}
