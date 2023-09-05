using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TubleBot
{
    public partial class Form1 : Form
    {

        public List<Node> nodes = new List<Node>();
        public Form1()
        {
            string[] txt = File.ReadAllLines("tubeStationsZonesAndAdjacencies.txt");
            nodes = getNodesFromData(txt);

            if (File.Exists("output.txt"))
            {
                string[] lines = File.ReadAllLines("output.txt");

                foreach (string l in lines)
                {
                    string[] splitLine = l.Split(',');
                    Node n = Utility.getNodeWithName(nodes, splitLine[0].Trim().ToUpper());
                    if (n != null)
                    {
                        n.avgScore = double.Parse(splitLine[1]);
                    }
                }

            }

            InitializeComponent();
        }

        private void trainButton_Click(object sender, EventArgs e)
        {
            while (true) {
                for (int i = (int)numericUpDown1.Value; i < nodes.Count; i++) {

                    Node origin = nodes[i];

                    //for each node, run it against every possible goal

                    for (int g = 0; g < nodes.Count; g++) {
                        Node goal = nodes[g];
                        runTuble(origin, goal);
                    }

                    origin.avgScore /= origin.numScoresFactoredIntoAvg;

                    StringBuilder sb = new StringBuilder();

                    foreach (Node n in nodes)
                    {

                        sb.Append(n.name);
                        sb.Append(",\t");
                        sb.Append(n.avgScore);
                        sb.Append("\n");
                    }

                    File.WriteAllText("output.txt", sb.ToString());
                }
            }
            //Console.WriteLine();
        }


        public void runTuble(Node origin, Node goal) {

            List<Node> tubleFeasibleNodes = new List<Node>(nodes);

            Random rnd = new Random();
  
            Node subOrigin = origin;

            int round = 1;

           // MessageBox.Show("(It doesn't know it, but it's looking for " + goal.name+")");
           // MessageBox.Show(tubleFeasibleNodes.Count + " possibilities.\nTesting " + subOrigin.name);

            while (tubleFeasibleNodes.Count > 1) {

                
                if (subOrigin == goal) {
                    tubleFeasibleNodes = new List<Node> { goal };
                    continue;
                }

                //how many zones away from the goal is it

                double zoneDifference = Math.Abs(goal.zone - subOrigin.zone);

                if (zoneDifference >= 2) {
                    zoneDifference = 3; //more than two zones away
                }

                for (int i = tubleFeasibleNodes.Count - 1; i >= 0; i--)
                {
                    double testZoneDifference = Math.Abs(tubleFeasibleNodes[i].zone - subOrigin.zone);

                    if (testZoneDifference >= 2)
                    {
                        testZoneDifference = 3; //more than two zones away
                    }

                    if (testZoneDifference != zoneDifference) {
                        tubleFeasibleNodes.RemoveAt(i);
                    }

                }


                //we check the distance between it and the goal

                double dist = runDijkstra(nodes, subOrigin, goal);

                //then eliminate all possibilities that are not that distance away from us

                double[] dists = new double[tubleFeasibleNodes.Count];

                for (int i = tubleFeasibleNodes.Count-1; i >= 0; i--)
                {
                    double testDist = runDijkstra(nodes, subOrigin, tubleFeasibleNodes[i]);
                    if (testDist != dist) {
                        tubleFeasibleNodes.RemoveAt(i);
                    }
                }

                subOrigin = tubleFeasibleNodes[rnd.Next(0, tubleFeasibleNodes.Count)];
                //MessageBox.Show(tubleFeasibleNodes.Count + " possibilities.\nTesting " + subOrigin.name);

                round++;
            }            

            //update the scores of the node we started at to get this result

            origin.numScoresFactoredIntoAvg++;
            origin.avgScore += (double)round;
        }


        public List<Node> getNodesFromData(string[] input)
        {
            List<Node> nodeList = new List<Node>();

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != null)
                    {
                    Link newLink = new Link(input[i], nodeList, false, false);
                    }
            }

            return nodeList;
        }

        public class DijkstraContext
        {
            public Node node;

            public DijkstraContext(Node _node)
            {
                node = _node;
            }
        }

        private double runDijkstra(List<Node> nodes, Node originNode, Node goalNode)
        {
            //string report = "";

            foreach (Node n in nodes) {
                n.clean();
            }

            DijkstraContext[] dijkstraNodes = new DijkstraContext[nodes.Count];

            for (int i = 0; i < dijkstraNodes.Length; i++) {
                dijkstraNodes[i] = new DijkstraContext(nodes[i]);
            }


            int origin = nodes.IndexOf(originNode);
            int goal = nodes.IndexOf(goalNode);

            int currentIndex = origin;
            dijkstraNodes[origin].node.cumuCosts.Add(0);
            dijkstraNodes[origin].node.trueCumuCost = 0;

           // MessageBox.Show("Origin: " + nodes[origin].name + "\nGoal: " + nodes[goal].name);
            bool verbose = false;

            while (currentIndex != dijkstraNodes.Length)
            {

                foreach (DijkstraContext node in dijkstraNodes)
                {
                    if (node.node.trueCumuCost == double.PositiveInfinity || node.node.trueCumuCost == 0)
                    {
                        if (node.node.trueCumuCost == 0)
                        {
                        }
                        else if (node.node.precedingLinks.Count == 0)
                        {
                        }
                        else
                        {
                            double lowest = double.MaxValue;
                            int indexOfLowest = 0;

                            for (int i = 0; i < node.node.cumuCosts.Count; i++)
                            {
                                if (node.node.cumuCosts[i] < lowest)
                                {
                                    lowest = node.node.cumuCosts[i];
                                    indexOfLowest = i;
                                }
                            }
                        }
                    }
                }

                if (currentIndex == int.MaxValue)
                {
                    break;
                }

                if (verbose)
                {
                    //report += "Evaluating node " + dijkstraNodes[currentIndex].node.name + "\n";

                    Console.WriteLine("Evaluating node " + dijkstraNodes[currentIndex].node.name);
                }

                dijkstraNodes[currentIndex].node.evaluated = true;

                if (currentIndex != origin)
                {
                    double smallest = double.PositiveInfinity;
                    Link theLinkWeJustTravelledDown = null;

                        foreach (Link preceding in dijkstraNodes[currentIndex].node.precedingLinks)
                        {
                            //we must have travelled down the smallest untravelled preceding link available at the current time. So find it.

                            if (!preceding.travelled && preceding.myNode.trueCumuCost + preceding.cost < smallest)
                            {
                                theLinkWeJustTravelledDown = preceding;
                                smallest = preceding.myNode.trueCumuCost + preceding.cost;
                            }
                        }
                    
                    theLinkWeJustTravelledDown.travelled = true;

                    //make sure the link's counterpart is also marked as being travelled so that we don't accidentally do it again

                    foreach (Link checkForDuplicate in theLinkWeJustTravelledDown.otherNode.links)
                    {
                        if (checkForDuplicate.otherNode == theLinkWeJustTravelledDown.myNode && checkForDuplicate.cost == theLinkWeJustTravelledDown.cost)
                        {
                            checkForDuplicate.invalidForTravel = true;
                        }
                    }

                    dijkstraNodes[currentIndex].node.trueCumuCost = smallest;
                    dijkstraNodes[currentIndex].node.truePrecedingLink = theLinkWeJustTravelledDown;
                }


                //explore links of this node, and remember the cumucosts it would take to get to their destination nodes

                foreach (Link l in dijkstraNodes[currentIndex].node.links)
                {

                    if (!l.otherNode.evaluated)
                    {
                        double potentialCost = dijkstraNodes[currentIndex].node.trueCumuCost + l.cost;
                        l.otherNode.cumuCosts.Add(potentialCost);
                        l.otherNode.precedingLinks.Add(l);
                        if (verbose)
                        {
                            Console.WriteLine("Telling otherNode " + l.otherNode.name + " that its cumucost from here is " + potentialCost);
                        }
                    }
                }

                if (currentIndex == goal)
                {
                    currentIndex = int.MaxValue;
                    continue;
                }

                double valueOfLowestFrontierNode = 999999;
                int indexOfLowestFrontierNode = goal;

                for (int i = 0; i < dijkstraNodes.Length; i++)
                {
                    if (!dijkstraNodes[i].node.evaluated)
                    {
                        for (int j = 0; j < dijkstraNodes[i].node.cumuCosts.Count; j++)
                        {
                            double testCost = dijkstraNodes[i].node.cumuCosts[j];

                            if (testCost != double.PositiveInfinity && testCost < valueOfLowestFrontierNode)
                            {
                                valueOfLowestFrontierNode = testCost;
                                indexOfLowestFrontierNode = i;
                            }
                        }
                    }
                }

                currentIndex = indexOfLowestFrontierNode;
            }

            //report += "\nPath is as follows (but in reverse):";

            Node curNode = dijkstraNodes[goal].node;

            while (curNode != dijkstraNodes[origin].node)
            {
                //report += curNode.name + " -> ";
                curNode = curNode.truePrecedingLink.myNode;
            }

            //report += dijkstraNodes[origin].node.name;

            //report += "\nThe total cost (to " + dijkstraNodes[goal].node.name + ") was: " + dijkstraNodes[goal].node.trueCumuCost;

         
            return dijkstraNodes[goal].node.trueCumuCost;
        }

        public class JarnikPrimContext
        {

            public double curDist = double.PositiveInfinity;
            public Node precedingNode = null;
            public bool addedToTree = false;
        }

        public string runJarnikPrim(List<Node> nodes)
        {

            string report = "";

            DijkstraContext[] dijkstraNodes = new DijkstraContext[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                dijkstraNodes[i] = new DijkstraContext(nodes[i]);
            }

            int currentIndex = 0;
            dijkstraNodes[0].node.cumuCosts.Add(0);
            dijkstraNodes[0].node.trueCumuCost = 0;

            string nodesInOrderOfEvaluation = "";


            /*
            string DijkstraChart = "";

            foreach (DijkstraContext node in dijkstraNodes)
            {
                DijkstraChart += node.node.name + "\t";
            }

            DijkstraChart += "\n\n";
            */

            int numNodesEvaluated = 0;

            List<Link> linksInOrderOfEvaluationMoreOrLess = new List<Link>();

            while (numNodesEvaluated < nodes.Count)
            {
                /*
                foreach (DijkstraContext node in dijkstraNodes)
                {
                    if (node.node.trueCumuCost == double.PositiveInfinity || node.node.trueCumuCost == 0)
                    {
                        if (node.node.trueCumuCost == 0)
                        {
                            DijkstraChart += "0," + node.node.name;
                        }
                        else if (node.node.precedingLinks.Count == 0)
                        {
                            DijkstraChart += "∞," + node.node.name;
                        }
                        else
                        {
                            double lowest = double.MaxValue;
                            int indexOfLowest = 0;

                            for (int i = 0; i < node.node.cumuCosts.Count; i++)
                            {
                                if (node.node.cumuCosts[i] < lowest)
                                {
                                    lowest = node.node.cumuCosts[i];
                                    indexOfLowest = i;
                                }
                            }

                            DijkstraChart += lowest + "," + node.node.precedingLinks[indexOfLowest].myNode.name;
                        }
                    }
                    else
                    {
                        DijkstraChart += node.node.trueCumuCost + "," + node.node.truePrecedingLink.myNode.name;
                    }

                    if (node.node.evaluated)
                    {
                        DijkstraChart += "✔";
                    }

                    DijkstraChart += "\t";
                }

                DijkstraChart += "\n";?

                */

                report += "Evaluating node " + dijkstraNodes[currentIndex].node.name + "\n";

                Console.WriteLine("Evaluating node " + dijkstraNodes[currentIndex].node.name);

                dijkstraNodes[currentIndex].node.evaluated = true;
                numNodesEvaluated++;

                nodesInOrderOfEvaluation += dijkstraNodes[currentIndex].node.name + ", ";

                if (currentIndex > 0)
                {
                    double smallest = double.PositiveInfinity;
                    Link theLinkWeJustTravelledDown = null;

                    foreach (Link preceding in dijkstraNodes[currentIndex].node.precedingLinks)
                    {
                        //we must have travelled down the smallest untravelled preceding link available at the current time. So find it.

                        if (!preceding.travelled && !preceding.invalidForTravel && preceding.cost < smallest)
                        {
                            theLinkWeJustTravelledDown = preceding;
                            smallest = preceding.cost;
                        }
                    }

                    theLinkWeJustTravelledDown.travelled = true;
                    linksInOrderOfEvaluationMoreOrLess.Add(theLinkWeJustTravelledDown);

                    Console.WriteLine("We must have travelled down the link " + theLinkWeJustTravelledDown.myNode.name + "->" + theLinkWeJustTravelledDown.otherNode.name);

                    //make sure the link's counterpart is also marked as being travelled so that we don't accidentally do it again

                    foreach (Link checkForDuplicate in theLinkWeJustTravelledDown.otherNode.links)
                    {
                        if (checkForDuplicate.otherNode == theLinkWeJustTravelledDown.myNode && checkForDuplicate.cost == theLinkWeJustTravelledDown.cost)
                        {
                            if (checkForDuplicate.travelled)
                            {
                                theLinkWeJustTravelledDown.invalidForTravel = true;
                            }
                            else
                            {
                                checkForDuplicate.invalidForTravel = true;
                            }

                        }
                    }

                    dijkstraNodes[currentIndex].node.trueCumuCost = smallest;
                    dijkstraNodes[currentIndex].node.truePrecedingLink = theLinkWeJustTravelledDown;
                }


                //explore links of this node, and remember the cumucosts it would take to get to their destination nodes

                foreach (Link l in dijkstraNodes[currentIndex].node.links)
                {
                    if (!l.otherNode.evaluated)
                    {
                        double potentialCost = l.cost;
                        l.otherNode.cumuCosts.Add(potentialCost);
                        l.otherNode.precedingLinks.Add(l);

                        foreach (Link checkForDuplicate in l.otherNode.links)
                        {
                            if (checkForDuplicate.otherNode == l.myNode && checkForDuplicate.cost == l.cost)
                            {
                                if (checkForDuplicate.travelled)
                                {
                                    l.invalidForTravel = true;
                                }
                                else
                                {
                                    checkForDuplicate.invalidForTravel = true;
                                }

                            }
                        }

                            Console.WriteLine("Telling otherNode " + l.otherNode.name + " that its cumucost from here is " + potentialCost);
                    }
                }

                double valueOfLowestFrontierNode = 999999;
                int indexOfLowestFrontierNode = dijkstraNodes.Length - 1;

                for (int i = 0; i < dijkstraNodes.Length; i++)
                {
                    if (!dijkstraNodes[i].node.evaluated)
                    {
                        foreach (double potentialCumuCost in dijkstraNodes[i].node.cumuCosts)
                        {
                            if (potentialCumuCost != double.PositiveInfinity && potentialCumuCost < valueOfLowestFrontierNode)
                            {
                                valueOfLowestFrontierNode = potentialCumuCost;
                                indexOfLowestFrontierNode = i;
                            }
                        }
                    }
                }

                currentIndex = indexOfLowestFrontierNode;
            }

            report += "\nNodes were evaluated in this order:\n";

            report += nodesInOrderOfEvaluation;

            double totalCost = 0;

            report += "\nThe highlighted links are:";

            foreach (Link l in linksInOrderOfEvaluationMoreOrLess)
            {
                if (l.travelled && !l.invalidForTravel)
                {
                    report += "\n" + l.myNode.name + "->" + l.otherNode.name;
                    totalCost += l.cost;
                }
            }

            report += "\nThe total cost was: " + totalCost;


            /*
            report += "\n\nAnd here is the dijkstra chart:\n";

            report += DijkstraChart;

            report += "\nAnd there should be an extra column known as 'fin', which documents the most recently ticked node in that row.";
            */

            return report;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
