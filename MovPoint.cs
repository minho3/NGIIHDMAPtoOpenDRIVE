using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = ShapeToXodr.SHPReader.Point;

namespace ShapeToXodr
{
    class MovPoint
    {
        public double[,] ptData;
        public List<Point> movPoints;
        public List<List<Point>> movPoint2D = new List<List<Point>>(); //링크별 점 객수
        public double[] hdgList;
        public double[][] hdgList2D; // 2차원 배열 형식으로 링크 구분 하기위함
        public double[][] Length2D;
        public string[] lnkID;

        public MovPoint(ref List<SHPReader.SHPData> links) //모든 링크의 점들을 이동하여 referenceLine 을 구성함
        {
            int idx = 0;
            ptData = new double[links.Count, 2]; //점 갯수, 길이
            hdgList2D = new double[links.Count][];
            Length2D = new double[links.Count][];
            lnkID = new string[links.Count];
            double AngleX = 0, AngleY = 0; //기울기 저장 변수
            double Odegree, Cdegree; //각도 저장 변수
            double ChX, ChY; //이동된 점 저장 변수
            double mov = 1.625; //이동할 양 
            List<double> tmphdg = new List<double>();


            foreach (SHPReader.SHPData list in links)
            {
                movPoints = new List<Point>();

                ptData[idx, 0] = list.NumPoints;
                ptData[idx, 1] = Convert.ToDouble(list.DBFData["Length"]);

                hdgList2D[idx] = new double[list.NumPoints];
                Length2D[idx] = new double[list.NumPoints];
                double tmp = Convert.ToDouble(list.DBFData["Length"]);

                for (int i = 0; i < list.NumPoints; i++)
                {
                    if (i != list.NumPoints - 1)
                    {
                        AngleX = list.APoints[i + 1].X - list.APoints[i].X;
                        AngleY = list.APoints[i + 1].Y - list.APoints[i].Y;
                        Odegree = Math.Atan2(AngleY, AngleX);
                        Cdegree = Odegree + Math.PI / 2;
                    }
                    else
                    {
                        Odegree = Math.Atan2(AngleY, AngleX);
                        Cdegree = Odegree + Math.PI / 2;
                    }

                    Length2D[idx][i] = Math.Sqrt(Math.Pow(AngleX, 2) + Math.Pow(AngleY, 2));
                    if (tmp > Length2D[idx][i])
                    {
                        tmp -= Length2D[idx][i];
                    }

                    Length2D[idx][list.NumPoints - 1] = tmp;

                    ChX = mov * Math.Cos(Cdegree) + list.APoints[i].X;
                    ChY = mov * Math.Sin(Cdegree) + list.APoints[i].Y;

                    Point pt = new Point
                    {
                        X = ChX,
                        Y = ChY,
                        M = list.APoints[i].M,
                        Z = list.APoints[i].Z
                    };
                    tmphdg.Add(Odegree);
                    hdgList2D[idx][i] = Odegree;

                    movPoints.Add(pt);
                }
                movPoint2D.Add(movPoints);

                lnkID[idx] = list.DBFData["ID"].ToString();
                hdgList = new double[tmphdg.Count];
                tmphdg.CopyTo(hdgList);

                idx++;
            }

            movPoints = new List<Point>();
            for (int i = 0; i < lnkID.Length; i++)
            {
                foreach (Point pts in movPoint2D[i])
                {
                    movPoints.Add(pts);
                }
            }
        }

        public void SubsGeo(SHPReader.Box mainBox)//Carla에 적용하기위해 좌표를 조정하기 위한 메소드
        {
            for (int i = 0; i < movPoints.Count; i++)
            {
                double X = movPoints[i].X, Y = movPoints[i].Y;
                X -= mainBox.Xmin;
                Y -= mainBox.Ymin;
                Point changedPt = new Point { X = X, Y = Y };
                movPoints[i] = changedPt;
            }

            for(int i =0; i<movPoint2D.Count;i++)
            {
                for(int j =0; j<movPoint2D[i].Count;j++)
                {
                    double X = movPoint2D[i][j].X, Y = movPoint2D[i][j].Y;
                    X -= mainBox.Xmin;
                    Y -= mainBox.Ymin;
                    Point changedPt = new Point { X = X, Y = Y };
                    movPoint2D[i][j] = changedPt;
                }
            }
        }
    }
}
