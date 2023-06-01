using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRASP_CVRP;




public static class Constructions
{

    //GRASP SECTION BEGINS

    public static List<Tour> GRASPcheapestinsertion(ref Random rnd, int capacity, int RCLlength, List<POINT> points, double[,] dmatrix)
    {
        List<Tour> output = new List<Tour>();
        int localcap = capacity;
        POINT depot = points.First();
        List<POINT> visited = new List<POINT> { depot };
        List<POINT> pointsremaining = points.Except(visited).ToList();

        while (pointsremaining.Count > 0)
        {
            output.Add(CheapestinsertionGRASP(ref rnd,RCLlength,ref pointsremaining,depot,capacity));
        }


        return output;
    }

    public static Tour CheapestinsertionGRASP(ref Random rnd, int RCLlenght, ref List<POINT> remainingpoints, POINT depot, int capacity)
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
            Dictionary<double, List<Tuple<POINT,int>>> RCL = new Dictionary<double, List<Tuple<POINT, int>>>();

            //choose point
            double choice = rnd.NextDouble();
            int index = 0;
            POINT chosen = new POINT();//Choose(choice, RCL, out int index)
           
            while (result.Tourload + chosen.Demand > capacity)
            {//choose new
                double newchoice = rnd.NextDouble();
                int newindex = 0;
                chosen = new POINT();//Choose(newchoice, RCL, out int newindex)
                nofit_iteration++;
                if(nofit_iteration == maxnofit) { returntour= true; break;}
            }


        }
        //while (result.Tourload < capacity)
        //{
        //    POINT pointtoinsert = Cheapestpoint(result, out int insertionindex); //ask about the out int index
        //    result.Visitednodes.Insert(insertionindex + 1, pointtoinsert); //insertion always pushes the existing value further in the list, we dont want to push the value at index but insert after it, therefore index+1
        //    result.Unvisitednodes.Remove(pointtoinsert);
        //}

        return result;

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






