using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;

namespace ShapeToXodr
{
    class RefLineCreator
    {
        private string Root_Node;
        /* 노드리스트
         * "A119BS030836"
         * "A119BS030250"
         * "A119BS032005"
         * "A119BS032064"
         * "A119BS030160"
         * "A119BS030042"
         * "A119BS030011"
         * "A119BS030147"
         * "A119BS030904"
         * "A119BS030779"
         * "A119BS031453"
         * "A119BS031913"
         * "A119BS030525"
         * "A119BS030736"
         * "A119BS031293"
         * "A119BS032015"
         * "A119BS031673"
         * "A119BS031573"
         * "A119BS032048"
         * "A119BS032056"
         * "A119BS030827"
         * "A119BS031948"
         * "A119BS030027"
         * "A119BS030093"
         * "A119BS031634"
         * "A119BS031243"
         * "A119BS030249"
        */
        private string[] R_NodeList = { "A119BS030904", "A119BS030836", "A119BS032005", "A119BS032064", "A119BS030042", "A119BS030147", "A119BS031573", "A119BS030827", "A119BS031948", "A119BS031701", "A119BS030093", "A119BS030250", "A119BS030736", "A119BS030160", "A119BS030011", "A119BS032056", "A119BS030779", "A119BS031453", "A119BS031913", "A119BS030525", "A119BS031293", "A119BS032015", "A119BS031673", "A119BS032048", "A119BS030027", "A119BS031634", "A119BS031457", "A119BS031243", "A119BS030249" };
        //우측 기준 노드리스트
        private string[] L_NodeList; //{ "A119BS030723", "A119BS031861", "A119BS030889", "A119BS030805", "A119BS031145", "A119BS031125", "A119BS031027", "A119BS030008", "A119BS031579", "A119BS030902", "A119BS030602", "A119BS030563", "A119BS030720", "A119BS031973", "A119BS031925", "A119BS030094", "A119BS030088", "A119BS030253", "A119BS030655", "A119BS030239", "A119BS030164", "A119BS030559", "A119BS031337", "A119BS031318", "A119BS031282", "A119BS030728", "A119BS031568","A119BS031467" };
        //좌측 기준 노드리스트 예정
        public SHPReader.Single_Layer A1_NODE;
        public SHPReader.Single_Layer A2_LINK;
        private int A1DataLength = 0;
        private int A2DataLength = 0;
        private SHPReader.SHPData rootLink;
        public List<SHPReader.SHPData> RefLinks_R = new List<SHPReader.SHPData>();
        public int[] partPtCounter_R;//root노드 시작으로 끝나는 링크까지의 모든 점개수(전체 개수는 아님)
        public int[] linkPtCounter_R;//각 링크의 점개수 우측
        public int[] rootToEnd_R;//각 노드의 링크 인덱스 우측

        public List<SHPReader.SHPData> RefLinks_L = new List<SHPReader.SHPData>();
        public int[] partPtCounter_L;//root노드 시작으로 끝나는 링크까지의 모든 점개수(전체 개수는 아님)
        public int[] linkPtCounter_L;//각 링크의 점개수 좌측
        public int[] rootToEnd_L;//각 노드의 링크 인덱스 좌측

        public RefLineCreator(List<SHPReader.Single_Layer> laysers)
        {
            foreach (SHPReader.Single_Layer data in laysers)
            {
                if (data.LayerName == "A1_NODE")
                {
                    A1_NODE = data;
                }
                else if (data.LayerName == "A2_LINK")
                {
                    A2_LINK = data;
                }
            }

            if (A1_NODE.SHPData != null)
            {
                A1DataLength = A1_NODE.SHPData.Count();
            }

            if (A2_LINK.SHPData != null)
            {
                A2DataLength = A2_LINK.SHPData.Count();
            }

            if (R_NodeList == null)
            {
                FindRootLink();

                if (rootLink != null)
                {
                    RefLinks_R.Add(rootLink);
                    //rootLink를 찾았을때 할일
                    NextLink_R(rootLink);
                }
            }
            else
            {
                foreach (string lst in R_NodeList)
                {
                    Root_Node = lst;

                    FindRootLink();

                    RefLinks_R.Add(rootLink);

                    NextLink_R(rootLink);

                    RemoveExpList();

                    RefLinks_R.Add(null);
                }
            }

            RemoveExpList();
            DelNull(RefLinks_R);
        }

        public void LrefLineCreator()//왼쪽 referenceLine 기준 link를 찾는 메소드
        {
            string[] LNodeList = { "A119BS030723", "A119BS031861", "A119BS030889", "A119BS030805", "A119BS031145", "A119BS031125", "A119BS031027", "A119BS030008", "A119BS031579", "A119BS030902", "A119BS030602", "A119BS030563", "A119BS030720", "A119BS031973", "A119BS031925", "A119BS030094", "A119BS030088", "A119BS030253", "A119BS030655", "A119BS030239", "A119BS030164", "A119BS030559", "A119BS031337", "A119BS031318", "A119BS031282", "A119BS030728", "A119BS031568","A119BS031467" };
            foreach (string lst in LNodeList)
            {
                Root_Node = lst;

                FindRootLink();

                RefLinks_L.Add(rootLink);

                NextLink_L(rootLink);

                RemoveExpList();
            }
            DelNull(RefLinks_L, false);
        }

        private SHPReader.SHPData R_LinkData(string r_LinkID)//해당 link의 r_LINK 존재 여부를 찾는 메소드
        {
            SHPReader.SHPData R_LData;

            for (int i = 0; i < A2DataLength; i++)
            {
                if (Convert.ToString(A2_LINK.SHPData[i].DBFData["ID"]) == r_LinkID)
                {
                    R_LData = A2_LINK.SHPData[i];
                    return R_LData;
                }
            }
            return null;
        }

        private SHPReader.SHPData RootCheckSumLaneNo(List<SHPReader.SHPData> r_LinkData, List<SHPReader.SHPData> hookLink)//기준 Link의 LaneNo를 검증하는 메소드
        {
            SHPReader.SHPData rootLink;
            int index = hookLink.Count();

            if (index == 1)
            {
                rootLink = hookLink[index - 1];
                return rootLink;
            }

            for (int i = index - 1; i >= 0; i--)
            {
                if (r_LinkData[i] == null)
                {
                    continue;
                }
                if (Convert.ToDouble(r_LinkData[i].DBFData["LaneNo"]) == 2 || r_LinkData[i].DBFData["L_LinkID"] == null)
                {
                    rootLink = hookLink[i];
                    return rootLink;
                }
            }

            return null;
        }

        private List<SHPReader.SHPData> NextCheckSumLaneNo(List<SHPReader.SHPData> r_LinkData, List<SHPReader.SHPData> hookLink)//기준link의 다음 으로 연결된 link를 찾는 메소드 ( 재귀 )
        {
            List<SHPReader.SHPData> nextLink = new List<SHPReader.SHPData>();
            int index = hookLink.Count();

            if (index == 1)
            {
                nextLink.Add(hookLink[index - 1]);
                return nextLink;
            }

            for (int i = index - 1; i >= 0; i--)
            {
                if (r_LinkData[i] == null || Convert.ToDouble(r_LinkData[i].DBFData["LaneNo"]) == 2 || r_LinkData[i].DBFData["L_LinkID"] == null)
                {
                    nextLink.Add(hookLink[i]);
                }
                if (hookLink[i].DBFData["ID"].ToString() == "A219BS032033" || hookLink[i].DBFData["ID"].ToString() == "A219BS031577"
                    || hookLink[i].DBFData["ID"].ToString() == "A219BS031806" || hookLink[i].DBFData["ID"].ToString() == "A219BS031951"
                    || hookLink[i].DBFData["ID"].ToString() == "A219BS031301" || hookLink[i].DBFData["ID"].ToString() == "A219BS031258"
                    || hookLink[i].DBFData["ID"].ToString() == "A219BS031009" || hookLink[i].DBFData["ID"].ToString() == "A219BS030977"
                    || hookLink[i].DBFData["ID"].ToString() == "A219BS031307" || hookLink[i].DBFData["ID"].ToString() == "A219BS031913"
                    || hookLink[i].DBFData["ID"].ToString() == "A219BS031263") //수동 추가
                {
                    nextLink.Add(hookLink[i]);
                }
            }

            return nextLink;
        }

        private void FindRootLink()//노드리스트 부터 시작하는 link 를 찾는 메소드
        {
            object R_LinkID_tm;
            List<SHPReader.SHPData> hookLink = new List<SHPReader.SHPData>();
            List<SHPReader.SHPData> r_Links = new List<SHPReader.SHPData>();

            for (int i = 0; i < A2DataLength; i++)
            {
                if (A2_LINK.SHPData[i].DBFData["FromNodeID"].ToString() == Root_Node)
                {
                    hookLink.Add(A2_LINK.SHPData[i]);
                    R_LinkID_tm = A2_LINK.SHPData[i].DBFData["R_LinkID"];
                    if (R_LinkID_tm != null)
                    {
                        r_Links.Add(R_LinkData(R_LinkID_tm as string));
                    }
                    else
                    {
                        r_Links.Add(null);
                    }
                }
            }
            rootLink = RootCheckSumLaneNo(r_Links, hookLink);
        }

        private bool ExeptionAdd_R(object dbfNodeId, object ID) //링크 찾을때 예외 추가사항
        {
            if (dbfNodeId.ToString() == "A119BS030192" && ID.ToString() == "A219BS032033"
                || dbfNodeId.ToString() == "A119BS030898" && ID.ToString() == "A219BS031577"
                || dbfNodeId.ToString() == "A119BS030863" && ID.ToString() == "A219BS031949"
                || dbfNodeId.ToString() == "A119BS030826" && ID.ToString() == "A219BS031806"
                || dbfNodeId.ToString() == "A119BS031832" && ID.ToString() == "A219BS031143"
                || dbfNodeId.ToString() == "A119BS031837" && ID.ToString() == "A219BS031144"
                || dbfNodeId.ToString() == "A119BS031817" && ID.ToString() == "A219BS031769"
                || dbfNodeId.ToString() == "A119BS030857" && ID.ToString() == "A219BS031951"
                || dbfNodeId.ToString() == "A119BS031639" && ID.ToString() == "A219BS031301"
                || dbfNodeId.ToString() == "A119BS031586" && ID.ToString() == "A219BS031258"
                || dbfNodeId.ToString() == "A119BS031465" && ID.ToString() == "A219BS031009"
                || dbfNodeId.ToString() == "A119BS031435" && ID.ToString() == "A219BS030977"
                || dbfNodeId.ToString() == "A119BS031779" && ID.ToString() == "A219BS031307"
                || dbfNodeId.ToString() == "A119BS031544" && ID.ToString() == "A219BS031913"
                || dbfNodeId.ToString() == "A119BS031581" && ID.ToString() == "A219BS031263") // 추가할 링크의 toNodeID 와 ID를 넣음
            {
                return true;
            }
            return false;
        }

        private bool ExeptionEnd_R(object ID)//링크 검색 종료사항
        {
            if (ID.ToString() == "A219BS030419" || ID.ToString() == "A219BS030426"
                || ID.ToString() == "A219BS031468" || ID.ToString() == "A219BS030241"
                || ID.ToString() == "A219BS030394" || ID.ToString() == "A219BS030239"
                || ID.ToString() == "A219BS030290" || ID.ToString() == "A219BS031172")
            {
                return true;
            }
            return false;
        }


        private void NextLink_R(SHPReader.SHPData profile)//다음 link를 찾는 메소드 (우측)
        {
            object R_LinkID_tm;
            List<SHPReader.SHPData> NLinkT = new List<SHPReader.SHPData>();
            List<SHPReader.SHPData> r_Links = new List<SHPReader.SHPData>();
            SHPReader.SHPData nLink;
            List<SHPReader.SHPData> chkData = new List<SHPReader.SHPData>();

            if (profile == null)
            {
                return;
            }

            for (int i = 0; i < A2DataLength; i++)
            {
                if (ExeptionEnd_R(profile.DBFData["ID"]))//예외 처리 - 종료
                {
                    continue;
                }

                if ((A2_LINK.SHPData[i].DBFData["FromNodeID"].ToString() == profile.DBFData["ToNodeId"].ToString() && Convert.ToDouble(A2_LINK.SHPData[i].DBFData["LaneNo"]) == 1)
                    || ExeptionAdd_R(profile.DBFData["ToNodeId"], A2_LINK.SHPData[i].DBFData["ID"]))//예외 추가

                {
                    if (A2_LINK.SHPData[i].DBFData["ID"].ToString() == "A219BS031127" || A2_LINK.SHPData[i].DBFData["ID"].ToString() == "A219BS031129")//예외 스킵
                    {
                        continue;
                    }

                    NLinkT.Add(A2_LINK.SHPData[i]);
                    R_LinkID_tm = A2_LINK.SHPData[i].DBFData["R_LinkID"];
                    if (R_LinkID_tm != null)
                    {
                        r_Links.Add(R_LinkData(R_LinkID_tm as string));
                    }
                    else
                    {
                        r_Links.Add(null);
                    }
                }
            }

            chkData = NextCheckSumLaneNo(r_Links, NLinkT);

            foreach (SHPReader.SHPData chk in chkData)
            {
                nLink = chk;

                foreach (SHPReader.SHPData sH in RefLinks_R) // 데이터 중복 및 EOL 검사
                {
                    if (nLink == sH || nLink == null)
                    {
                        return;
                    }
                }

                RefLinks_R.Add(nLink);
                NextLink_R(nLink);
            }
        }

        private void NextLink_L(SHPReader.SHPData profile)//다음 link를 찾는 메소드 (좌측)
        {
            object R_LinkID_tm;
            List<SHPReader.SHPData> NLinkT = new List<SHPReader.SHPData>();
            List<SHPReader.SHPData> r_Links = new List<SHPReader.SHPData>();
            SHPReader.SHPData nLink;
            List<SHPReader.SHPData> chkData = new List<SHPReader.SHPData>();

            if (profile == null)
            {
                return;
            }

            for (int i = 0; i < A2DataLength; i++)
            {
                if (profile.DBFData["ID"].ToString() == "A219BS030419" || profile.DBFData["ID"].ToString() == "A219BS030426"
                    || profile.DBFData["ID"].ToString() == "A219BS031468" || profile.DBFData["ID"].ToString() == "A219BS030241"
                    || profile.DBFData["ID"].ToString() == "A219BS030394" || profile.DBFData["ID"].ToString() == "A219BS030239"
                    || profile.DBFData["ID"].ToString() == "A219BS030290")//예외 처리 - 종료
                {
                    continue;
                }

                if ((A2_LINK.SHPData[i].DBFData["FromNodeID"].ToString() == profile.DBFData["ToNodeId"].ToString() && Convert.ToDouble(A2_LINK.SHPData[i].DBFData["LaneNo"]) == 1))//예외 추가

                {
                    if (A2_LINK.SHPData[i].DBFData["ID"].ToString() == "A219BS031127" || A2_LINK.SHPData[i].DBFData["ID"].ToString() == "A219BS031129")//예외 스킵
                    {
                        continue;
                    }

                    NLinkT.Add(A2_LINK.SHPData[i]);
                    R_LinkID_tm = A2_LINK.SHPData[i].DBFData["R_LinkID"];
                    if (R_LinkID_tm != null)
                    {
                        r_Links.Add(R_LinkData(R_LinkID_tm as string));
                    }
                    else
                    {
                        r_Links.Add(null);
                    }
                }
            }

            chkData = NextCheckSumLaneNo(r_Links, NLinkT);

            foreach (SHPReader.SHPData chk in chkData)
            {
                nLink = chk;

                foreach (SHPReader.SHPData sH in RefLinks_R) // 데이터 중복 및 EOL 검사
                {
                    if (nLink == sH || nLink == null)
                    {
                        return;
                    }
                }

                RefLinks_L.Add(nLink);
                NextLink_L(nLink);
            }
        }

        private void RemoveExpList()//예외처리 중 삭제 항목
        {
            string[] excepionlist = { "A219BS031569", "A219BS031581", "A219BS031477", "A219BS031468", "A219BS031649", "A219BS031458", "A219BS031452", "A219BS030531", "A219BS031948", "A219BS031267", "A219BS031861", "A219BS031127", "A219BS031129", "A219BS031127", "A219BS031129", "A219BS031486", "A219BS031129", "A219BS031873", "A219BS031922", "A219BS031008", "A219BS030976", "A219BS030241", "A21BS031973", "A219BS031757", "A219BS031973", "A219BS031172", "A219BS031273", "A219BS031314" };

            for (int i = 0; i < RefLinks_R.Count() - 1; i++)
            {
                foreach (string list in excepionlist)
                {
                    if (RefLinks_R[i] != null && RefLinks_R[i].DBFData["ID"].ToString() == list)
                    {
                        RefLinks_R.RemoveAt(i);
                    }

                }
            }
            for (int i = 0; i < RefLinks_L.Count() - 1; i++)
            {
                foreach (string list in excepionlist)
                {
                    if (RefLinks_L[i] != null && RefLinks_L[i].DBFData["ID"].ToString() == list)
                    {
                        RefLinks_L.RemoveAt(i);
                    }

                }
            }

        }

        private void DelNull(List<SHPReader.SHPData> refDatas, bool isR = true)//새로운 Link의 시작마다 추가된 null 을 제거하고, 정리하는 메소드
        {
            List<int> brokelist = new List<int>();

            for (int i = 0; i < refDatas.Count(); i++)
            {
                if (refDatas[i] == null && i != refDatas.Count() - 1)
                {
                    brokelist.Add(i);
                }
            }
            if (isR)
            {
                brokelist.Add(refDatas.Count() - 1);
                partPtCounter_R = new int[brokelist.Count()];
                brokelist.CopyTo(partPtCounter_R);
                rootToEnd_R = new int[partPtCounter_R.Length];
                List<int> tmrTE = new List<int>();
                int k = 0;

                for (int i = 0; i < refDatas.Count(); i++)
                {
                    if (refDatas[i] == null)
                    {
                        tmrTE.Add(i - k);
                        refDatas.RemoveAt(i);
                        k = i;
                    }
                }

                tmrTE.CopyTo(rootToEnd_R);

                for (int i = 0; i < partPtCounter_R.Length; i++)
                {
                    partPtCounter_R[i] -= i;
                }

                brokelist.Clear();
                linkPtCounter_R = new int[refDatas.Count()];

                foreach (SHPReader.SHPData data in refDatas)
                {
                    brokelist.Add(data.NumPoints);
                }

                brokelist.CopyTo(linkPtCounter_R);
            }
            else
            {
                brokelist.Add(refDatas.Count() - 1);
                partPtCounter_L = new int[brokelist.Count()];
                brokelist.CopyTo(partPtCounter_L);
                rootToEnd_L = new int[partPtCounter_L.Length];
                List<int> tmrTE = new List<int>();
                int k = 0;

                for (int i = 0; i < refDatas.Count(); i++)
                {
                    if (refDatas[i] == null)
                    {
                        tmrTE.Add(i - k);
                        refDatas.RemoveAt(i);
                        k = i;
                    }
                }

                tmrTE.CopyTo(rootToEnd_L);

                for (int i = 0; i < partPtCounter_L.Length; i++)
                {
                    partPtCounter_L[i] -= i;
                }

                brokelist.Clear();
                linkPtCounter_L = new int[refDatas.Count()];

                foreach (SHPReader.SHPData data in refDatas)
                {
                    brokelist.Add(data.NumPoints);
                }

                brokelist.CopyTo(linkPtCounter_L);
            }
        }
    }
}
