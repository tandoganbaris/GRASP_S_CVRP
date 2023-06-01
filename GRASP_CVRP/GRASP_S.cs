using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRASP_CVRP;
public class GRASP_S
{
    public int Selection_pref { get; set; } = 2;
    public int RCL_Length { get; set; } = 6;
    public List<POINT> Points = new List<POINT>();
    public int Capacity { get; set; } = 0; //each vehicle Cap
    public double[,] distancematrix
    {
        get
        {


            double[,] dmatrix = new double[Points.Count, Points.Count];
            for (int i = 0; i < Points.Count; i++)
            {
                for (int j = 0; j < Points.Count; j++)
                {
                    dmatrix[i, j] = Constructions.Euclideandistance(Points[i], Points[j]);
                }
            }
            return dmatrix;





        }
        set { }




    }
    public Stopwatch Timer { get; set; } = new Stopwatch();
    public int Timerlimit { get; set; } = 15 * 60000; //modify the former number for limit in minutes
    public int IterationLimit { get; set; } = 7000;
    public int Petalacceptance { get; set; } = 100;
    public Random rnd = new Random();

    public SortedList<double, List<Tour>> Petals { get; set; } = new SortedList<double, List<Tour>>();
    public SolutionGRASP Finalsolution { get; set; } = new SolutionGRASP();
    public GRASP_S() { }
    public GRASP_S(List<POINT> points, int capacity)
    {
        Points = points;
        Capacity = capacity;
    }

    public void Main()
    {
        int iteration = 0;
        while ((iteration < IterationLimit) && (Timer.ElapsedMilliseconds < Timerlimit))
        {
            while (iteration % Petalacceptance != 0)
            {
                SolutionGRASP incumbent = new SolutionGRASP(Constructions.GRASPcheapestinsertion(ref rnd, Capacity, RCL_Length, Points, distancematrix, Selection_pref));//build solution
                SolutionGRASP incumbent_afterLS = LocalSearch(incumbent);
                //acceptance criteria

                iteration++;
            }
            if (iteration % Petalacceptance == 0)
            {
                //choose best Petal
                //remove its nodes from the remaining
                iteration++;
            }
        }
    }

    public SolutionGRASP LocalSearch(SolutionGRASP incumbent)
    {
        SolutionGRASP output = new SolutionGRASP();

        return output;
    }
}
