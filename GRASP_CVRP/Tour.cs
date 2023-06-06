using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GRASP_CVRP;




public class Tour : ICloneable
{
    public List<POINT> Visitednodes { get; set; } = new List<POINT>();
    public List<POINT> Unvisitednodes { get; set; } = new List<POINT>();
    public List<POINT> Initialnodes { get; set; } = new List<POINT>();

    public Dictionary<string, string> Tourlog { get; set; } = new Dictionary<string, string>();

    public double Fitness { get { double d = Math.Pow((10 / Distance), 2); return d; } set { } } //note that the power here is to amplify differences between good tours and bad tours 

    public List<string> Log { get; set; } = new List<string>();
    public double Tourload
    {
        get
        {
            double load = 0;

            if (Visitednodes.Count >= 2) //if min two nodes
            {
                if (Visitednodes.First().ID == Visitednodes.Last().ID)
                {
                    foreach (POINT point in Visitednodes)
                    {
                        load += point.Demand;
                    }
                    load -= Visitednodes.Last().Demand;


                }
                else
                {
                    foreach (POINT point in Visitednodes)
                    {
                        load += point.Demand;
                    }
                }

            }
            else if (Visitednodes.Count < 2)
            {
                foreach (POINT point in Visitednodes)
                {
                    load += point.Demand;
                }

            }
            return load;
        }
        set { }
    }
    public double Capacity { get; set; }

    public double[,] distancematrix { get; set; }

    public double Distance //distance is calculated depending on the visitednodes
    {
        get
        {
            double distance = 0;
            if (Visitednodes.Count > 0)
            {
                if (Visitednodes.First().ID == Visitednodes.Last().ID)
                { distance = Distancecalc(); }
                else if (!(Visitednodes.First().ID == Visitednodes.Last().ID)) { distance = Distancecalc_Onedepotinlist(); }
            }
            return distance;
        }
        set { }
    }
    public double PetalParameter //the higher the better
    {
        get
        {
            double output = 0;
            output += Math.Pow((100 / Distance), 2.5)*100 - Math.Pow((Capacity-Tourload), 2);
            return output;
        }
        set { }
    }

    public Tour(List<POINT> points)
    {
        foreach (POINT point in points)
        {
            POINT clone = (POINT)point.Clone();
            Unvisitednodes.Add(clone);
            Initialnodes.Add(clone);
        }
        Unvisitednodes = points.ToList();
        Initialnodes = points.ToList();



    }
    public Tour()
    {

    }
    public override string ToString()
    {
        string points = "Points: ";
        foreach (POINT p in Visitednodes.SkipLast(1).ToList())
        {
            points += p.ID.ToString() + "->";
        }
        points += Visitednodes.Last().ID.ToString();
        return $"Visitednodes:{Visitednodes.Count} ; Unvisitednodes: {Unvisitednodes.Count} ; Distance: {Distance} ; Load: {Tourload} ; {points.ToString()} \n";
    }

    public double Distancecalc() //if the depot exists twice, once in the beginning once in the end
    {
        double distance = 0;
        try
        {
            for (int i = 0; i < Visitednodes.Count - 1; i++)//count 49 for 48 nodes. at the end i=47(visitednodes[47]) to i=48 (visitednodes[48])(node 49), visitednodes starts from 0
            {
                //distance += constructions.Euclideandistance(Visitednodes[i], Visitednodes[i + 1]);
                distance += distancematrix[Visitednodes[i].ID - 1, Visitednodes[i + 1].ID - 1];
            }
        }
        catch (Exception e) { Log.Add("Distance Calculation Error: " + e.Message); }
        Distance = distance;
        return distance;

    }
    public double Distancecalc_Onedepotinlist() //if the depot exists once
    {

        double distance = 0;
        for (int i = 0; i < Visitednodes.Count - 1; i++)//count 48 for 48 nodes. at the end i=46(visitednodes[46]) to i=47 (visitednodes[47]), visitednodes starts from 0
        {
            //distance += constructions.Euclideandistance(Visitednodes[i], Visitednodes[i + 1]);
            distance += distancematrix[Visitednodes[i].ID - 1, Visitednodes[i + 1].ID - 1];
        }
        //distance += constructions.Euclideandistance(Visitednodes[0], Visitednodes.Last()); //connect last to depot
        distance += distancematrix[Visitednodes[0].ID - 1, Visitednodes.Last().ID - 1];

        return distance;

    }
    public void Refresh() //method to refresh parameters in case it doesnt work by itself
    {
        //DISTANCE REFRESH
        double distance = 0;
        if (Visitednodes.First().ID == Visitednodes.Last().ID)
        { distance = Distancecalc(); }
        else if (!(Visitednodes.First().ID == Visitednodes.Last().ID)) { distance = Distancecalc_Onedepotinlist(); }
        Distance = distance;

        //LOAD REFRESH
        double load = 0;
        foreach (POINT point in Visitednodes)
        {
            load += point.Demand;
        }
        Tourload = load;

        //MATRIX REFRESH

        //if (Visitednodes.Count > 0)
        //{
        //    double[,] dmatrix = new double[Visitednodes.Count, Visitednodes.Count];
        //    for (int i = 0; i < Visitednodes.Count; i++)
        //    {  
        //        for (int j = 0; j < Visitednodes.Count; j++)
        //        {
        //            dmatrix[i, j] = Constructions.Euclideandistance(Visitednodes[i], Visitednodes[j]);
        //        }
        //    }
        //    distancematrix = dmatrix;

        //}
        //else { double[,] matrix = new double[1, 1]; distancematrix = matrix; }

        return;
    }
    public object Clone()
    {
        return this.MemberwiseClone();
    }

}

public class SolutionGRASP
{
    public double Totaldist
    {
        get
        {
            double distance = 0;
            foreach (Tour t in Petals)
            {
                try { distance += t.Distance; }
                catch (Exception e) { Log.Add(e.Message); }
            }
            return distance;
        }
        set { }
    }
    public List<string> Log { get; set; } = new List<string>();
    public List<Tour> Petals { get; set; } = new List<Tour>();
    public Random rnd = new Random(); //ref random to avoid repetition
    public SolutionGRASP() { }
    public SolutionGRASP(List<Tour> input)
    {
        Petals = input.ToList();
    }
    public override string ToString()
    {
        string output = $"totaldistance: {Totaldist} ; No tours: {Petals.Count}";

        return output;
    }

}



