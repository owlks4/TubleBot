using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubleBot
{
    public class Node
    {
        public string name;
        public List<Link> links = new List<Link>();
        public List<double> cumuCosts = new List<double>(); //one for each preceding link
        public double trueCumuCost = double.PositiveInfinity; //for once we've worked out which path was actually taken, rather than just potential paths
        public List<Link> precedingLinks = new List<Link>();
        public Link truePrecedingLink;
        public bool evaluated = false;
        public double zone;

        public double avgScore;
        public int numScoresFactoredIntoAvg;

        public void clean() {

            cumuCosts = new List<double>();
            trueCumuCost = double.PositiveInfinity; //for once we've worked out which path was actually taken, rather than just potential paths
            precedingLinks = new List<Link>();
            truePrecedingLink = null;
            evaluated = false;

            foreach (Link l in links) {
                l.clean();
            }
    }
        public Node(string _name)
        {
            name = _name;
        }
    }
}
