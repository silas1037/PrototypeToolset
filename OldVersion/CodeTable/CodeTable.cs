using System;
using System.Collections.Generic;
using System.Text;
using CODETABLE.Properties;

namespace CODETABLE
{
    /// <summary>
    /// JIS���
    /// </summary>
    public class CodeTable
    {
        /// <summary>
        /// ��ʼ������
        /// </summary>
        public enum InitType
        {
            /// <summary>
            /// �����
            /// </summary>
            Empty,
            /// <summary>
            /// Ĭ�ϵ�JIS���
            /// </summary>
            DftJIS,
            /// <summary>
            /// Ĭ�ϵ�JIS���ֻ�п�ͷ���֣�
            /// </summary>
            DftJIS_HeadPart,
        }
        int[] i2jis;
        int[] jis2i;

        int[] i2ucs;
        int[] ucs2i;

        /// <summary>
        /// ����������
        /// </summary>
        public const int MaxNum = 0x1E80;
        int _index;

        /// <summary>
        /// ���ǰ�����ַ���
        /// </summary>
        public int Num
        {
            get
            {
                int rst = 0;
                for (int i = 1; i <= MaxNum; i++)
                    if (i2ucs[i] != 0)
                        rst++;
                return rst;
            }
        }

        /// <summary>
        /// ��ȡ��������ӵ�λ��
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (1 <= _index && _index <= MaxNum)
                    _index = value;
                else
                    throw new IndexOutOfRangeException("��ų�����");
            }
        }

        /// <summary>
        /// ���ǰ��������
        /// </summary>
        public const int NumOfHead = 1410;

        /// <summary>
        /// ��ȡ��i�������UCSֵ
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int this[int i]
        {
            get { return i2ucs[i]; }
        }

        /// <summary>
        /// ��ʼ��Ϊ�����
        /// </summary>
        public CodeTable()
        {
            Init(InitType.Empty);
        }

        /// <summary>
        /// ���ļ��д������
        /// </summary>
        /// <param name="name">�ļ���</param>
        public CodeTable(string name)
        {
            Init(InitType.Empty);

            System.IO.FileStream fs = System.IO.File.OpenRead(name);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);

            int n = br.ReadInt32();

            for (int i = 1; i <= n; i++)
            {
                int jis = br.ReadUInt16();
                int ucs = br.ReadUInt16();


                int j = jis2i[jis];

                {
                    i2ucs[j] = ucs;
                    ucs2i[ucs] = j;
                }

                if (j > _index)
                    _index = j;
            }

            br.Close();
            fs.Close();
        }

        /// <summary>
        /// ����ʼ��
        /// </summary>
        /// <param name="initType">��ʼ��������</param>
        public CodeTable(InitType initType)
        {
            Init(initType);
        }

        private void Init(InitType initType)
        {
            i2jis = new int[MaxNum + 1];
            i2ucs = new int[MaxNum + 1];

            jis2i = new int[0x10000];
            ucs2i = new int[0x10000];

            System.IO.MemoryStream ms = new System.IO.MemoryStream(Resources.jis2ucs);
            System.IO.BinaryReader br = new System.IO.BinaryReader(ms);

            int n = br.ReadInt32();

            for (int i = 1; i <= n; i++)
            {
                int jis = br.ReadUInt16();
                int ucs = br.ReadUInt16();

                i2jis[i] = jis;
                jis2i[jis] = i;

                if (initType != InitType.Empty && (initType == InitType.DftJIS ||  i <= NumOfHead))
                {
                    int j = jis2i[jis];
                    i2ucs[j] = ucs;
                    ucs2i[ucs] = j;
                }
            }

            br.Close();
            ms.Close();

            switch (initType)
            {
                case InitType.DftJIS:
                    _index = MaxNum + 1;
                    break;
                case InitType.DftJIS_HeadPart:
                    _index = NumOfHead + 1;
                    break;
                default:
                    _index = 1;
                    break;
            }
        }

        /// <summary>
        /// ��JIS��UCS
        /// </summary>
        /// <param name="jis"></param>
        /// <returns></returns>
        public int JIS2UCS(int jis)
        {
            if (!IsJIS(jis))
                throw new IndexOutOfRangeException("�Ƿ���JIS��");

            return i2ucs[jis2i[jis]];
        }

        /// <summary>
        /// ��UCS��JIS
        /// </summary>
        /// <param name="ucs"></param>
        /// <returns></returns>
        public int UCS2JIS(int ucs)
        {
            if (ucs < 0 || ucs > 0x10000)
                throw new IndexOutOfRangeException("�Ƿ���UCS��");

            if (ucs2i[ucs] != 0)
                return i2jis[ucs2i[ucs]];
            else
                return 0;
        }

        /// <summary>
        /// ��i��jis
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int I2JIS(int i)
        {
            return i2jis[i];
        }
        /// <summary>
        /// ��jis��i
        /// </summary>
        /// <param name="jis"></param>
        /// <returns></returns>
        public int JIS2I(int jis)
        {
            return jis2i[jis];
        }

        /// <summary>
        /// ���һ��UCS�����
        /// ����UCS�Ѵ��ڻ���ӳɹ������ظ�UCS��Ӧ��JIS
        /// </summary>
        /// <param name="ucs"></param>
        /// <returns></returns>
        public int Add(int ucs)
        {
            if (ucs < 0 || ucs > 0x10000)
                throw new IndexOutOfRangeException("�Ƿ���UCS��");
            if (ucs2i[ucs] != 0)
                return i2jis[ucs2i[ucs]];

            if (ucs < 0x7F)
                return ucs;

            while (_index <= MaxNum && i2ucs[_index] != 0)
                _index++;

            if (_index <= MaxNum)
            {
                int jis = i2jis[_index];

                i2ucs[_index] = ucs;
                ucs2i[ucs] = _index;
                _index++;

                return jis;
            }
            else
                throw new Exception("����������㣡 ");
        }

        /// <summary>
        /// �ж�һ��ֵ�Ƿ�ΪJIS
        /// </summary>
        /// <param name="jis"></param>
        /// <returns></returns>
        public bool IsJIS(int jis)
        {
            if (jis < 0x100)
            {
                if (jis >= 0x81 && jis <= 0x9F
                    || jis >= 0xE0 && jis <= 0xEA)
                    return true;
                else
                    return false;
            }
            return jis2i[jis] != 0;
        }

        /// <summary>
        /// �����д���ļ�
        /// </summary>
        /// <param name="name"></param>
        public void WriteToFile(string name)
        {
            System.IO.FileStream fs = System.IO.File.Create(name);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);

            int num = Num;
            bw.Write(num);
            for (int i = 1; i <= MaxNum; i++)
                if (i2ucs[i] != 0)
                {
                    bw.Write((UInt16)i2jis[i]);
                    bw.Write((UInt16)i2ucs[i]);
                }

            bw.Close();
            fs.Close();
        }
    }
}
