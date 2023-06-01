using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRASP_CVRP;

   

    public class FileHandler //Multipurpose


    {
        public List<POINT> Plist
        {
            get
            {
              
                List<POINT> list = new List<POINT>();
                string[] lines = File.ReadAllLines(Filepath1);
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(' ');
                    if (parts[0] == "TYPE")
                    {
                        if (parts.Contains("CVRP")) { list = points().ToList(); Distancematrix = Constructions.distancematrix(list); break; } //create points and fill also the distance matrix
                        else if (parts[2] == "AVRP") { list = pointsfrommatrix(); break; } //makes a list of the points with no coordinates and also adds the distance matrix //needs testing
                        else { Log.Add("VRP Type issue"); break; }
                    }
                    else if ((i == lines.Length - 1) && !Log.Contains("TYPE not found in file")) { Log.Add("TYPE not found in file"); i = lines.Length; break; }
                }
                return list;
            }
            set { }
        }
        public double[,] Distancematrix { get; set; }
        public string Filepath1 { get; set; }
        public string Filepath2 { get; set; }
        public int Maxcap
        {
            get { int cap = Capacityread(); return cap; }
            set { }
        }

        public string Comment { get; set; }
        public List<string> Log { get; set; } = new List<string>();


        public int Capacityread()
        {
            int cap = 0;
            string[] lines = File.ReadAllLines(Filepath1);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts[0] == "CAPACITY")
                {
                    try { cap = Convert.ToInt32(parts[2]); }
                    catch (Exception e)
                    {
                        Log.Add(e.Message);
                    }; break;
                }
            }
            return cap;
        }
        public List<POINT> points()
        {
            List<POINT> pointlist = new List<POINT>();
            string[] lines = File.ReadAllLines(Filepath1);
            int pointsstart = 0; int demandstart = 0; int difference;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts.Contains("NODE_COORD_SECTION")) { pointsstart = i + 1; break; }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts.Contains("DEMAND_SECTION")) { demandstart = i + 1; break; }

            }
            difference = demandstart - pointsstart;
            for (int i = pointsstart; i < demandstart; i++)
            {
                if (i + difference >= lines.Length) { break; } //if over the lenght of file then break
                string[] parts = lines[i].Split(' ');
                string[] parts2 = lines[i + difference].Split(' ');
                if (parts[0] == parts2[0])

                {
                    POINT p = new POINT();
                    p.ID = Convert.ToInt32(parts[0]);
                    p.Xcoordinate = Convert.ToInt32(parts[1]);
                    p.Ycoordinate = Convert.ToInt32(parts[2]);
                    p.Demand = Convert.ToInt32(parts2[1]);
                    pointlist.Add(p);

                }
                else if (pointlist.Count < 1) //if no points exist and the indexes dont match
                {
                    POINT p = new POINT();
                    p.Log.Add("ID of Nodes and Demand dont match in Filehandler");
                    pointlist.Add(p);
                    break;
                }
            }
            return pointlist;
        }
        public List<POINT> pointsfrommatrix()
        {
            List<POINT> pointlist = new List<POINT>();
            string[] lines = File.ReadAllLines(Filepath1);
            int pointsstart = 0; int demandstart = 0; int difference; int dimension = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts[0] == "EDGE") { pointsstart = i + 1; break; }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts[0] == "DIMENSION")
                {
                    try { dimension = Convert.ToInt32(parts[2]); }
                    catch (Exception e)
                    {
                        Log.Add(e.Message);
                    }; break;
                }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts[0] == "DEMAND") { demandstart = i + 1; break; }

            }
            difference = demandstart - pointsstart;
            for (int i = demandstart; i < lines.Length; i++)
            {

                string[] parts = lines[i].Split(' ');
                {
                    POINT p = new POINT();
                    p.ID = Convert.ToInt32(parts[0]);

                    p.Demand = Convert.ToInt32(parts[1]);
                    pointlist.Add(p);
                }

            }
            double[,] distancematrix = new double[dimension, dimension];
            for (int i = 0; i < dimension; i++)
            {
                string[] parts = lines[i + pointsstart].Split(' ');
                for (int j = 0; j < dimension; j++)
                {
                    distancematrix[i, j] = Convert.ToDouble(parts[j]);
                }
            }
            Distancematrix = distancematrix;
            return pointlist;
        }


        public void WriteToFile(string Filepath2)
        {
            points();
            foreach (POINT p in Plist)
            {
                try
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@Filepath2, true))
                    {
                        file.WriteLine(p.ID + "," + p.Xcoordinate + "," + p.Ycoordinate + "\n");
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("The exception was: " + ex.Message);
                }



            }

        }


        public FileHandler() { }
        public FileHandler(string filepath1, string filepath2)
        {
            Filepath1 = filepath1;
            Filepath2 = filepath2;

        }
        public override string ToString()
        {
            string output = string.Empty;
            foreach (string s in Log)
            {
                output += s + "; ";
            }

            return $"Pointlist Count:{Plist.Count} ; Capacity: {Maxcap} ; Log: {output.ToString()}";
        }

    }












