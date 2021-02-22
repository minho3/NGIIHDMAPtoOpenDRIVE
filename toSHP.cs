using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using dBASE.NET;

namespace ShapeToXodr
{
    class ToSHP
    {
        FileStream fs;
        byte[] rev;
        readonly byte[] n = new byte[20];
        int TotalLength; // 전체 길이
        int ContentLength; // 콘텐츠 길이
        int cntr;
        string name = "referenceLine";
        SHPReader.SHPData data;

        public ToSHP(List<SHPReader.Point> movPoint, SHPReader.Box bBox, string[] ID = null, int[] brokelist = null, int Type = SHPReader.SHPT_POINT, string name = null)
        {
            if (name != null)
            {
                this.name = name;
            }

            data = new SHPReader.SHPData(Type)
            {
                APoints = new SHPReader.Point[movPoint.Count()],
                NumParts = 1,
                AParts = new int[1] { 0 },
                BBox = bBox,
                NumPoints = movPoint.Count()
            };

            switch (Type)
            {
                case SHPReader.SHPT_ARC:
                    {
                        cntr = brokelist.Length;
                        ContentLength = 44 + 4 * data.NumParts + 16 * movPoint.Count();
                        TotalLength = 100 + cntr * 8 + cntr * ContentLength;
                        break;
                    }
                case SHPReader.SHPT_POINT:
                    {
                        cntr = movPoint.Count();
                        ContentLength = 20;
                        TotalLength = 100 + cntr * 8 + cntr * ContentLength;
                        break;
                    }
            }

            for (int i = 0; i < data.NumPoints; i++)
            {
                data.APoints[i].X = movPoint[i].X;
                data.APoints[i].Y = movPoint[i].Y;
                data.APoints[i].M = 0;
                data.APoints[i].Z = 0;
            }

            fs = File.Create(@".\" + this.name + ".shp");

            MakeMainHeader(Type, bBox);
            if (brokelist != null)
            {
                DataConvert(Type, movPoint, bBox, brokelist);
            }
            else
            {
                DataConvert(Type, movPoint, bBox);
            }

            fs.Close();
            //------------------------shp------------------------
            fs = File.Create(@".\" + this.name + ".shx");

            MakeMainHeader(Type, bBox, true);

            int loc = 50;
            if (Type == SHPReader.SHPT_POINT)
            {
                for (int i = 0; i < cntr; i++)
                {
                    rev = BitConverter.GetBytes(loc); // 시작위치

                    Array.Reverse(rev);
                    fs.Write(rev, 0, 4);

                    rev = BitConverter.GetBytes(ContentLength / 2);//길이

                    Array.Reverse(rev);
                    fs.Write(rev, 0, 4);

                    loc += (ContentLength + 8) / 2;
                }
            }
            else
            {
                for (int i = 0; i < cntr; i++)
                {
                    rev = BitConverter.GetBytes(loc); // 시작위치

                    Array.Reverse(rev);
                    fs.Write(rev, 0, 4);

                    ContentLength = 44 + 4 * data.NumParts + 16 * brokelist[i];

                    rev = BitConverter.GetBytes(ContentLength / 2);//길이

                    Array.Reverse(rev);
                    fs.Write(rev, 0, 4);

                    loc += (ContentLength + 8) / 2;
                }

            }

            fs.Close();
            //------------------------shx--------------------
            Dbf dbf = new Dbf();

            DbfField field = new DbfField("ID", DbfFieldType.Character, 6);
            dbf.Fields.Add(field);
            dbf.Fields.Add(new DbfField("index", DbfFieldType.Character, 6));

            DbfRecord[] record = new DbfRecord[movPoint.Count()];

            for (int i = 0; i < cntr; i++)
            {
                record[i] = dbf.CreateRecord();
                record[i].Data[0] = ID[i].Substring(7, 5);
                record[i].Data[1] = i + 1;
            }

            dbf.Write(this.name + ".dbf", DbfVersion.VisualFoxPro);
        }

        private void DataConvert(int Type, List<SHPReader.Point> movPoint, SHPReader.Box bBox, int[] brokelist = null)//타입별로 ShapeFile 데이터 생성부분
        {
            switch (Type)
            {
                case SHPReader.SHPT_POINT:
                    {
                        for (int i = 0; i < cntr; i++)
                        {
                            rev = BitConverter.GetBytes(i + 1);//Record Number
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(rev);

                            fs.Write(rev, 0, 4);


                            rev = BitConverter.GetBytes(ContentLength / 2);//ContentLength
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(rev);

                            fs.Write(rev, 0, 4);

                            rev = BitConverter.GetBytes(Type);

                            fs.Write(rev, 0, 4);

                            rev = BitConverter.GetBytes(movPoint[i].X);

                            fs.Write(rev, 0, 8);

                            rev = BitConverter.GetBytes(movPoint[i].Y);

                            fs.Write(rev, 0, 8);
                        }

                        break;
                    }
                case SHPReader.SHPT_ARC:
                    {
                        int j = 0;
                        int idx = 0;

                        for (int i = 0; i < cntr; i++)
                        {
                            if (i == 0)
                            {
                                j = 0;
                                idx = brokelist[i];
                            }
                            else
                            {
                                idx += brokelist[i];
                            }
                            ContentLength = 44 + 4 * data.NumParts + 16 * brokelist[i];


                            rev = BitConverter.GetBytes(i + 1);//Record Number
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(rev);

                            fs.Write(rev, 0, 4);


                            rev = BitConverter.GetBytes(ContentLength / 2);//ContentLength
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(rev);

                            fs.Write(rev, 0, 4);

                            rev = BitConverter.GetBytes(Type);

                            fs.Write(rev, 0, 4);

                            rev = BitConverter.GetBytes(bBox.Xmin);

                            fs.Write(rev, 0, 8);

                            rev = BitConverter.GetBytes(bBox.Ymin);

                            fs.Write(rev, 0, 8);

                            rev = BitConverter.GetBytes(bBox.Xmax);

                            fs.Write(rev, 0, 8);

                            rev = BitConverter.GetBytes(bBox.Ymax);

                            fs.Write(rev, 0, 8);

                            rev = BitConverter.GetBytes(data.NumParts);

                            fs.Write(rev, 0, 4);

                            rev = BitConverter.GetBytes(brokelist[i]);

                            fs.Write(rev, 0, 4);

                            for (int k = 0; k < data.NumParts; k++)
                            {
                                rev = BitConverter.GetBytes(data.AParts[k]);

                                fs.Write(rev, 0, 4);
                            }

                            for (; j < idx; j++)
                            {
                                rev = BitConverter.GetBytes(data.APoints[j].X);

                                fs.Write(rev, 0, 8);

                                rev = BitConverter.GetBytes(data.APoints[j].Y);

                                fs.Write(rev, 0, 8);
                            }
                        }
                        break;
                    }
            }
        }

        private void MakeMainHeader(int Type, SHPReader.Box bBox, bool isShx = false)//shapeFile의 메인 Header 생성부분
        {
            rev = BitConverter.GetBytes(9994);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(rev);
            }
            fs.Write(rev, 0, 4);

            fs.Write(n, 0, n.Length);
            if (isShx)
            {
                rev = BitConverter.GetBytes((100 + cntr * 8) / 2);
            }
            else
            {
                rev = BitConverter.GetBytes(TotalLength / 2); //총길이
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(rev);

            fs.Write(rev, 0, 4);

            rev = BitConverter.GetBytes(1000);

            fs.Write(rev, 0, 4);

            rev = BitConverter.GetBytes(Type);

            fs.Write(rev, 0, 4);

            rev = BitConverter.GetBytes(bBox.Xmin);

            fs.Write(rev, 0, 8);

            rev = BitConverter.GetBytes(bBox.Ymin);

            fs.Write(rev, 0, 8);

            rev = BitConverter.GetBytes(bBox.Xmax);

            fs.Write(rev, 0, 8);

            rev = BitConverter.GetBytes(bBox.Ymax);

            fs.Write(rev, 0, 8);

            rev = BitConverter.GetBytes(0.0);

            fs.Write(rev, 0, 8);

            rev = BitConverter.GetBytes(0.0);

            fs.Write(rev, 0, 8);

            rev = BitConverter.GetBytes(bBox.Mmin);

            fs.Write(rev, 0, 8);

            rev = BitConverter.GetBytes(bBox.Mmax);

            fs.Write(rev, 0, 8); //Header
        }
    }
}
