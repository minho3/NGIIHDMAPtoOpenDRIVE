using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;

namespace ShapeToXodr
{
    class toXODR
    {
        private const double Road_Width = 3.25;
        private const double RoadMark_Width = 0.125;
        private const string Es = "e16";

        private XmlDocument Export = new XmlDocument();
        private XmlNode root; //lv0 OpenDrive
        //============================= lv0 ======================
        private XmlNode header;
        private XmlCDataSection Cdata;
        private List<XmlNode> Road = new List<XmlNode>();
        //============================= lv1 ======================
        private XmlNode GeoReference;
        private XmlNode Lanes;
        //============================= lv2 ======================
        private XmlNode laneSection;
        //============================= lv3 ======================
        private XmlAttribute headerattr;

        public enum Entry_PS
        {
            all,
            predecessor,
            successor,
            none
        }

        private enum LaneStat
        {
            increase,
            decrease,
            split,
            nothing
        }

        private XmlWriterSettings setting = new XmlWriterSettings();

        public toXODR()
        {
            XmlAttribute attr = Export.CreateAttribute("length");
            setting.Indent = true;

            root = Export.CreateElement("OpenDRIVE");
            header = Export.CreateElement("header");
            Cdata = Export.CreateCDataSection("+proj=tmerc +lat_0=38 +lon_0=127.5 +k=0.9996 +x_0=1000000 +y_0=2000000 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs");
            GeoReference = Export.CreateElement("geoReference");

            Road.Add(Export.CreateElement("road"));
            Lanes = Export.CreateElement("lanes");
            laneSection = Export.CreateElement("laneSection");

            Export.AppendChild(root);
            root.AppendChild(header);

            CreateHeaderattr();
        }

        private void CreateHeaderattr()
        {
            headerattr = Export.CreateAttribute("revMajor");
            headerattr.Value = 1.ToString();
            header.Attributes.Append(headerattr);

            headerattr = Export.CreateAttribute("revMinor");
            headerattr.Value = 5.ToString();
            header.Attributes.Append(headerattr);

            header.AppendChild(GeoReference);
            GeoReference.AppendChild(Cdata);
        }

        private void Make_Attribute(XmlNode node, string name, string value = "")
        {
            XmlAttribute attr = Export.CreateAttribute(name);
            attr.Value = value;

            node.Attributes.Append(attr);
        }

        private void Make_Attribute(XmlNode node, string[] name, string[] value = null)
        {
            XmlAttribute attr = null;

            for (int i = 0; i < name.Length; i++)
            {
                attr = Export.CreateAttribute(name[i]);
                if (value[i] != null)
                {
                    attr.Value = value[i];
                }
                else
                {
                    attr.Value = null;
                }
                node.Attributes.Append(attr);
            }
        }

        private XmlNode CreateGeometry(double[] hdg, List<SHPReader.Point> pts, double[] length)
        {
            XmlNode planView = Export.CreateElement("planView");
            XmlNode[] geometrys = new XmlNode[hdg.Length];
            XmlNode[] line = new XmlNode[hdg.Length];

            string[] names = { "s", "x", "y", "hdg", "length" };
            string[] vals;

            double stptr = 0;

            for (int i = 0; i < hdg.Length - 1; i++)
            {
                geometrys[i] = Export.CreateElement("geometry");
                line[i] = Export.CreateElement("line");
            }

            for (int i = 0; i < hdg.Length - 1; i++)
            {
                planView.AppendChild(geometrys[i]);
                if (i == 0)
                {
                    vals = new string[] { stptr.ToString(Es), pts[i].X.ToString(Es), pts[i].Y.ToString(Es), hdg[i].ToString(Es), length[i].ToString(Es) };
                }
                else
                {
                    vals = new string[] { stptr.ToString(Es), pts[i].X.ToString(Es), pts[i].Y.ToString(Es), hdg[i].ToString(Es), length[i].ToString(Es) };
                }
                stptr += length[i];
                Make_Attribute(geometrys[i], names, vals);
                geometrys[i].AppendChild(line[i]);
            }

            return planView;
        }

        public void RefData(double[] hdg, List<SHPReader.Point> pts, double[] s_Length, string LinkID, double t_Length)
        {
            root.AppendChild(AddRoadElement(LinkID, t_Length));
            XmlNode lastCH = root.LastChild;
            lastCH.AppendChild(CreateGeometry(hdg, pts, s_Length));
        }

        private XmlNode AddRoadElement(string LinkID, double t_Length)
        {
            XmlNode road = Export.CreateElement("road");

            string[] roadattr = { "name", "length", "id", "junction" };
            string[] roadval = { null, t_Length.ToString(Es), LinkID.Substring(6, 6), "-1" };

            Make_Attribute(road, roadattr, roadval);

            return road;
        }

        private XmlNode Make_Road_Link(string prId, string suId, Entry_PS at_type = Entry_PS.all, string cPoint = "end", string type = "road")
        {
            XmlNode link = Export.CreateElement("link");
            XmlNode predecessor;
            XmlNode successor;
            string[] attrs = { "elementType", "elementId", "contactPoint" };
            string[] attrval = { type, prId, cPoint };

            switch (at_type)
            {
                case Entry_PS.all:
                    predecessor = Export.CreateElement(Entry_PS.predecessor.ToString());
                    Make_Attribute(predecessor, attrs, attrval);

                    successor = Export.CreateElement(Entry_PS.successor.ToString());
                    attrval[1] = suId;
                    attrval[2] = "start";
                    Make_Attribute(successor, attrs, attrval);

                    link.AppendChild(predecessor);
                    link.AppendChild(successor);

                    break;
                case Entry_PS.predecessor:
                    predecessor = Export.CreateElement(Entry_PS.predecessor.ToString());
                    Make_Attribute(predecessor, attrs, attrval);

                    link.AppendChild(predecessor);
                    break;
                case Entry_PS.successor:
                    successor = Export.CreateElement(Entry_PS.successor.ToString());
                    attrval[1] = suId;
                    attrval[2] = "start";
                    Make_Attribute(successor, attrs, attrval);

                    link.AppendChild(successor);
                    break;
            }

            return link;
        }

        public void Make_Road_LinkData(int[] rootToEnd_S, string[] linkID)
        {
            int k = 0;
            XmlNode link;
            for (int i = 0; i < rootToEnd_S.Length; i++)
            {
                for (int cntr = 0; cntr < rootToEnd_S[i]; cntr++)
                {
                    if (rootToEnd_S[i] == 1)
                    {
                        k++;
                        break;
                    }

                    if (cntr == 0)
                    {
                        link = Make_Road_Link(null, linkID[k + 1].Substring(6, 6), Entry_PS.successor);
                    }
                    else if (cntr == rootToEnd_S[i] - 1)
                    {
                        link = Make_Road_Link(linkID[k - 1].Substring(6, 6), null, Entry_PS.predecessor);
                    }
                    else
                    {
                        link = Make_Road_Link(linkID[k - 1].Substring(6, 6), linkID[k + 1].Substring(6, 6));
                    }
                    root.ChildNodes[k + 1].PrependChild(link);
                    k++;
                }
            }
        }

        private XmlNode Make_Lane_LinkData(int laneid, Entry_PS pS = Entry_PS.all)//laneid Entery 만 받고 Link생성
        {
            XmlNode link = Export.CreateElement("link");
            XmlNode predecessorChild;
            XmlNode successorChild;
            switch (pS)
            {
                case Entry_PS.predecessor:
                    link.AppendChild(Export.CreateElement("predecessor"));

                    predecessorChild = link.LastChild;
                    Make_Attribute(predecessorChild, "id", laneid.ToString());

                    break;
                case Entry_PS.successor:
                    link.AppendChild(Export.CreateElement("successor"));

                    successorChild = link.LastChild;
                    Make_Attribute(successorChild, "id", laneid.ToString());
                    break;
                default:
                    link.AppendChild(Export.CreateElement("successor"));

                    successorChild = link.LastChild;
                    Make_Attribute(successorChild, "id", laneid.ToString());

                    link.AppendChild(Export.CreateElement("predecessor"));

                    predecessorChild = link.LastChild;
                    Make_Attribute(predecessorChild, "id", laneid.ToString());

                    break;
            }
            return link;
        }

        private XmlNode Make_Lane_LinkData(int laneid, int nextlanesize, LaneStat[] laneStat, Entry_PS pS)
        {
            XmlNode link = Export.CreateElement("link");
            XmlNode predecessorChild;
            XmlNode successorChild;

            if (pS == Entry_PS.all)
            {
                for (int i = 0; i < laneStat.Length; i++)
                {
                    switch (laneStat[i])
                    {
                        case LaneStat.increase:
                            if (i == 0)
                            {
                                link.AppendChild(Export.CreateElement("predecessor"));

                                predecessorChild = link.LastChild;
                                Make_Attribute(predecessorChild, "id", laneid.ToString());

                                link.AppendChild(Export.CreateElement("predecessor"));

                                predecessorChild = link.LastChild;
                                Make_Attribute(predecessorChild, "id", (laneid - 1).ToString());
                            }
                            else
                            {
                                link.AppendChild(Export.CreateElement("successor"));

                                successorChild = link.LastChild;
                                Make_Attribute(successorChild, "id", laneid.ToString());

                                link.AppendChild(Export.CreateElement("successor"));

                                successorChild = link.LastChild;
                                Make_Attribute(successorChild, "id", (laneid - 1).ToString());
                            }
                            break;
                        case LaneStat.decrease:
                            if (i == 0)
                            {
                                link.AppendChild(Export.CreateElement("predecessor"));

                                predecessorChild = link.LastChild;
                                Make_Attribute(predecessorChild, "id", (laneid + 1).ToString());
                            }
                            else
                            {
                                if (laneid != -1)
                                {
                                    link.AppendChild(Export.CreateElement("successor"));

                                    successorChild = link.LastChild;
                                    Make_Attribute(successorChild, "id", (laneid + 1).ToString());
                                }
                                else
                                {
                                    link.AppendChild(Export.CreateElement("successor"));

                                    successorChild = link.LastChild;
                                    Make_Attribute(successorChild, "id", laneid.ToString());
                                }
                            }
                            break;
                        case LaneStat.split:
                        default:
                            if (i == 0)
                            {
                                link.AppendChild(Export.CreateElement("predecessor"));

                                predecessorChild = link.LastChild;
                                Make_Attribute(predecessorChild, "id", laneid.ToString());
                            }
                            else
                            {
                                link.AppendChild(Export.CreateElement("successor"));

                                successorChild = link.LastChild;
                                Make_Attribute(successorChild, "id", laneid.ToString());
                            }
                            break;
                    }
                }
            }
            else if (pS == Entry_PS.predecessor)
            {
                switch (laneStat[0])
                {
                    case LaneStat.increase:
                        link.AppendChild(Export.CreateElement("predecessor"));

                        predecessorChild = link.LastChild;
                        Make_Attribute(predecessorChild, "id", laneid.ToString());
                        break;
                    case LaneStat.decrease:
                        link.AppendChild(Export.CreateElement("predecessor"));

                        predecessorChild = link.LastChild;
                        Make_Attribute(predecessorChild, "id", (laneid + 1).ToString());

                        link.AppendChild(Export.CreateElement("predecessor"));

                        predecessorChild = link.LastChild;
                        Make_Attribute(predecessorChild, "id", laneid.ToString());
                        break;
                    case LaneStat.split:
                        link.AppendChild(Export.CreateElement("predecessor"));

                        predecessorChild = link.LastChild;
                        Make_Attribute(predecessorChild, "id", laneid.ToString());
                        break;
                    case LaneStat.nothing:
                    default:
                        link.AppendChild(Export.CreateElement("predecessor"));

                        predecessorChild = link.LastChild;
                        Make_Attribute(predecessorChild, "id", laneid.ToString());
                        break;
                }

            }
            else //Entry_PS.successor
            {
                switch (laneStat[1])
                {
                    case LaneStat.increase:
                        link.AppendChild(Export.CreateElement("successor"));

                        successorChild = link.LastChild;
                        Make_Attribute(successorChild, "id", laneid.ToString());

                        link.AppendChild(Export.CreateElement("successor"));

                        successorChild = link.LastChild;
                        Make_Attribute(successorChild, "id", (laneid - 1).ToString());
                        break;
                    case LaneStat.decrease:
                        if (laneid != -1)
                        {
                            link.AppendChild(Export.CreateElement("successor"));

                            successorChild = link.LastChild;
                            Make_Attribute(successorChild, "id", (laneid + 1).ToString());
                        }
                        else
                        {
                            link.AppendChild(Export.CreateElement("successor"));

                            successorChild = link.LastChild;
                            Make_Attribute(successorChild, "id", laneid.ToString());
                        }
                        break;
                    case LaneStat.split:
                        link.AppendChild(Export.CreateElement("successor"));

                        successorChild = link.LastChild;
                        Make_Attribute(successorChild, "id", laneid.ToString());
                        break;
                    case LaneStat.nothing:
                    default:
                        link.AppendChild(Export.CreateElement("successor"));

                        successorChild = link.LastChild;
                        Make_Attribute(successorChild, "id", laneid.ToString());
                        break;
                }
            }

            return link;
        }

        private LaneStat[] ChkLane(Entry_PS pS, int prvLane, int idx, int nexLane)
        {
            LaneStat[] rData = new LaneStat[2];

            switch (pS)
            {
                case Entry_PS.predecessor:
                    if (Math.Abs(idx - prvLane) >= 1)
                    {
                        if (idx - prvLane == -1)
                        {
                            rData[0] = LaneStat.decrease;
                        }
                        else if (idx - prvLane == 1)
                        {
                            rData[0] = LaneStat.increase;
                        }
                        else
                        {
                            rData[0] = LaneStat.split;
                        }
                    }
                    else
                    {
                        rData[0] = LaneStat.nothing;
                    }
                    break;
                case Entry_PS.successor:
                    if (Math.Abs(idx - nexLane) >= 1)
                    {
                        if (idx - nexLane == -1)
                        {
                            rData[1] = LaneStat.increase;
                        }
                        else if (idx - nexLane == 1)
                        {
                            rData[1] = LaneStat.decrease;
                        }
                        else
                        {
                            rData[1] = LaneStat.split;
                        }
                    }
                    else
                    {
                        rData[1] = LaneStat.nothing;
                    }
                    break;
                case Entry_PS.all:
                    int[] tmp = { prvLane, nexLane };

                    for (int i = 0; i < 2; i++)
                    {
                        if (Math.Abs(idx - tmp[i]) >= 1)
                        {
                            if (idx - tmp[i] == -1)
                            {
                                rData[i] = LaneStat.increase;
                            }
                            else if (idx - tmp[i] == 1)
                            {
                                rData[i] = LaneStat.decrease;
                            }
                            else
                            {
                                rData[i] = LaneStat.split;
                            }
                        }
                        else
                        {
                            rData[i] = LaneStat.nothing;
                        }
                    }
                    break;
            }
            return rData;
        }

        private void Make_LaneOffset(XmlNode lanesNode, int prv, int index)
        {
            string[] lOffsetattr = { "s", "a", "b", "c", "d" };
            string[] lOffsetval_St = { 0.0.ToString(Es), 0.0.ToString(Es), 1.ToString(Es), 0.0.ToString(Es), 0.0.ToString(Es) };
            string[] lOffsetval_Se = { Road_Width.ToString(Es), Road_Width.ToString(Es), 0.0.ToString(Es), 0.0.ToString(Es), 0.0.ToString(Es) };
            XmlNode offset_Data;
            if (prv != 0)
            {
                if (index == prv)
                {
                    lanesNode.AppendChild(Export.CreateElement("laneOffset"));

                    offset_Data = lanesNode.LastChild;

                    lOffsetval_St[1] = Road_Width.ToString(Es);
                    lOffsetval_St[2] = 0.0.ToString(Es);
                    Make_Attribute(offset_Data, lOffsetattr, lOffsetval_St);

                    lanesNode.AppendChild(Export.CreateElement("laneOffset"));
                    offset_Data = lanesNode.LastChild;

                    Make_Attribute(offset_Data, lOffsetattr, lOffsetval_Se);
                }
                else
                {
                    lanesNode.AppendChild(Export.CreateElement("laneOffset"));

                    offset_Data = lanesNode.LastChild;

                    lOffsetval_St[1] = Road_Width.ToString(Es);
                    Make_Attribute(offset_Data, lOffsetattr, lOffsetval_St);

                    lanesNode.AppendChild(Export.CreateElement("laneOffset"));
                    offset_Data = lanesNode.LastChild;

                    lOffsetval_Se[1] = (Road_Width * prv).ToString(Es);
                    Make_Attribute(offset_Data, lOffsetattr, lOffsetval_Se);
                }
            }
            else
            {
                lanesNode.AppendChild(Export.CreateElement("laneOffset"));

                offset_Data = lanesNode.LastChild;

                Make_Attribute(offset_Data, lOffsetattr, lOffsetval_St);

                lanesNode.AppendChild(Export.CreateElement("laneOffset"));
                offset_Data = lanesNode.LastChild;

                Make_Attribute(offset_Data, lOffsetattr, lOffsetval_Se);
            }
        }

        private void Fix_Poket_Lane(int index, XmlNode rightNode, int[][] lanedata)
        {
            XmlNode roadLink = ((rightNode.ParentNode).ParentNode).ParentNode; // > road
            XmlNode tmNode;
            List<XmlNode> laneLink = GetLaneLink(rightNode);
            if (index == 2)
            {

            }
            for (int i = 0; i < roadLink.ChildNodes.Count; i++)
            {
                if (roadLink.ChildNodes[i].Name == "link")
                {
                    roadLink = roadLink.ChildNodes[i];
                    break;
                }
            }
            Entry_PS entry = ChkEntry(roadLink);

            if (entry == Entry_PS.predecessor)
            {
                if (lanedata[index][1] != lanedata[index - 1][1] && lanedata[index][1] > lanedata[index - 1][1])
                {
                    for (int i = 0; i < laneLink.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        else
                        {
                            for (int k = 0; k < laneLink[i].ChildNodes.Count; k++)
                            {
                                tmNode = laneLink[i].ChildNodes[k];
                                tmNode.Attributes.GetNamedItem("id").Value = (Convert.ToInt32(tmNode.Attributes.GetNamedItem("id").Value) - 1).ToString();
                            }
                        }
                    }
                }
            }
            else if (entry == Entry_PS.successor)
            {
                if (lanedata[index][1] != lanedata[index + 1][1] && lanedata[index][1] < lanedata[index + 1][1])
                {
                    for (int i = 0; i < laneLink.Count; i++)
                    {
                        if (i == 0)
                        {
                            laneLink[i].AppendChild(Export.CreateElement("successor"));
                            Make_Attribute(laneLink[i].LastChild, "id", (-2).ToString());
                        }
                        else
                        {
                            for (int k = 0; k < laneLink[i].ChildNodes.Count; k++)
                            {
                                tmNode = laneLink[i].ChildNodes[k];
                                tmNode.Attributes.GetNamedItem("id").Value = (Convert.ToInt32(tmNode.Attributes.GetNamedItem("id").Value) - 1).ToString();
                            }
                        }
                    }
                }
            }
            else if (entry == Entry_PS.all)
            {
                if (lanedata[index][1] != lanedata[index - 1][1] && lanedata[index][1] > lanedata[index - 1][1])
                {
                    for (int i = 0; i < laneLink.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        else
                        {
                            for (int k = 0; k < laneLink[i].ChildNodes.Count; k++)
                            {
                                if (laneLink[i].ChildNodes[k].Name == "predecessor")
                                {
                                    tmNode = laneLink[i].ChildNodes[k];
                                }
                                else
                                {
                                    continue;
                                }
                                if (tmNode.Attributes.GetNamedItem("id").Value != "-1".ToString())
                                {
                                    tmNode.Attributes.GetNamedItem("id").Value = (Convert.ToInt32(tmNode.Attributes.GetNamedItem("id").Value) + 1).ToString();
                                }
                            }
                        }
                    }
                }
                if (lanedata[index][1] != lanedata[index + 1][1] && lanedata[index][1] < lanedata[index + 1][1])
                {
                    for (int i = 0; i < laneLink.Count; i++)
                    {
                        if (i == 0)
                        {
                            laneLink[i].AppendChild(Export.CreateElement("successor"));
                            Make_Attribute(laneLink[i].LastChild, "id", (-2).ToString());
                        }
                        else
                        {
                            for (int k = 0; k < laneLink[i].ChildNodes.Count; k++)
                            {
                                if (laneLink[i].ChildNodes[k].Name == "successor")
                                {
                                    tmNode = laneLink[i].ChildNodes[k];
                                }
                                else
                                {
                                    continue;
                                }
                                if (tmNode.Attributes.GetNamedItem("id").Value != "-1".ToString())
                                {
                                    tmNode.Attributes.GetNamedItem("id").Value = (Convert.ToInt32(tmNode.Attributes.GetNamedItem("id").Value) - 1).ToString();
                                }
                            }
                        }
                    }
                }
                else if (lanedata[index][1] != lanedata[index + 1][1] && lanedata[index][1] > lanedata[index + 1][1])
                {
                    for (int i = 0; i < laneLink.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        else
                        {
                            for (int k = 0; k < laneLink[i].ChildNodes.Count; k++)
                            {
                                if (laneLink[i].ChildNodes[k].Name == "successor")
                                {
                                    tmNode = laneLink[i].ChildNodes[k];
                                }
                                else
                                {
                                    continue;
                                }
                                if (tmNode.Attributes.GetNamedItem("id").Value != "-1".ToString())
                                {
                                    tmNode.Attributes.GetNamedItem("id").Value = (Convert.ToInt32(tmNode.Attributes.GetNamedItem("id").Value) + 1).ToString();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        private List<XmlNode> GetLaneLink(XmlNode rightNode)
        {
            List<XmlNode> laneLinkNodes = new List<XmlNode>();
            XmlNode laneNode;
            for (int i = 0; i < rightNode.ChildNodes.Count; i++)
            {
                laneNode = rightNode.ChildNodes[i];
                for (int k = 0; k < laneNode.ChildNodes.Count; k++)
                {
                    if (laneNode.ChildNodes[k].Name == "link")
                    {
                        laneLinkNodes.Add(laneNode.ChildNodes[k]);
                    }
                }
            }

            return laneLinkNodes;
        }

        private Entry_PS ChkEntry(XmlNode linkNode)
        {
            Entry_PS rEntry_PS;
            bool prFlag = false, suFlag = false;
            for (int i = 0; i < linkNode.ChildNodes.Count; i++)
            {
                if (linkNode.ChildNodes[i].Name == "predecessor")
                {
                    prFlag = true;
                }
                else if (linkNode.ChildNodes[i].Name == "successor")
                {
                    suFlag = true;
                }
            }

            if (prFlag && suFlag)
            {
                rEntry_PS = Entry_PS.all;
            }
            else if (prFlag)
            {
                rEntry_PS = Entry_PS.predecessor;
            }
            else if (suFlag)
            {
                rEntry_PS = Entry_PS.successor;
            }
            else
            {
                return Entry_PS.none;
            }

            return rEntry_PS;
        }

        public void AddLane(int[][] Lanedata, int[] rootToEnd)
        {
            string[] laneattr = { "id", "type", "level" };
            string[] laneval = { "", "driving", "false" };

            string[] wattr = { "sOffset", "a", "b", "c", "d" };
            string[] wval = { 0.0.ToString(Es), Road_Width.ToString(Es), 0.0.ToString(Es), 0.0.ToString(Es), 0.0.ToString(Es) };

            string[] roadmarkattr = { "sOffset", "type", "color", "width" };
            string[] roadmarkval = { 0.0.ToString(Es), "solid", "yellow", RoadMark_Width.ToString(Es) };
            int cntr = 0, idx = rootToEnd[0];
            int Road_indexer = 0;

            for (int i = 0; i < root.ChildNodes.Count; i++) // Road index 랑 분리할것 밖에서 선언후 if 안에서 처리
            {
                if (root.ChildNodes[i].Name == "road")
                {
                    root.ChildNodes[i].AppendChild(Export.CreateElement("lanes"));

                    XmlNode lastCH = root.ChildNodes[i].LastChild;
                    if (Lanedata[Road_indexer][1] != 0)
                    {
                        Make_LaneOffset(lastCH, Lanedata[Road_indexer - 1][1], Lanedata[Road_indexer][1]);
                    }
                    lastCH.AppendChild(Export.CreateElement("laneSection"));

                    lastCH = lastCH.LastChild;// last ch = laneSection
                    Make_Attribute(lastCH, "s", 0.0.ToString(Es));

                    lastCH.AppendChild(Export.CreateElement("center"));
                    lastCH = lastCH.LastChild; //lastch = center

                    lastCH.AppendChild(Export.CreateElement("lane"));
                    lastCH = lastCH.LastChild; //lastch = lane

                    laneval[0] = 0.ToString();
                    laneval[1] = "none";
                    Make_Attribute(lastCH, laneattr, laneval);

                    lastCH.AppendChild(Export.CreateElement("roadMark"));
                    lastCH = lastCH.LastChild; //lastch = roadmark
                    roadmarkval[2] = "yellow";
                    Make_Attribute(lastCH, roadmarkattr, roadmarkval);

                    lastCH = lastCH.ParentNode; //lastch = lane
                    lastCH = lastCH.ParentNode; //lastch = center
                    lastCH = lastCH.ParentNode; //lastch = lanesection

                    lastCH.AppendChild(Export.CreateElement("right"));
                    lastCH = lastCH.LastChild;

                    int lanecounter = Lanedata[Road_indexer][0] + Lanedata[Road_indexer][1];
                    bool split_Link = false;

                    if (Lanedata[Road_indexer][1] != 0)//포켓차로 구현부 작업 할것 이루 왼쪽 차선 작업 
                    {
                        for (int pcounter = 0; pcounter < Lanedata[Road_indexer][1]; pcounter++)
                        {
                            lastCH.AppendChild(Export.CreateElement("lane"));
                            lastCH = lastCH.LastChild;

                            laneval[0] = (-pcounter - 1).ToString();
                            laneval[1] = "driving";
                            Make_Attribute(lastCH, laneattr, laneval);

                            lastCH.AppendChild(Export.CreateElement("width"));
                            lastCH = lastCH.LastChild; // lastch = width
                            Make_Attribute(lastCH, wattr, wval);

                            lastCH = lastCH.ParentNode; //lastch = lane

                            lastCH.AppendChild(Export.CreateElement("roadMark"));
                            lastCH = lastCH.LastChild; //lastch = roadmark
                            roadmarkval[2] = "standard";
                            Make_Attribute(lastCH, roadmarkattr, roadmarkval);

                            lastCH = lastCH.ParentNode; //lastch = lane

                            lastCH.PrependChild(Make_Lane_LinkData(-1, Entry_PS.predecessor));

                            lastCH = lastCH.ParentNode; //lastch = right
                        }
                    }
                    else if (Lanedata[Road_indexer][1] != 0)
                    {

                    }

                    if (i == 2)
                    {
                        // pS, numberFlag TestPoint
                    }
                    for (int k = Lanedata[Road_indexer][1]; k < lanecounter; k++) // laneData 만큼 개수를 늘려주는 부분
                    {
                        Entry_PS pS;
                        lastCH.AppendChild(Export.CreateElement("lane"));
                        lastCH = lastCH.LastChild;

                        laneval[0] = (-k - 1).ToString();
                        laneval[1] = "driving";
                        Make_Attribute(lastCH, laneattr, laneval);

                        lastCH.AppendChild(Export.CreateElement("width"));
                        lastCH = lastCH.LastChild; // lastch = width
                        Make_Attribute(lastCH, wattr, wval);

                        lastCH = lastCH.ParentNode; //lastch = lane

                        lastCH.AppendChild(Export.CreateElement("roadMark"));
                        lastCH = lastCH.LastChild; //lastch = roadmark
                        roadmarkval[2] = "standard";
                        Make_Attribute(lastCH, roadmarkattr, roadmarkval);

                        lastCH = lastCH.ParentNode; //lastch = lane
                        if (i == root.ChildNodes.Count - 1)
                        {
                            lastCH = lastCH.ParentNode;
                        }
                        else
                        {
                            LaneStat[] numberFlag;
                            int prvlaneSize = -1;

                            if (Road_indexer == 0) // 도로 구분 부문
                            {
                                pS = Entry_PS.successor;
                                numberFlag = ChkLane(pS, 0, Lanedata[Road_indexer][0], Lanedata[Road_indexer + 1][0]);
                            }
                            else if (i == idx)
                            {
                                pS = Entry_PS.predecessor;
                                if (cntr != rootToEnd.Length - 1 && k == Lanedata[Road_indexer][0] - 1)
                                {
                                    cntr++;
                                    idx += rootToEnd[cntr];
                                }
                                numberFlag = ChkLane(pS, Lanedata[Road_indexer][0], Lanedata[Road_indexer][0], 0);
                            }
                            else if (cntr != 0 && i == idx - rootToEnd[cntr] + 1)
                            {
                                pS = Entry_PS.successor;
                                numberFlag = ChkLane(pS, 0, Lanedata[Road_indexer][0], Lanedata[Road_indexer + 1][0]);
                            }
                            else
                            {
                                pS = Entry_PS.all;
                                numberFlag = ChkLane(pS, Lanedata[Road_indexer - 1][0], Lanedata[Road_indexer][0], Lanedata[Road_indexer + 1][0]);
                            }

                            if (numberFlag[0] == LaneStat.split)//predecessor
                            {
                                prvlaneSize = Lanedata[Road_indexer - 1][0];
                            }
                            else if (numberFlag[1] == LaneStat.split)//successor
                            {
                                prvlaneSize = Lanedata[Road_indexer + 1][0];
                            }

                            if (k == lanecounter - 1)
                            {
                                if (numberFlag[0] == LaneStat.split)
                                {
                                    pS = Entry_PS.successor;
                                }
                                else if (numberFlag[1] == LaneStat.split)
                                {
                                    pS = Entry_PS.predecessor;
                                }
                                lastCH.PrependChild(Make_Lane_LinkData(-k - 1, prvlaneSize, numberFlag, pS));
                                prvlaneSize = -1;
                                split_Link = !split_Link;
                            }
                            else if (numberFlag[0] == LaneStat.split && k == prvlaneSize - 1 || numberFlag[1] == LaneStat.split && k == prvlaneSize - 1)
                            {
                                lastCH.PrependChild(Make_Lane_LinkData(-k - 1, prvlaneSize, numberFlag, pS));
                                prvlaneSize = -1;
                                split_Link = !split_Link;
                            }
                            else if (k != prvlaneSize - 1 && split_Link)
                            {
                                if (numberFlag[0] == LaneStat.split)
                                {
                                    pS = Entry_PS.successor;
                                }
                                else if (numberFlag[1] == LaneStat.split)
                                {
                                    pS = Entry_PS.predecessor;
                                }
                                lastCH.PrependChild(Make_Lane_LinkData(-k - 1, pS));
                            }
                            else if (!split_Link)
                            {
                                lastCH.PrependChild(Make_Lane_LinkData(-k - 1, pS));//Link정보 생성 부분
                            }
                            lastCH = lastCH.ParentNode;// right
                        }
                    }
                    Fix_Poket_Lane(Road_indexer, lastCH, Lanedata);
                    Road_indexer++;
                }
            }
        }

        public void XMLSave(string xodr_name = null)
        {
            if (xodr_name == null)
            {
                Export.Save(XmlWriter.Create(@".\export_xodr.xodr", setting));
            }
            else
            {
                Export.Save(XmlWriter.Create(xodr_name + @".xodr", setting));
            }
        }
    }
}
