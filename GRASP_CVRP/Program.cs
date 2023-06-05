

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

        for (int k = 0; k < 1; k++)
        {
            FileHandler fileHandler = new FileHandler(path, string.Empty);
            //Console.WriteLine(fileHandler.Plist.Count);
            List<POINT> points = fileHandler.Plist.ToList();
            GRASP_S graspalgo = new GRASP_S(points, fileHandler.Maxcap);
            graspalgo.Mainalgo();
            //List<POINT> inputtest = fileHandler.Plist.ToList();



        }
        double avg = 0;
        foreach (double key in results.Keys)
        {
            avg += key;
            Console.Write($"[{results.IndexOfKey(key)}] Total_Dist: " + key + "\n");
            foreach (Tour t in results[key])
            {
                Console.WriteLine((results[key].IndexOf(t) + 1).ToString() + " " + t);
            }

        }
        Console.WriteLine($"Average Distance in this round: {avg / results.Keys.Count}");


    }
}

