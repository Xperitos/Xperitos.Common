using System;
using System.Collections.Generic;
using System.Linq;

namespace Xperitos.Common.Utils
{
	/// <summary>
	/// Histogram Bucket
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Bucket
	{
		public double Tolerance { get; private set; }
		public List<double> Items { get; private set; } = new List<double>();
		public double Average { get; set; }

		public Bucket(double tolerance)
		{
			Tolerance = tolerance;
		}
		
		/// <summary>
		/// Add Item to bucket if fit into tolerance
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryAddItem(double item)
		{
			if (!IsItemFitBucket(item))
				return false;
			
			Items.Add(item);
			Average = Items.Average();

			return true;
		}

		/// <summary>
		/// Check if adding the item to the bucket fits within bucket items
		/// average +/- tolerance
		/// </summary>
		/// <param name="item"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		private bool IsItemFitBucket(double item)
		{
			double newAvg = (Average + item) / (Items.Count + 1);
			if (Math.Abs(item - newAvg) <= Tolerance)
				return true;
			return false;
		}
	}
	
	public class Histogram
	{
		/// <summary>
		/// The tolerance of items in the bucket +/- around the bucket average
		/// </summary>
		public double BucketTolerance { get; set; }

		/// <summary>
		/// Histogram Items
		/// </summary>
		private List<double> m_items = new List<double>();
		public List<double> Items { 
			get => m_items;
			set => SetItems(value);
		}

		public List<Bucket> Buckets { get; private set; } = new List<Bucket>();


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="bucketTolerance"></param>
		public Histogram(double bucketTolerance)
		{
			BucketTolerance = bucketTolerance;
		}
		
		private void SetItems(List<double> items)
		{
			m_items = items;
			m_items.Sort();

			// Find per item number of repetitions
			var itemsMass = m_items.Distinct().Select(i => new Tuple<double, int>(i, m_items.Count(j => Math.Abs(j - i) < 10e-5))).ToList();

			// Order the items according to their mass - should improve histogram buckets creation
			if (m_items.Count > 1)
				m_items = m_items.OrderByDescending(i => itemsMass.First(j => j.Item1 == i).Item2).ToList();

			foreach (var item in m_items)
			{
				FindBucket(item);
			}
		}

		private void FindBucket(double item)
		{
			// Try to add the item into one of the existing buckets
			// if not succeeded then create a new bucket
			if (!Buckets.Any(i => i.TryAddItem(item)))
			{
				// Add a new Bucket
				Bucket b = new Bucket(BucketTolerance);
				b.TryAddItem(item);
				Buckets.Add(b);
			}
		}
	}
}