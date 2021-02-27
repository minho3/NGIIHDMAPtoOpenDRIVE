using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dBASE.NET;
using System.IO;
using System.Windows.Forms;

namespace ShapeToXodr
{
    class SHPReader
    {
        public const int SHPT_NULL = 0;
        public const int SHPT_POINT = 1;
        public const int SHPT_ARC = 3;
        public const int SHPT_POLYGON = 5;
        public const int SHPT_MULTIPOINT = 8;
        public const int SHPT_POINTZ = 11;
        public const int SHPT_ARCZ = 13;
        public const int SHPT_POLYGONZ = 15;
        public const int SHPT_MULTIPOINTZ = 18;
        public const int SHPT_POINTM = 21;
        public const int SHPT_ARCM = 23;
        public const int SHPT_POLYGONM = 25;
        public const int SHPT_MULTIPOINTM = 28;
        public const int SHPT_MULTIPATCH = 31;
        public static List<string> NGIList = new List<string>(); // shp 파일 리스트를 저장하기 위한 변수
        //private static string[] NGIIMapList = {"A1_Node","A2_LINK","..."}; // 레이어 리스트 작성

        public struct Box // 바운딩 박스 정보를 저장하기 위한 구조체
        {
            public double Xmin;
            public double Xmax;
            public double Ymin;
            public double Ymax;
            public double Mmin;
            public double Mmax;
            public double Zmin;
            public double Zmax;
        }

        public struct Point //포인트 하나를 구성하는 구조체
        {
            public double X;
            public double Y;
            public double Z;
            public double M;
        }

        public struct Single_Layer // 레이어 이름, SHP+DBF를 읽은 데이터, 타입, 바운딩박스를 저장하기위한 구조체
        {
            public string LayerName;
            public int shpType;
            public List<SHPData> SHPData;
            public Box BoundBox;
        }

        public static string shpTypeintToStr(int shpType) //int 형 shp Type을 String 으로 변환
        {
            string Rdata = null;

            switch (shpType)
            {
                case SHPT_POINTZ:
                    Rdata = "PointZ";
                    break;
                case SHPT_ARCZ:
                    Rdata = "PolyLineZ";
                    break;
                case SHPT_POLYGONZ:
                    Rdata = "PolygonZ";
                    break;
                case SHPT_MULTIPOINTZ:
                    Rdata = "MultiPointZ";
                    break;
            }

            return Rdata;
        }

        public class SHPData //SHPShape 에서 데이터를 통합 및 변환하고 DBF 속성정보를 포함하는 클래스
        {
            private int index; //데이터 순서 DBF 의 데이터를 가져오기 위한 변수
            private int Type;
            private Box bBox; // 바운딩 박스
            private int numParts; // Part의 갯수
            private int numPoints; // Point의 갯수 PointZ만
            private int[] Parts; //Polygon 이나 PloyLine 의 요소를 저장하는 배열
            private Point[] Points; //모든 점의 정보를 저장하는 배열
            public Dictionary<string, object> DBFData; //해당 object의 속성정보를 저장하는 데 사용

            public SHPData(int Type)
            {
                this.Type = Type;
            }

            public SHPData()
            {

            }

            public Box BBox
            {
                get
                {
                    return bBox;
                }
                set
                {
                    bBox = value;
                }
            }

            public int NumParts
            {
                get
                {
                    return numParts;
                }
                set
                {
                    numParts = value;
                }
            }

            public int NumPoints
            {
                get
                {
                    return numPoints;
                }
                set
                {
                    numPoints = value;
                }
            }

            public Point[] APoints
            {
                get
                {
                    return Points;
                }
                set
                {
                    Points = value;
                }
            }

            public int[] AParts
            {
                get
                {
                    return Parts;
                }
                set
                {
                    Parts = value;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    index = value;
                }
            }
        }

        private static int EndianConverter(int input)// 엔디안 컨버터 리틀엔디안 -> 빅엔디안
        {
            byte[] data = BitConverter.GetBytes(input);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToInt32(data, 0);
        }

        private static FileStream JumpXBytes(FileStream fs, int x) // 쓰지않는 데이터를 건너뛸때 매개변수인 x만큼 건너뜀
        {
            for (int i = 0; i < x; i++)
            {
                fs.ReadByte();
            }
            return fs;
        }

        private static void ReadMainHeader(FileStream fs, ref int fsLength, ref int fsType, ref Box MainBox) //매개변수로 받은 Filestream(SHP File)에서 메인헤더 100바이트를 읽고 총길이와 타입을 매개변수에 전달한다.
        {
            byte[] shpType = new byte[4];

            JumpXBytes(fs, 32);
            fs.Read(shpType, 0, 4);
            fsType = BitConverter.ToInt32(shpType, 0);
            fsLength = Convert.ToInt32(fs.Length);
            MainBox = ReadBbox(fs, true);
        }

        private static void ReadContentHeader(FileStream fs) // 컨텐츠(레코드) 헤더를 건너뛰는 함수 컨텐츠 번호화 타입이 들어가있지만 필요가 없음
        {
            JumpXBytes(fs, 8);
        }

        private static Point PointReader(FileStream fs, bool bZM = false) // Point 하나를 읽기위한 함수
        {
            int val = 0;
            if (bZM) // PointZ 에서는 순서대로 다나오기때문에 그대로 읽지만 다른형태는 구조가 달라서 구분하기 위한부분
            {
                val = 4;
            }
            else
            {
                val = 2;
            }
            Point tmPoint = new Point();
            byte[] Pz = new byte[val * 8];
            double[] locs = new double[val];
            fs.Read(Pz, 0, val * 8);
            for (int i = 0; i < val * 8; i += 8)
            {
                locs[i / 8] = BitConverter.ToDouble(Pz, i);
            }
            tmPoint.X = locs[0];
            tmPoint.Y = locs[1];
            if (bZM)
            {
                tmPoint.Z = locs[2];
                tmPoint.M = locs[3];
            }

            return tmPoint;
        }

        private static Box ReadBbox(FileStream fs, bool bZM = false) //BoundingBox 읽는 부분 a 값이 true 이면 Z와 M 데이터 도 받아오고 기본으로 X,Y 만 받음
        {
            Box tBox = new Box();
            byte[] tByte;
            if (bZM)
            {
                tByte = new byte[64];
                fs.Read(tByte, 0, 64);
            }
            else
            {
                tByte = new byte[32];
                fs.Read(tByte, 0, 32);
            }

            tBox.Xmin = BitConverter.ToDouble(tByte, 0);
            tBox.Ymin = BitConverter.ToDouble(tByte, 8);
            tBox.Xmax = BitConverter.ToDouble(tByte, 16);
            tBox.Ymax = BitConverter.ToDouble(tByte, 24);

            if (bZM)
            {
                tBox.Zmin = BitConverter.ToDouble(tByte, 32);
                tBox.Zmax = BitConverter.ToDouble(tByte, 40);
                tBox.Mmin = BitConverter.ToDouble(tByte, 48);
                tBox.Mmax = BitConverter.ToDouble(tByte, 56);
            }

            return tBox;
        }

        private static int Read4BToint(FileStream fs)// 4바이트를 읽고 Int 형으로 변환해주는 함수
        {
            byte[] tByte = new byte[4];
            int RData;

            fs.Read(tByte, 0, 4);
            RData = BitConverter.ToInt32(tByte, 0);

            return RData;
        }

        private static void ReadMZData(FileStream fs, ref SHPData shp)
        {
            Box Tbox = shp.BBox;
            byte[] TData = new byte[16];

            fs.Read(TData, 0, 16);
            Tbox.Zmin = BitConverter.ToDouble(TData, 0);
            Tbox.Zmax = BitConverter.ToDouble(TData, 8);

            for (int i = 0; i < shp.NumPoints; i++)
            {
                fs.Read(TData, 0, 8);
                shp.APoints[i].Z = BitConverter.ToDouble(TData, 0);
            }

            fs.Read(TData, 0, 16);
            Tbox.Mmin = BitConverter.ToDouble(TData, 0);
            Tbox.Mmax = BitConverter.ToDouble(TData, 8);

            for (int i = 0; i < shp.NumPoints; i++)
            {
                fs.Read(TData, 0, 8);
                shp.APoints[i].M = BitConverter.ToDouble(TData, 0);
            }

            shp.BBox = Tbox;
        }

        private static Dbf AddDbfContent(string fsName)//dbf 파일은 가져오기위한 함수
        {
            Encoding encoding = Encoding.GetEncoding(949); // 한국어 표시문제 해결을 위한 인코딩 변경
            Dbf ReadDbf = new Dbf(encoding);
            ReadDbf.Read(Path.ChangeExtension(fsName, ".dbf"));

            return ReadDbf;
        }

        private static List<SHPData> ReadContents(FileStream fs, int shpType, int numContent)//타입을 보고 모든 콘텐츠(레코드)를 읽어오는 부분
        {
            int index = 0;
            List<SHPData> RListshps = new List<SHPData>();

            for (int i = index; i < numContent; i++)
            {
                SHPData tmpSHPContent = new SHPData(shpType)
                {
                    Index = index
                };
                ReadContentHeader(fs); //콘텐츠 헤더를 넘김
                fs.Position += 4; // 타입을 넘김

                switch (shpType)
                {
                    case SHPT_POINTZ: // 타입이 PointZ일때 실행하는 부분

                        tmpSHPContent.NumPoints = 1;
                        tmpSHPContent.APoints = new Point[1];
                        tmpSHPContent.APoints[0] = PointReader(fs, true);

                        break;

                    case SHPT_MULTIPOINTZ: //타입이 MultiPointZ 일때 실행 되는 부분

                        tmpSHPContent.BBox = ReadBbox(fs);
                        tmpSHPContent.NumPoints = Read4BToint(fs);

                        tmpSHPContent.APoints = new Point[tmpSHPContent.NumPoints];

                        for (int j = 0; j < tmpSHPContent.NumPoints; j++)
                        {
                            tmpSHPContent.APoints[j] = PointReader(fs);
                        }

                        ReadMZData(fs, ref tmpSHPContent);

                        break;

                    case SHPT_ARCZ:
                    case SHPT_POLYGONZ: //PolyLine이나 PolyGon일때 실행되는 부분 구조적으로 같음

                        tmpSHPContent.BBox = ReadBbox(fs);
                        tmpSHPContent.NumParts = Read4BToint(fs);
                        tmpSHPContent.NumPoints = Read4BToint(fs);

                        tmpSHPContent.AParts = new int[tmpSHPContent.NumParts];
                        tmpSHPContent.APoints = new Point[tmpSHPContent.NumPoints];

                        for (int j = 0; j < tmpSHPContent.NumParts; j++)
                        {
                            tmpSHPContent.AParts[j] = Read4BToint(fs);
                        }

                        for (int j = 0; j < tmpSHPContent.NumPoints; j++)
                        {
                            tmpSHPContent.APoints[j] = PointReader(fs);
                        }

                        ReadMZData(fs, ref tmpSHPContent);

                        break;

                    default:
                        return null; // 잘못된 형식이 들어온것이때문에 예외 처리 해야함
                }
                RListshps.Add(tmpSHPContent);
                index++;
            }

            fs.Close();
            return RListshps;
        }

        private static int getContentNumber(FileStream fs) // 컨텐츠(레코드)의 갯수를 파악하여 반환해주는 함수 SHP 파일보다 SHX 파일에서 구하기 쉬움
        {
            FileStream fshx = new FileStream(Path.ChangeExtension(fs.Name, ".shx"), FileMode.Open); //shp파일의 풀경로를 얻어외 .shp를 .shx 로 변환하여 열어줌
            int ContentCounter = 0;

            for (int i = Convert.ToInt32(fshx.Position = 100); i < Convert.ToInt32(fshx.Length); i = i + 8) // shx 파일도 헤더(100바이트) + 레코드번호(4바이트) + 길이(4바이트) 형식으로 이루어져있음
            {
                ContentCounter++;
            }
            fshx.Close();

            return ContentCounter;
        }

        private static void getDBFDataTotalize(Dbf dbf, List<SHPData> ShpLayers) // 해당 SHPData의 DBF 속성정보를 추가해줌
        {
            for (int dindex = 0; dindex < ShpLayers.Count; dindex++)
            {
                Dictionary<string, object> DBFData = new Dictionary<string, object>();
                for (int findex = 0; findex < dbf.Fields.Count; findex++)
                {
                    DBFData.Add(dbf.Fields[findex].Name, dbf.Records[dindex].Data[findex]);
                }
                ShpLayers[dindex].DBFData = DBFData;
            }
        }

        private static List<string> getFileList(DirectoryInfo path) // .shp인 모든 파일의 이름을 가져오는함수 A2_LANE2 같은 이름도 가져옴
        {
            FileInfo[] shps;
            //NGII HD Map Layer List 추가후 검증 리스트에 없는 이름일경우 확인 필요
            try
            {
                shps = path.GetFiles("*.shp"); // .shp인 파일을 리스트로 반환
            }
            catch
            {
                return null;
            }

            NGIList.Clear(); // 기존 저장된 데이터 삭제

            foreach (FileInfo conv in shps)
            {
                string cutStr = conv.Name;
                cutStr = cutStr.Substring(0, cutStr.Length - 4); // 확장자가 포함된 이름을 제거하는부분 확장자는 추후 변동이 있기때문
                NGIList.Add(cutStr);
            }
            return NGIList;
        }

        public static List<Single_Layer> readAllNGIIfiles(string FolderPath) // 모든 shp 파일과 dbf 파일을 읽어 하나의 배열에 저장하여 반환
        {
            Single_Layer allData = new Single_Layer();
            List<Single_Layer> ReturnDatas = new List<Single_Layer>();
            List<SHPData> shpContents = new List<SHPData>();
            DirectoryInfo convDir = new DirectoryInfo(FolderPath);

            getFileList(convDir);
            FolderPath += "\\";

            foreach (string str in NGIList)
            {
                string tmpstr = FolderPath + str + ".shp"; //"select Path/" + str + ".shp";
                FileInfo Finfo = new FileInfo(tmpstr);

                FileStream ImporingD = new FileStream(tmpstr, FileMode.Open);
                int Length = 0;
                int shpType = 0;
                int NumContent;
                Box MainBox = new Box();

                ReadMainHeader(ImporingD, ref Length, ref shpType, ref MainBox);
                NumContent = getContentNumber(ImporingD);

                allData.LayerName = str;
                shpContents = ReadContents(ImporingD, shpType, NumContent);
                if (shpContents != null)
                {
                    getDBFDataTotalize(AddDbfContent(Finfo.FullName), shpContents);
                    allData.SHPData = shpContents;
                    allData.shpType = shpType;
                    allData.BoundBox = MainBox;

                    ReturnDatas.Add(allData);
                }
            }

            if (ReturnDatas.Count != 0)
            {
                return ReturnDatas;
            }
            else
            {
                MessageBox.Show(".shp 파일이 없습니다\n다른 디렉토리를 지정해주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
