using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GRASP_CVRP;




public static class Constructions
{

    //GRASP SECTION BEGINS

    public static List<Tour> GRASPcheapestinsertion(ref Random rnd, int capacity, int RCLlength, List<POINT> points, double[,] dmatrix, int selectpref)
    {
        List<Tour> output = new List<Tour>();
        int localcap = capacity;
        POINT depot = points.First();
        List<POINT> visited = new List<POINT> { depot };
        List<POINT> pointsremaining = points.Except(visited).ToList();

        while (pointsremaining.Count > 0)
        {
            output.Add(CheapestinsertionGRASP(ref rnd, RCLlength, ref pointsremaining, depot, capacity, dmatrix, selectpref));
        }

        foreach (Tour t in output)
        {
            t.Initialnodes = points.ToList();
            t.Capacity = capacity;
            t.distancematrix = dmatrix;
            // t.Refresh();
        }
        return output;
    }

    public static Tour CheapestinsertionGRASP(ref Random rnd, int RCLlenght, ref List<POINT> remainingpoints, POINT depot, int capacity, double[,] dmatrix, int selectpref)
    {
        int nofit_iteration = 0;
        int maxnofit = (int)(RCLlenght / 2.0);
        bool returntour = false;
        Tour result = new Tour();
        result.Visitednodes.Add(depot);
        result.Visitednodes.Add(depot);
        result.Unvisitednodes.AddRange(remainingpoints);


        while (!returntour)
        {
            //construct RCL
            if (remainingpoints.Count == 0) { returntour = true; break; }
            Dictionary<double, List<Tuple<POINT, int>>> RCL = RCList(result, ref rnd, RCLlenght, dmatrix);

            //choose point
            double choice = rnd.NextDouble();
            int index = 0;
            POINT chosen = Choose(choice, RCL, out index, selectpref);
            double solodistance = dmatrix[depot.ID - 1, chosen.ID - 1] * 2; //if the node was a tour on its own
            double inserteddistance = dmatrix[result.Visitednodes[index].ID - 1, chosen.ID - 1] + //gives the added distance
                dmatrix[result.Visitednodes[index + 1].ID - 1, chosen.ID - 1] -
                dmatrix[result.Visitednodes[index].ID - 1, result.Visitednodes[index + 1].ID - 1];

            while ((result.Tourload + chosen.Demand > capacity) || (solodistance < inserteddistance))
            {//choose new
                double newchoice = rnd.NextDouble();
                int newindex = 0;
                chosen = Choose(newchoice, RCL, out newindex, selectpref);
                nofit_iteration++;
                solodistance = dmatrix[depot.ID - 1, chosen.ID - 1] * 2; //if the node was a tour on its own
                inserteddistance = dmatrix[result.Visitednodes[index].ID - 1, chosen.ID - 1] + //gives the added distance
                    dmatrix[result.Visitednodes[index + 1].ID - 1, chosen.ID - 1] -
                    dmatrix[result.Visitednodes[index].ID - 1, result.Visitednodes[index + 1].ID - 1];
                if (nofit_iteration == maxnofit) { returntour = true; break; }
            }
            if (returntour) { break; }
            result.Visitednodes.Insert(index + 1, chosen);
            result.Unvisitednodes.Remove(chosen);
            remainingpoints.Remove(chosen);


        }

        return result;

    }
    public static POINT Choose(double choice, Dictionary<double, List<Tuple<POINT, int>>> RCL, out int index, int selectpref)
    {
        POINT output = new POINT();
        index = 0;
        double key_cum = 0;
        double choiceinternal = 0;
        Dictionary<double, List<Tuple<POINT, int>>> RCL_norm = new Dictionary<double, List<Tuple<POINT, int>>>();
        if (RCL.Keys.Count > 1)
        {
            for (int i = 0; i < RCL.Keys.Count; i++)
            {
                key_cum += (Math.Pow((10 / (RCL.ElementAt(i).Key+10)), selectpref)) * 100;
                RCL_norm[key_cum] = RCL[RCL.ElementAt(i).Key];
              
            }
            choiceinternal = choice * RCL_norm.Keys.Last(); //where the random double works on the range of tournament
            for (int i = 0; i < RCL_norm.Keys.Count - 1; i++)
            {
                if (RCL_norm.ElementAt(i+1).Key> choiceinternal)
                {
                        output = RCL_norm.ElementAt(i).Value.First().Item1 as POINT;
                        index = RCL_norm.ElementAt(i).Value.First().Item2;
                        break;                   
                }

            }
        }
        else
        {
            output = RCL.First().Value.First().Item1 as POINT;
            index = RCL.First().Value.First().Item2;
        }
        if (output.Demand == 0 && output.ID == 0)
        {
            string here = string.Empty;
        }
        return output;
    }
    public static Dictionary<double, List<Tuple<POINT, int>>> RCList(Tour tour, ref Random rnd, int RCLlength, double[,] dmatrix)
    {
        Dictionary<double, List<Tuple<POINT, int>>> output = new Dictionary<double, List<Tuple<POINT, int>>>();
        for (int i = 0; i < tour.Visitednodes.Count - 1; i++)
        {
            POINT before = tour.Visitednodes[i];
            POINT after = tour.Visitednodes[i + 1];
            double distanceintour = dmatrix[before.ID - 1, after.ID - 1];
            POINT pointtoinsert = new POINT();
            double distance1 = 0;
            double distance2 = 0;
            for (int j = 0; j < tour.Unvisitednodes.Count; j++) //find the cheapest point to insert for each index
            {
                pointtoinsert = tour.Unvisitednodes[j];
                distance1 = dmatrix[before.ID - 1, pointtoinsert.ID - 1];
                distance2 = dmatrix[pointtoinsert.ID - 1, after.ID - 1];
                double insertioncost = distance1 + distance2 - distanceintour;
                if (!output.Keys.Contains(insertioncost)) { output.Add(insertioncost, new List<Tuple<POINT, int>> { new Tuple<POINT, int>(pointtoinsert, i) }); }
                else { output[insertioncost].Add(new Tuple<POINT, int>(pointtoinsert, i)); }

            }
        }
        Dictionary<double, List<Tuple<POINT, int>>> output2 = output.OrderBy(x => x.Key)
                                                                     .Take(RCLlength)
                                                                     .ToDictionary(x => x.Key, x => x.Value);
        return output2;

    }


    //GRASP SECTION ENDS
    public static double Distancecalc(List<POINT> list) //if the depot exists twice, once in the beginning once in the end
    {

        double distance = 0;
        for (int i = 0; i < list.Count - 1; i++)//count 49 for 48 nodes. at the end i=47(visitednodes[47]) to i=48 (visitednodes[48])(node 49), visitednodes starts from 0
        {
            distance += Constructions.Euclideandistance(list[i], list[i + 1]);
        }

        return distance;

    }

    public static Tour Cheapestinsertion(List<POINT> points, int startingindex)
    {
        Tour result = new Tour(points);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);
        result.Unvisitednodes.RemoveAt(startingindex);
        while (result.Unvisitednodes.Count > 0)
        {
            POINT pointtoinsert = Cheapestpoint(result, out int insertionindex); //ask about the out int index
            result.Visitednodes.Insert(insertionindex + 1, pointtoinsert); //insertion always pushes the existing value further in the list, we dont want to push the value at index but insert after it, therefore index+1
            result.Unvisitednodes.Remove(pointtoinsert);
        }
        //result.Distancecalc();
        return result;

    }
    private static POINT Cheapestpoint(Tour tour, out int index)
    {

        double cheapestinsertioncost = double.MaxValue;
        index = 0;
        POINT result = new POINT();
        for (int i = 0; i < tour.Visitednodes.Count; i++)
        {
            POINT before = tour.Visitednodes[i];
            POINT after = null;
            if (i == tour.Visitednodes.Count - 1) { after = tour.Visitednodes[1]; } else { after = tour.Visitednodes[i + 1]; } // assume last node depot, first node depot, so we need the first node after the depot      
            double distanceintour = Euclideandistance(before, after);
            POINT pointtoinsert = new POINT();
            double distance1 = 0;
            double distance2 = 0;
            for (int j = 0; j < tour.Unvisitednodes.Count; j++) //find the cheapest point to insert for each index
            {
                pointtoinsert = tour.Unvisitednodes[j];
                distance1 = Euclideandistance(before, pointtoinsert);
                distance2 = Euclideandistance(after, pointtoinsert);
                double insertioncost = distance1 + distance2 - distanceintour;
                if (insertioncost < cheapestinsertioncost)
                {
                    cheapestinsertioncost = insertioncost;
                    result = pointtoinsert;
                    index = i; //find the index to enter the point

                }

            }

        }
        return result;

    }
    public static Tour Randominsertion(List<POINT> points, int startingindex) //this is a random tour so not really insertion. should have been like cheapest insertion, inserted between the nodes, at either the cheapest vertex or random vertex.
    {
        Tour result = new Tour(points);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);

        result.Unvisitednodes.RemoveAt(startingindex);
        while (result.Unvisitednodes.Count > 0)
        {
            Random random = new Random();
            int index = random.Next(0, (result.Unvisitednodes.Count) - 1);
            POINT pointtoinsert = result.Unvisitednodes[index];
            double distance1 = 0, distance2 = 0, distanceintour = 0; double nearestinsertioncost = double.MaxValue; int indextoinsert = 0;
            for (int k = 0; k < result.Visitednodes.Count; k++) //find where to insert
            {

                POINT before = result.Visitednodes[k];
                POINT after = null;
                if (k == result.Visitednodes.Count - 1) { after = result.Visitednodes[1]; } else { after = result.Visitednodes[k + 1]; }// assume last node depot, first node depot, so we need the first node after the depot  ||| count would return 48, last index is 48 because we added the startingindex at the end         

                distance1 = Euclideandistance(before, pointtoinsert);
                distance2 = Euclideandistance(after, pointtoinsert);
                distanceintour = Euclideandistance(before, after);
                double insertioncost = distance1 + distance2 - distanceintour;
                if (insertioncost < nearestinsertioncost)
                {
                    nearestinsertioncost = insertioncost;
                    indextoinsert = k;
                }
                else { continue; }
            }
            result.Visitednodes.Insert(indextoinsert + 1, pointtoinsert);
            result.Unvisitednodes.Remove(pointtoinsert);
        }
        result.Distancecalc();
        return result;

    }

    public static Tour Nearestinsertion(List<POINT> points, int startingindex)
    {
        Tour result = new Tour(points);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);
        result.Unvisitednodes.RemoveAt(startingindex);

        while (result.Unvisitednodes.Count > 0)
        {

            POINT pointtoinsert = Nearestpoint(result, out int insertionindex);
            result.Visitednodes.Insert(insertionindex + 1, pointtoinsert);
            result.Unvisitednodes.Remove(pointtoinsert);

        }
        //result.Distancecalc();

        return result;

    }
    private static POINT Nearestpoint(Tour tour, out int index)
    {

        double nearestinsertioncost = double.MaxValue;
        index = 0;
        POINT result = new POINT();
        POINT pointtoinsert = new POINT();
        double distance_totour;
        double distance_chosen = double.MaxValue;
        for (int i = 0; i < tour.Visitednodes.Count - 1; i++) //find which to insert, disregard last as its the same as first
        {
            POINT pOINT = tour.Visitednodes[i];



            for (int j = 0; j < tour.Unvisitednodes.Count; j++)
            {
                distance_totour = Euclideandistance(pOINT, tour.Unvisitednodes[j]); //
                if (distance_totour < distance_chosen) //find nearest point to the entire tour, for each i and j and compare.
                {
                    pointtoinsert = tour.Unvisitednodes[j];
                    distance_chosen = distance_totour;
                }
            }
        }

        //two options from here, either chose the preceding or following vertex of i or look for the cheapest vertex to insert j (probably the same outcome)
        double distance1 = 0, distance2 = 0, distanceintour = 0;
        for (int k = 0; k < tour.Visitednodes.Count; k++) //find where to insert
        {

            POINT before = tour.Visitednodes[k];
            POINT after = null;
            if (k == tour.Visitednodes.Count - 1) { after = tour.Visitednodes[1]; } else { after = tour.Visitednodes[k + 1]; }// assume last node depot, first node depot, so we need the first node after the depot  ||| count would return 48, last index is 48 because we added the startingindex at the end         

            distance1 = Euclideandistance(before, pointtoinsert);
            distance2 = Euclideandistance(after, pointtoinsert);
            distanceintour = Euclideandistance(before, after);
            double insertioncost = distance1 + distance2 - distanceintour;
            if (insertioncost < nearestinsertioncost)
            {
                nearestinsertioncost = insertioncost;
                index = k;
            }
        }



        result = pointtoinsert;



        return result;

    }


    public static Tour Farthestinsertion(List<POINT> points, int startingindex)
    {
        Tour result = new Tour(points);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);
        result.Visitednodes.Add(result.Unvisitednodes[startingindex]);
        result.Unvisitednodes.RemoveAt(startingindex);

        while (result.Unvisitednodes.Count > 0)
        {

            POINT pointtoinsert = Farthestpoint(result, out int insertionindex);
            result.Visitednodes.Insert(insertionindex + 1, pointtoinsert);
            result.Unvisitednodes.Remove(pointtoinsert);

        }
        //result.Distancecalc();
        return result;
    }
    private static POINT Farthestpoint(Tour tour, out int index)
    {

        double nearestinsertioncost = double.MaxValue;
        index = 0;
        POINT result = new POINT();
        POINT pointtoinsert = new POINT();
        double distance_totour;
        double distance_chosen = 0;
        for (int i = 0; i < tour.Visitednodes.Count - 1; i++) //find which to insert, disregard last as its the same as first
        {
            POINT pOINT = tour.Visitednodes[i];


            for (int j = 0; j < tour.Unvisitednodes.Count; j++)
            {
                distance_totour = Euclideandistance(pOINT, tour.Unvisitednodes[j]); //
                if (distance_totour > distance_chosen) //find farthest point to the entire tour, for each i and j and compare.
                {
                    pointtoinsert = tour.Unvisitednodes[j];
                    distance_chosen = distance_totour;
                }
            }
        }

        //two options from here, either chose the preceding or following vertex of i or look for the cheapest vertex to insert j (probably the same outcome)
        double distance1 = 0, distance2 = 0, distanceintour = 0;
        for (int k = 0; k < tour.Visitednodes.Count; k++) //find where to insert
        {

            POINT before = tour.Visitednodes[k];
            POINT after = null;
            if (k == tour.Visitednodes.Count - 1) { after = tour.Visitednodes[1]; } else { after = tour.Visitednodes[k + 1]; }// assume last node depot, first node depot, so we need the first node after the depot  ||| count would return 48, last index is 48 because we added the startingindex at the end         

            distance1 = Euclideandistance(before, pointtoinsert);
            distance2 = Euclideandistance(after, pointtoinsert);
            distanceintour = Euclideandistance(before, after);
            double insertioncost = distance1 + distance2 - distanceintour;
            if (insertioncost < nearestinsertioncost)
            {
                nearestinsertioncost = insertioncost;
                index = k;
            }
        }



        result = pointtoinsert;



        return result;
    }



    public static double Euclideandistance(POINT p1, POINT p2)
    {


        double result = Math.Sqrt(Math.Pow(p1.Xcoordinate - p2.Xcoordinate, 2) +
          Math.Pow(p1.Ycoordinate - p2.Ycoordinate, 2));

        return result;
    }
    public static double[,] distancematrix(List<POINT> inputlist)
    {
        double[,] dmatrix = new double[inputlist.Count, inputlist.Count];
        for (int i = 0; i < inputlist.Count; i++)
        {
            for (int j = 0; j < inputlist.Count; j++)
            {
                dmatrix[i, j] = Euclideandistance(inputlist[i], inputlist[j]);
            }
        }


        return dmatrix;




    }
    public static List<Tour> CreateGianttour_XY(List<POINT> inputlist)
    {
        List<Tour> Gianttours = new List<Tour>();

        for (int i = 0; i < inputlist.Count; i++)
        {
            Tour t1 = Farthestinsertion(inputlist.ToList(), i); t1.Tourlog.Add("Created by: Farthestinsertion", $" with distance {t1.Distance}");
            Tour t2 = Cheapestinsertion(inputlist.ToList(), i); t2.Tourlog.Add("Created by: Cheapestinsertion", $" with distance {t2.Distance}");
            Tour t3 = Nearestinsertion(inputlist.ToList(), i); t3.Tourlog.Add("Created by: Nearestinsertion", $" with distance {t3.Distance}");
            Tour t4 = Randominsertion(inputlist.ToList(), i); t4.Tourlog.Add("Created by: Randominsertion", $" with distance {t4.Distance}");
            Gianttours.AddRange(new List<Tour>() { t1, t2, t3, t4 });
        }





        return Gianttours;
    }
}






