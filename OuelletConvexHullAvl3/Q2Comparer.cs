﻿using System.Collections.Generic;
using System.Windows;

namespace OuelletConvexHullAvl3
{
	public class Q2Comparer : IComparer<Point>
	{
		public int Compare(Point pt1, Point pt2)
		{
			if (pt1.X > pt2.X) // Decreasing order
			{
				return -1;
			}
			if (pt1.X < pt2.X)
			{
				return 1;
			}

			return 0;
		}
	}
}
