using System;
using System.Collections.Generic;
using System.Text;

namespace LB_ZIP
{
    public static class LB_ZIP
    {
        /// <summary>
        /// ���ƥ�䳤��
        /// </summary>
        const int max_matchlength = 16;

        /// <summary>
        /// �����ж�ȡһ����Ŀ�ֽڣ���ѹ��д����һ��
        /// </summary>
        /// <param name="streamIn">�������</param>
        /// <param name="inputsize">������ֽ���</param>
        /// <param name="streamOut">д�����</param>
        public static void UnzipTo(System.IO.Stream streamIn, System.IO.Stream streamOut, int count)
        {
            byte[] temp = Unzip(streamIn, count);
            streamOut.Write(temp, 0, temp.Length);
        }

        /// <summary>
        /// ��ѹ�ֽ����飬���ؽ�ѹ����ֽ�����
        /// </summary>
        /// <param name="datain">������ֽ�����</param>
        /// <param name="off">�������ʼλ��</param>
        /// <param name="count">������ֽ���Ŀ</param>
        /// <returns></returns>
        public static byte[] Unzip(byte[] datain, int off, int count)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(datain);
            ms.Seek(off, 0);
            byte[] rst = Unzip(ms, count);
            ms.Close();

            return rst;
        }


        /// <summary>
        /// �����ж�ȡһ����Ŀ�ֽڣ����ؽ�ѹ����ֽ�����
        /// </summary>
        /// <param name="streamIn">�������</param>
        /// <param name="count">������ֽ���Ŀ</param>
        /// <returns></returns>
        public static byte[] Unzip(System.IO.Stream streamIn, int count)
        {
            System.IO.BinaryReader br = new System.IO.BinaryReader(streamIn);
            uint mark = br.ReadUInt32();

            if (mark != 0x00000010)
            {
                throw new Exception("�����ѹ����ʽ��");
            }

            int size = br.ReadInt32();


            Block[] blocks = new Block[0x10000];

            byte[] rst = new byte[size];
            int index = 0;

            int ib = 0;

            count -= 8;
            while(count > 0)
            {
                ib++;

                int x = br.ReadUInt16(); count -= 2;
                blocks[ib].off = index;

                if (x < 0x100)
                {
                    blocks[ib].length = 1;

                    blocks[ib].i = ib;

                    rst[index++] = (byte)x;

                }
                else if (x == 0x100)
                {
                    ib = 0;
                }
                else
                {
                    x -= 0x100;

                    blocks[ib].i = x;

                    blocks[ib].length = blocks[x].length + 1;

                    for (int j = 0; j < blocks[ib].length; j++)
                    {
                        rst[index++] = rst[blocks[x].off + j]; 
                    }
                }
            }

            return rst;
        }

        struct Block
        {
            public int off;
            public int length;

            public int i;
        }

        /// <summary>
        /// �����ж�ȡһ���ֽڣ�����ѹ������ֽ�����
        /// </summary>
        /// <param name="streamIn">�������</param>
        /// <param name="count">������ֽ���</param>
        /// <returns></returns>
        public static byte[] Zip(System.IO.Stream streamIn, int count)
        {
            byte[] b = new byte[count];
            streamIn.Read(b, 0, b.Length);
            return Zip(b, 0, b.Length);
        }

        const int maxBlocks = 0xFEFE;
        /// <summary>
        /// ��һ���ֽ�����ıλ�����һ����Ŀ�ֽ�����ѹ��������ѹ�����ֽ�����
        /// </summary>
        /// <param name="datain">������ֽ�����</param>
        /// <param name="off">�������ʼλ��</param>
        /// <param name="count">������ֽ���Ŀ</param>
        /// <returns></returns>
        public static byte[] Zip(byte[] datain, int off, int count)
        {
            Block[] blocks = new Block[count + count / maxBlocks + 2];

            //Hash���ұ�
            int[] Link_Head = new int[hash_max + 1];
            int[] Link_Table = new int[count + 1];

            int id = off;
            int ib = 1;

            while (id < off + count)
            {
                int hash,j;

                if (id > off + count - hash_bytenum)
                {
                    hash = j = 0;
                }
                else
                {
                    hash = Hash(datain, id);
                    j = Link_Head[hash];
                }

                int k = ib;
                int M = 1;

                while (j != 0 && id + blocks[j].length + 1 < off + count)
                {
                    if(blocks[j].length + 1 > M && MatchLength(datain, id, blocks[j].off,blocks[j].length + 1))
                    {
                        k = j;
                        M = blocks[j].length + 1;
                    }
                    j = Link_Table[j];
                }

                blocks[ib].i = k;
                blocks[ib].off = id;
                blocks[ib].length = M;

                Link_Table[ib] = Link_Head[hash];
                Link_Head[hash] = ib;

                ib++;
                id += M;

                if (ib % maxBlocks == 0 && id < off + count)
                {
                    Link_Head = new int[hash_max + 1];
                    Link_Table = new int[count + 1];

                    blocks[ib].i = 0;
                    blocks[ib].length = 0;
                    blocks[ib].off = id;

                    ib++;
                }
            }

            byte[] rst = new byte[8 + (ib - 1) * 2];
            rst[0] = 0x10;

            for (int i = 0; i < 4; i++)
                rst[4 + i] = (byte)(count >> (i * 8));

            int ir = 8;
            for (int i = 1; i < ib; i++)
            {
                if (blocks[i].length == 1)
                {
                    rst[ir++] = datain[blocks[i].off];
                    rst[ir++] = 0;
                }
                else
                {
                    int x = blocks[i].i % maxBlocks + 0x100;
                    rst[ir++] = (byte)x;
                    rst[ir++] = (byte)(x >> 8);
                }
            }

            return rst;
        }

        static bool MatchLength(byte[] b, int off1, int off2, int count)
        {
            for (int i = 0; i < count; i++)
                if (b[off1 + i] != b[off2 + i])
                    return false;

            return true;
        }

        const int hash_max = 0xFFFF;
        const int hash_bytenum = 2;
        static int Hash(byte[] b, int off)
        {
            return (b[off] << 8) | b[off + 1];
        }
    }
}
