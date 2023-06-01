using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRASP_CVRP;
public class POINT : ICloneable
{
    public int ID { get; set; }
    public int Xcoordinate { get; set; }
    public int Ycoordinate { get; set; }

    public double Demand { get; set; }
    public List<string> Log { get; set; } = new List<string>();
    public bool Isloaded { get; set; }


    public POINT() { }

    public POINT(int id, int xcoordinate, int ycoordinate, double demand)
    {
        ID = id;
        Xcoordinate = xcoordinate;
        Ycoordinate = ycoordinate;
        Demand = demand;

    }
    public override string ToString()
    {
        string output = $"[ID: {ID.ToString().PadRight(4)};  X: {Xcoordinate.ToString().PadRight(3)} / Y: {Ycoordinate.ToString().PadRight(3)} ;  Demand: {Demand.ToString().PadRight(3)}]";
        if (!(Log.Count == 0)) output += string.Join(" ; ", Log);
        return output;
    }
    public object Clone()
    {
        POINT output = new POINT(ID, Xcoordinate, Ycoordinate, Demand);
        return output;
    }

}

