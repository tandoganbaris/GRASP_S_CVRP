using System;
using System.Collections.Generic;
using System.Data;
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
    public double LearningRate { get; set; } = 1;
    public List<double> Learning_hist { get; set; } = new List<double>();
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

    public SortedList<double, List<Tour>> Petals { get; set; } = new SortedList<double, List<Tour>>(); //choose best from after n iterations
    public SolutionGRASP Finalsolution { get; set; } = new SolutionGRASP();
    public GRASP_S() { }
    public GRASP_S(List<POINT> points, int capacity)
    {
        Points = points;
        Capacity = capacity;
    }

    public void Mainalgo()
    {
        int iteration = 0;
        while ((iteration < IterationLimit) && (Timer.ElapsedMilliseconds < Timerlimit))
        {
            while (iteration % Petalacceptance != 0)
            {
                SolutionGRASP incumbent = new SolutionGRASP(Constructions.GRASPcheapestinsertion(ref rnd, Capacity, RCL_Length, Points, distancematrix, Selection_pref));//build solution
                SolutionGRASP incumbent_afterLS = LocalSearch(incumbent);




                foreach (Tour t in incumbent_afterLS.Petals) //will be added to history
                {
                    if (!Petals.ContainsKey(t.Distance)) { Petals[t.Distance] = new List<Tour> { (Tour)t.Clone() }; } //if the key doesnt exist
                    else { Petals[t.Distance].Add((Tour)t.Clone()); } //if the key exists

                }

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
        double randomchoice = (rnd.NextDouble()*3)*0.7 + LearningRate*0.3;
        randomchoice = (double)((int)randomchoice); //round down
        double method = 0; double quality = 0;
        
        // if(randomchoice <1){ output = Intraroute(incumbent); method = 0; }


        // else if(randomchoice>=1 && randomchoice<2) { output = Singlenode(incumbent); method = 1; }

        // else { output = Interroute(incumbent); method = 2; }


        // if(output.Totaldist < incumbent.Totaldist) { quality = 1 - (output.Totaldist / incumbent.Totaldist); }

        //UpdateLearning(method, quality);
        return output;
    }
    public void UpdateLearning(double method, double quality)
    {
        if (quality > 0)
        {
            int frequency = Convert.ToInt16(quality / 0.002);
            for (int i = 0; i < frequency; i++)
            {
                Learning_hist.Add(method);
            }
            Learning_hist = Learning_hist.TakeLast(1000).ToList();

            LearningRate = Learning_hist.Average();
        }
        

        return;
    }

    public SolutionGRASP Intraroute(SolutionGRASP incumbent)
    {
        SolutionGRASP output = new SolutionGRASP();
        Stopwatch timerLS = new Stopwatch();
        timerLS.Start();
        bool improved = false;

        incumbent.Petals = incumbent.Petals.OrderBy(x => rnd.Next()).ToList(); //shuffle tours to not repeat choosing the same tours
        while (!improved && timerLS.ElapsedMilliseconds < 15000) //30 second LS limit 
        {

            foreach (Tour route1 in incumbent.Petals)
            {
                foreach (Tour route2 in incumbent.Petals)
                {
                    if (route1 != route2)
                    {
                        for (int i = 1; i < route1.Visitednodes.Count - 1; i++) //omits first and last node
                        {
                            for (int j = 1; j < route2.Visitednodes.Count - 1; j++)
                            {
                                POINT customer1 = route1.Visitednodes[i];
                                POINT customer2 = route2.Visitednodes[j];

                                if (customer1.Demand + route2.Tourload <= route2.Capacity &&
                                    customer2.Demand + route1.Tourload <= route1.Capacity)
                                {
                                    double R1Distancebefore = distancematrix[route1.Visitednodes[i].ID-1, route1.Visitednodes[i + 1].ID-1] +
                                        distancematrix[route1.Visitednodes[i].ID - 1, route1.Visitednodes[i - 1].ID- 1];
                                    double R2Distancebefore = distancematrix[route2.Visitednodes[j].ID - 1, route2.Visitednodes[j + 1].ID - 1] +
                                        distancematrix[route2.Visitednodes[j].ID - 1, route2.Visitednodes[j - 1].ID - 1];

                                    double R1Distanceafter = distancematrix[route2.Visitednodes[j].ID - 1, route1.Visitednodes[i + 1].ID - 1] +
                                        distancematrix[route2.Visitednodes[j].ID - 1, route1.Visitednodes[i - 1].ID - 1];
                                    double R2Distanceafter = distancematrix[route1.Visitednodes[i].ID - 1, route2.Visitednodes[j + 1].ID - 1] +
                                        distancematrix[route1.Visitednodes[i].ID - 1, route2.Visitednodes[j - 1].ID - 1];

                                    if (R1Distancebefore + R2Distancebefore > R1Distanceafter + R2Distanceafter)
                                    {
                                        route1.Visitednodes[i] = customer2;
                                        route2.Visitednodes[j] = customer1;
                                        improved = true;

                                    }


                                }
                            }
                        }
                    }
                }
            }
        }
        output = incumbent;
        return output;
    }
    public SolutionGRASP Singlenode(SolutionGRASP incumbent)
    {
        SolutionGRASP output = new SolutionGRASP();
        Stopwatch timerLS = new Stopwatch();
        timerLS.Start();
        bool improved = false;

        incumbent.Petals = incumbent.Petals.OrderBy(x => rnd.Next()).ToList(); //shuffle tours to not repeat choosing the same tours
        while (!improved && timerLS.ElapsedMilliseconds < 15000) //30 second LS limit 
        {

            foreach (Tour route in incumbent.Petals)
            {
                for (int i = 1; i < route.Visitednodes.Count - 1; i++) //omits first and last node
                {
                    POINT customer = route.Visitednodes[i];

                    double R1Distancebefore = (distancematrix[customer.ID - 1, route.Visitednodes[i + 1].ID - 1] +
                        distancematrix[customer.ID - 1, route.Visitednodes[i - 1].ID - 1]) -
                        distancematrix[route.Visitednodes[i + 1].ID - 1, route.Visitednodes[i - 1].ID - 1];

                    for (int j = 1; j < route.Visitednodes.Count - 1; j++)
                    {
                        int newindex = rnd.Next(1, route.Visitednodes.Count - 1);
                        double R1Distanceafter = distancematrix[customer.ID - 1, route.Visitednodes[j].ID - 1] +
                        distancematrix[customer.ID-1, route.Visitednodes[j + 1].ID-1] -
                        distancematrix[route.Visitednodes[j].ID - 1, route.Visitednodes[j + 1].ID - 1];


                        if (R1Distancebefore > R1Distanceafter)
                        {
                            POINT customer2 = route.Visitednodes[j];

                            route.Visitednodes.Remove(customer); //remove the customer

                            route.Visitednodes.Insert(route.Visitednodes.IndexOf(customer2) + 1, customer); //insert after the c2 

                            improved = true; break;

                        }
                    }
                }
            }
        }
        output = incumbent;
        return output;
    }
    public SolutionGRASP Interroute(SolutionGRASP incumbent)
    {
        SolutionGRASP output = new SolutionGRASP();
        Stopwatch timerLS = new Stopwatch();
        timerLS.Start();
        bool improved = false;

        incumbent.Petals = incumbent.Petals.OrderBy(x => rnd.Next()).ToList(); //shuffle tours to not repeat choosing the same tours
        while (!improved && timerLS.ElapsedMilliseconds < 15000) //30 second LS limit 
        {

            foreach (Tour route in incumbent.Petals)
            {

                for (int i = 1; i < route.Visitednodes.Count - 1; i++) //omits first and last node
                {
                    for (int j = 1; j < route.Visitednodes.Count - 1; j++)
                    {

                        if (i != j)
                        {
                            POINT customer1 = route.Visitednodes[i];
                            POINT customer2 = route.Visitednodes[j];

                           
                                double R1Distancebefore = distancematrix[route.Visitednodes[i].ID - 1, route.Visitednodes[i + 1].ID - 1] +
                                    distancematrix[route.Visitednodes[i].ID - 1, route.Visitednodes[i - 1].ID - 1];
                                double R2Distancebefore = distancematrix[route.Visitednodes[j].ID - 1, route.Visitednodes[j + 1].ID - 1] +
                                    distancematrix[route.Visitednodes[j].ID - 1, route.Visitednodes[j - 1].ID - 1];

                                double R1Distanceafter = distancematrix[route.Visitednodes[j].ID - 1, route.Visitednodes[i + 1].ID - 1] +
                                    distancematrix[route.Visitednodes[j].ID - 1, route.Visitednodes[i - 1].ID - 1];
                                double R2Distanceafter = distancematrix[route.Visitednodes[i].ID - 1, route.Visitednodes[j + 1].ID - 1] +
                                    distancematrix[route.Visitednodes[i].ID - 1, route.Visitednodes[j - 1].ID - 1];

                                if (R1Distancebefore + R2Distancebefore > R1Distanceafter + R2Distanceafter)
                                {
                                    route.Visitednodes[i] = customer2;
                                    route.Visitednodes[j] = customer1;
                                    improved = true;

                                }
                            
                        }
                    }
                }


            }
        }
        output = incumbent;
        return output;
    }
}
