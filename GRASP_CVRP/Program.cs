

using GRASP_CVRP;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
namespace GRASP_CVRP;

class Program
{
    private static void Main(string[] args)
    {
        string path = @"C:\classes\ws22-23\vigo\E-n51-k5.vrp";
        //@"C:\classes\ws22-23\vigo\vrp\M\M-n101-k10.vrp";
        //@"C:\classes\ws22-23\vigo\E-n51-k5.vrp"; //enter your filepath

        SortedList<double, List<Tour>> results = new SortedList<double, List<Tour>>();
        List<double> averagelist = new List<double>();
        double bestval = double.MaxValue;

        for (int k = 0; k < 150; k++)
        {
            FileHandler fileHandler = new FileHandler(path, string.Empty);
            //Console.WriteLine(fileHandler.Plist.Count);
            List<POINT> points = fileHandler.Plist.ToList();
            GRASP_S graspalgo = new GRASP_S(points, fileHandler.Maxcap);
            graspalgo.Mainalgo();
            double outputval = graspalgo.Finalsolution.Totaldist;
            Console.WriteLine($"the value is {outputval}");
            averagelist.Add(outputval);
            if (outputval < bestval)
            {
                bestval = outputval;

            }
            //List<POINT> inputtest = fileHandler.Plist.ToList();
            graspalgo.Resetall();




        }
        Console.WriteLine($"The average is {averagelist.Average()} ; the best ist {bestval}");



    }
}

