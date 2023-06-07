using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Schema;

namespace GRASP_CVRP;
public class GRASP_S
{
    public bool LocalSearchSwitch { get; set; } = true;
    public bool PetalUse { get; set; } = true;
    public int Selection_pref { get; set; } = 3;




    public int RCL_Length { get; set; } =5;
    public List<POINT> Points = new List<POINT>();
    public List<POINT> InitialPoints = new List<POINT>();
    public int Capacity { get; set; } = 0; //each vehicle Cap
    public double Methodavg { get; set; } = 1;
    public int Iteration { get; set; } = 0;

    public double alpha
    {
        get
        {
            double output = 0;
            if (Iteration > 100) { output = 0.5; }
            else { output = 0.005 * Iteration; }

            return output;
        }
        set { }
    }
    public List<double> Learning_hist { get; set; } = new List<double>();
    public void Resetall()
    {
        Iteration = 0;
        Methodavg = 1;
        Learning_hist.Clear();
        Petals.Clear();
        Points.Clear();
        Points.AddRange(InitialPoints);
        Timer.Reset();
        Solutions.Clear();
        Finalsolution = new SolutionGRASP();

    }
    public double[,] distancematrix { get; set; }

    public Stopwatch Timer { get; set; } = new Stopwatch();
    public int Timerlimit { get; set; } = (int)(0.65 * 60000); //modify the former number for limit in minutes
    public int IterationLimit { get; set; } = 50000;
    public int Petalacceptance { get; set; } = 100;
    public Random rnd = new Random();

    public SortedList<double, List<Tour>> Petals { get; set; } = new SortedList<double, List<Tour>>(); //choose best from after n iterations
    public SortedList<double, List<SolutionGRASP>> Solutions { get; set; } = new SortedList<double, List<SolutionGRASP>>();
    public double Averagequality { get { double output = 0; output += Solutions.Keys.Average(); return output; } set { } }
    public SolutionGRASP Finalsolution { get; set; } = new SolutionGRASP();

    /// <summary>
    /// dont use this as the distance matrix wont be calculated in the constructor
    /// </summary>
    public GRASP_S() { }
    public GRASP_S(List<POINT> points, int capacity)
    {
        InitialPoints = points;
        Points = points;
        Capacity = capacity;
        Calculate_dmatrix();

    }
    public void Calculate_dmatrix()
    {
        double[,] dmatrix = new double[Points.Count, Points.Count];
        for (int i = 0; i < Points.Count; i++)
        {
            for (int j = 0; j < Points.Count; j++)
            {
                dmatrix[i, j] = Constructions.Euclideandistance(Points[i], Points[j]);
            }
        }
        distancematrix = dmatrix;
        return;
    }

    public void Mainalgo()
    {

        while ((Iteration < IterationLimit) && (Timer.ElapsedMilliseconds < Timerlimit) && (Points.Count > 1))
        {

            SolutionGRASP incumbent = new SolutionGRASP(Constructions.GRASPcheapestinsertion(ref rnd, Capacity, RCL_Length, Points, distancematrix, Selection_pref));//build solution
            SolutionGRASP incumbent_afterLS = new SolutionGRASP();
            //if (Iteration % 10 == 0)
            //{
                incumbent_afterLS = LocalSearch(incumbent);
            //}
            //else { incumbent_afterLS = incumbent; }



            //if (Iteration == 100)
            //{
            //    string here = string.Empty;
            //}

            switch (PetalUse)
            {
                case false:
                    {
                        if (!Solutions.ContainsKey(incumbent_afterLS.Totaldist)) { Solutions[incumbent_afterLS.Totaldist] = new List<SolutionGRASP> { incumbent_afterLS }; } //if the key doesnt exist
                        else { Solutions[incumbent_afterLS.Totaldist].Add(incumbent_afterLS); } //if the key exists
                        if (Solutions.Count > 200)
                        {
                            SortedList<double, List<SolutionGRASP>> output = new SortedList<double, List<SolutionGRASP>>();
                            for (int i = 0; i < Solutions.Count - 1; i++)
                            {
                                double key = Solutions.ElementAt(i).Key;
                                List<SolutionGRASP> value = Solutions.ElementAt(i).Value;
                                output[key] = value;
                            }
                            Solutions.Clear();
                            Solutions = output;

                        }
                        Iteration++;

                        break;
                    }
                case true:
                    {
                        foreach (Tour t in incumbent_afterLS.Petals) //will be added to history
                        {


                            if (!Petals.ContainsKey(t.PetalParameter)) { Petals[t.PetalParameter] = new List<Tour> { (Tour)t.Clone() }; } //if the key doesnt exist
                            else { Petals[t.PetalParameter].Add((Tour)t.Clone()); } //if the key exists

                        }
                        if (Petals.Count > 200)
                        {
                            SortedList<double, List<Tour>> output = new SortedList<double, List<Tour>>();
                            for (int i = Petals.Count - 1; i > 99; i--)
                            {
                                double key = Petals.ElementAt(i).Key;
                                List<Tour> value = Petals.ElementAt(i).Value;
                                output[key] = value;
                            }
                            Petals.Clear();
                            Petals = output;

                        }

                        Iteration++;

                        if ((Iteration % Petalacceptance == 0) && (Iteration > 10))
                        {
                            Tour chosenone = Petals.Last().Value.First() as Tour;          //choose best Petal
                            Finalsolution.Petals.Add(chosenone);
                            foreach (POINT p in chosenone.Visitednodes)//remove its nodes from the remaining
                            {
                                if (p.Demand != 0)
                                {
                                    Points.Remove(p);
                                }

                            }
                            Petals.Clear();


                            Iteration++;

                        }
                        break;
                    }
            }


        }
        return;
    }

    public SolutionGRASP LocalSearch(SolutionGRASP incumbent)
    {
        double valuebefore = incumbent.Totaldist;
        SolutionGRASP output = new SolutionGRASP();
        double randomchoice = (rnd.NextDouble() * 3) * (1 - alpha) + Methodavg * alpha;
        randomchoice = (double)((int)randomchoice); //round down
        double method = 0; double quality = 0;
        switch (LocalSearchSwitch)
        {
            case true:
                {
                    //    if (randomchoice < 1) { output = Intraroute(incumbent); method = 0; }


                    //    else if (randomchoice >= 1 && randomchoice < 2) { output = Singlenode(incumbent); method = 1; }

                    //    else { output = Interroute(incumbent); method = 2; }


                    //    if (output.Totaldist < valuebefore) { quality = 1 - (output.Totaldist / valuebefore); }


                    output = Singlenode(incumbent); method = 1;

                    //UpdateLearning(method, quality);


                    break;
                }
            case false:
                {
                    output = incumbent;
                    break;
                }
        }



        return output;
    }
    public void UpdateLearning(double method, double quality)
    {
        if (quality > 0.001)
        {
            int frequency = Convert.ToInt16(quality / 0.001);
            for (int i = 0; i < frequency; i++)
            {
                Learning_hist.Add(method);
            }
            Learning_hist = Learning_hist.TakeLast(1000).ToList();

            Methodavg = Learning_hist.Average();
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
        while (!improved && timerLS.ElapsedMilliseconds < 5000)
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
                                    double R1Distancebefore = distancematrix[route1.Visitednodes[i].ID - 1, route1.Visitednodes[i + 1].ID - 1] +
                                        distancematrix[route1.Visitednodes[i].ID - 1, route1.Visitednodes[i - 1].ID - 1];
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
                                        output = incumbent;
                                        return output;

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
        double valuebefore = incumbent.Totaldist;
        SolutionGRASP output = new SolutionGRASP();
        Stopwatch timerLS = new Stopwatch();
        timerLS.Start();
        bool improved = false;
        int iteration = 0; //to break out of infinite loops

        incumbent.Petals = incumbent.Petals.OrderBy(x => rnd.Next()).ToList(); //shuffle tours to not repeat choosing the same tours
        while (!improved && timerLS.ElapsedMilliseconds < 5000)
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

                        int newindex = rnd.Next(1, route.Visitednodes.Count - 1); //choose random index
                        while (customer.ID == route.Visitednodes[newindex].ID || customer.ID == route.Visitednodes[newindex + 1].ID) //avoid having the same index
                        {
                            newindex = rnd.Next(1, route.Visitednodes.Count - 1);
                            iteration++;
                            if (iteration > 50) { return incumbent; }
                        }
                        double R1Distanceafter = distancematrix[customer.ID - 1, route.Visitednodes[newindex].ID - 1] +
                        distancematrix[customer.ID - 1, route.Visitednodes[newindex + 1].ID - 1] -
                        distancematrix[route.Visitednodes[newindex].ID - 1, route.Visitednodes[newindex + 1].ID - 1];


                        if (R1Distancebefore > R1Distanceafter)
                        {
                            POINT customer2 = route.Visitednodes[newindex];

                            route.Visitednodes.Remove(customer); //remove the customer

                            route.Visitednodes.Insert(route.Visitednodes.IndexOf(customer2) + 1, customer); //insert after the c2 

                            improved = true;
                            output = incumbent;
                            return output;

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
        while (!improved && timerLS.ElapsedMilliseconds < 5000)
        {

            foreach (Tour route in incumbent.Petals)
            {

                for (int i = 1; i < route.Visitednodes.Count - 1; i++) //omits first and last node
                {
                    for (int j = 1; j < route.Visitednodes.Count - 1; j++)
                    {

                        if ((i != j) && (i != j + 1 | j != i + 1)) //they are not same and not after one another
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
                            double before = R1Distancebefore + R2Distancebefore;
                            double after = R1Distanceafter + R2Distanceafter;
                            if (before > after)
                            {
                                route.Visitednodes[i] = customer2;
                                route.Visitednodes[j] = customer1;
                                improved = true;
                                output = incumbent;
                                return output;

                            }

                        }
                        else if ((i != j) && (i == j + 1 || j == i + 1)) //after one another 
                        {
                            if (i == j + 1) // j before i
                            {
                                POINT customer1 = route.Visitednodes[i];
                                POINT customer2 = route.Visitednodes[j];


                                double before = distancematrix[route.Visitednodes[j - 1].ID - 1, customer2.ID - 1] +
                                    distancematrix[customer1.ID - 1, route.Visitednodes[i + 1].ID - 1];

                                double after = distancematrix[route.Visitednodes[j - 1].ID - 1, customer1.ID - 1] +
                                    distancematrix[customer2.ID - 1, route.Visitednodes[i + 1].ID - 1];

                                if (before > after)
                                {
                                    route.Visitednodes[i] = customer2;
                                    route.Visitednodes[j] = customer1;
                                    improved = true;
                                    output = incumbent;
                                    return output;

                                }

                            }
                            else // i before j 
                            {
                                POINT customer1 = route.Visitednodes[i];
                                POINT customer2 = route.Visitednodes[j];


                                double before = distancematrix[route.Visitednodes[i - 1].ID - 1, customer1.ID - 1] +
                                    distancematrix[customer2.ID - 1, route.Visitednodes[j + 1].ID - 1];

                                double after = distancematrix[route.Visitednodes[i - 1].ID - 1, customer2.ID - 1] +
                                    distancematrix[customer1.ID - 1, route.Visitednodes[j + 1].ID - 1];

                                if (before > after)
                                {
                                    route.Visitednodes[i] = customer2;
                                    route.Visitednodes[j] = customer1;
                                    improved = true;
                                    output = incumbent;
                                    return output;

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
}
