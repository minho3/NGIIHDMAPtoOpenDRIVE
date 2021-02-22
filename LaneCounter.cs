using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeToXodr
{
    class LaneCounter
    {
        public int[][] lanesCounter;//1열 우측 차선 개수 2열 좌측 차선 개수
        List<SHPReader.SHPData> A2LinkData; // A2의 모든 데이터
        private bool flag = false;//재귀함수 탈출을 위한 변수

        public LaneCounter(SHPReader.Single_Layer A2Link, string[] LinkID)
        {
            A2LinkData = A2Link.SHPData;
            List<SHPReader.SHPData> root = new List<SHPReader.SHPData>();
            lanesCounter = new int[LinkID.Length][];
            int i = 0, cntr = 1;

            for (i = 0; i < LinkID.Length; i++)
            {
                foreach (SHPReader.SHPData Data in A2LinkData)
                {
                    if (Data.DBFData["ID"].ToString() == LinkID[i])
                    {
                        root.Add(Data);
                        break;
                    }
                }
                lanesCounter[i] = new int[2];
            }

            i = 0;
            foreach (SHPReader.SHPData data in root)
            {
                if (data.DBFData["R_LinkID"] == null)
                {
                    lanesCounter[i][0] = 1;
                }
                else
                {
                    lanesCounter[i][0] = RL_LanesCounter(cntr, data.DBFData["R_LinkID"].ToString());
                }
                cntr = 0;
                flag = false;

                if (data.DBFData["L_LinkID"] == null)
                {
                    lanesCounter[i][1] = 0;
                }
                else
                {
                    lanesCounter[i][1] = RL_LanesCounter(cntr, data.DBFData["L_LinkID"].ToString(), false);
                }

                cntr = 1;
                flag = false;
                i++;
            }
        }

        private int RL_LanesCounter(int ctr, string hookID, bool isR = true)
        {

            SHPReader.SHPData hookData = new SHPReader.SHPData();
            if (!flag)
            {
                foreach (SHPReader.SHPData data in A2LinkData)
                {
                    if (data.DBFData["ID"].ToString() == hookID)
                    {
                        hookData = data;
                        ctr++;
                        break;
                    }
                }

                if (isR && (hookData.DBFData["R_LinkID"] == null))
                {
                    return ctr;
                }
                else if (isR && hookData.DBFData["LaneNo"].ToString() == 1.ToString())
                {
                    ctr--;
                    flag = true;
                    return ctr;
                }
                else if (isR)
                {
                    ctr = RL_LanesCounter(ctr, hookData.DBFData["R_LinkID"].ToString(), isR);
                }

                if (!isR && hookData.DBFData["L_LinkID"] == null)
                {
                    return ctr;
                }
                else if (!isR)
                {
                    ctr = RL_LanesCounter(ctr, hookData.DBFData["L_LinkID"].ToString(), isR);
                }
            }
            return ctr;
        }

        public void FixExtraLane(int[] rootToEnd)
        {
            int k = 0;
            for (int i = 0; i < rootToEnd.Length; i++)
            {
                for (int cntr = 0; cntr < rootToEnd[i]; cntr++)
                {
                    if (rootToEnd[i] == 1)//1일때 스킵
                    {
                        k++;
                        break;
                    }

                    if (cntr == 0)//시작점
                    {
                        CmpLanes(ref lanesCounter[k][0], ref lanesCounter[k + 1][0], lanesCounter[k + 2][0]);
                    }
                    else if (cntr == rootToEnd[i] - 1)//끝점
                    {
                        CmpLanes(ref lanesCounter[k][0], ref lanesCounter[k - 1][0], lanesCounter[k - 2][0]);
                    }
                    else
                    {
                        CmpLanes(ref lanesCounter[k][0], lanesCounter[k + 1][0], lanesCounter[k - 1][0]);
                    }
                    k++;
                }
            }
            //잘못 수정 또는 변경 적용 부분
            lanesCounter[14][0] = lanesCounter[19][0] = lanesCounter[20][0] = lanesCounter[62][0] = 1;
            lanesCounter[103][1] = lanesCounter[303][1] = lanesCounter[1][1] = 1;
            lanesCounter[136][1] = lanesCounter[328][1] = lanesCounter[329][1] = 0;

        }

        private void CmpLanes(ref int index, ref int cmpobj1, int cmpobj2)//처음, 마지막 비교 부분
        {
            if (Math.Abs(cmpobj1 - index) > 2)
            {
                if (cmpobj1 == cmpobj2)
                {
                    index = cmpobj1;
                }
                else if (index == cmpobj2)
                {
                    cmpobj1 = index;
                }
            }
        }

        private void CmpLanes(ref int index, int cmpnextobj, int cmppreobj)//처음 마지막 제외 모든 부분 비교
        {
            if (cmpnextobj == cmppreobj && cmpnextobj != index)
            {
                index = cmpnextobj;
            }
        }
    }
}
