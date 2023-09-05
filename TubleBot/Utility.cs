using System;
using System.Collections.Generic;

namespace TubleBot
{
    public static class Utility
    {
        internal static Node getNodeWithName(List<Node> nodes, string nameToFind)
        {
            foreach (Node n in nodes)
            {
                if (n.name == nameToFind)
                {
                    return n;
                }
            }

            return null;
        }

        internal static bool listContainsNodeWithName(List<Node> nodes, string nameToFind)
        {
            foreach (Node n in nodes)
            {
                if (n.name == nameToFind)
                {
                    return true;
                }
            }

            return false;
        }


        public static bool isAString(double d)
        {

            if (d == double.NegativeInfinity)
            {
                return true;
            }

            return false;
        }
    }
}
