using System;
using System.Collections.Generic;

namespace Nostradabus.BusinessEntities.Common
{
    public class Range<T>
    {
		#region Constructors

		/// <summary>
		/// Constructs a new inclusive range using the default comparer
		/// </summary>
		public Range(T start, T end)
			: this(start, end, true, true, Comparer<T>.Default)
		{
		}

		/// <summary>
		/// Constructs a new range including or excluding each end as specified, using the default comparer
		/// </summary>
		public Range(T start, T end, bool includeStart, bool includeEnd)
			: this(start, end, includeStart, includeEnd, Comparer<T>.Default)
		{
		}

		/// <summary>
		/// Constructs a new range including both ends using the specified comparer
		/// </summary>
		public Range(T start, T end, IComparer<T> comparer)
			: this(start, end, true, true, comparer)
		{
		}

		/// <summary>
		/// Constructs a new range, including or excluding each end as specified,
		/// with the given comparer.
		/// </summary>
		public Range(T start, T end, bool includeStart, bool includeEnd, IComparer<T> comparer)
		{
			if (start != null && end != null && comparer.Compare(start, end) > 0)
			{
				throw new ArgumentOutOfRangeException("end", "start must be lower than end according to comparer");
			}

			this.Start = start;
			this.End = end;
			this.Comparer = comparer;
			this.IncludesStart = includeStart;
			this.IncludesEnd = includeEnd;
		}

		#endregion Constructors
		
		#region Properties

		/// <summary>
        /// The start of the range.
        /// </summary>
		public virtual T Start { get; set; }

        /// <summary>
        /// The end of the range.
        /// </summary>
		public virtual T End { get; set; }
        
        /// <summary>
        /// Comparer to use for comparisons
        /// </summary>
		public virtual IComparer<T> Comparer { get; set; }
        
        /// <summary>
        /// Whether or not this range includes the start point
        /// </summary>
		public virtual bool IncludesStart { get; set; }
        
        /// <summary>
        /// Whether or not this range includes the end point
        /// </summary>
		public virtual bool IncludesEnd { get; set; }


		#endregion Properties

		#region Methods
	
        /// <summary>
        /// Returns whether or not the range contains the given value
        /// </summary>
        public bool Contains(T value)
        {
            int lowerBound = Comparer.Compare(value, Start);
			lowerBound = (Start != null) ? lowerBound : 1;
			
        	if (lowerBound < 0 || (lowerBound == 0 && !IncludesStart)) return false;
            
            int upperBound = Comparer.Compare(value, End);
			upperBound = (End != null) ? upperBound : -1;
			
            return upperBound < 0 || (upperBound == 0 && IncludesEnd);
		}

#if DOTNET35
        /// <summary>
        /// Returns an iterator which begins at the start of this range,
        /// adding the given step amount to the current value each iteration until the 
        /// end is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of an addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to add on each iteration</param>
        public RangeIterator<T> UpBy(T stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.Add(t, stepAmount));
        }

        /// <summary>
        /// Returns an iterator which begins at the end of this range,
        /// subtracting the given step amount to the current value each iteration until the 
        /// start is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of a subtraction operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to subtract on each iteration. Note that
        /// this is subtracted, so in a range [0,10] you would pass +2 as this parameter
        /// to obtain the sequence (10, 8, 6, 4, 2, 0).
        /// </param>
        public RangeIterator<T> DownBy(T stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.Subtract(t, stepAmount), false);
        }

        /// <summary>
        /// Returns an iterator which begins at the start of this range,
        /// adding the given step amount to the current value each iteration until the 
        /// end is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of an addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to add on each iteration</param>
        public RangeIterator<T> UpBy<TAmount>(TAmount stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.AddAlternative(t, stepAmount));
        }

        /// <summary>
        /// Returns an iterator which begins at the end of this range,
        /// subtracting the given step amount to the current value each iteration until the 
        /// start is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of a subtraction operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to subtract on each iteration. Note that
        /// this is subtracted, so in a range [0,10] you would pass +2 as this parameter
        /// to obtain the sequence (10, 8, 6, 4, 2, 0).
        /// </param>
        public RangeIterator<T> DownBy<TAmount>(TAmount stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.SubtractAlternative(t, stepAmount), false);
        }
#endif

#if DOTNET35
        /// <summary>
        /// Returns an iterator which steps through the range, adding the specified amount
        /// on each iteration. If the step amount is logically negative, the returned iterator
        /// begins at the start point; otherwise it begins at the end point.
        /// This method does not check for
        /// the availability of an addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">The amount to add on each iteration</param>
        public RangeIterator<T> Step(T stepAmount)
        {
            return Step(t => Operator.Add(t, stepAmount));
        }

        /// <summary>
        /// Returns an iterator which steps through the range, adding the specified amount
        /// on each iteration. If the step amount is logically negative, the returned iterator
        /// begins at the end point; otherwise it begins at the start point. This method
        /// is equivalent to Step(T stepAmount), but allows an alternative type to be used.
        /// The most common example of this is likely to be stepping a range of DateTimes
        /// by a TimeSpan.
        /// This method does not check for
        /// the availability of a suitable addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">The amount to add on each iteration</param>
        public RangeIterator<T> Step<TAmount>(TAmount stepAmount)
        {
            return Step(t => Operator.AddAlternative(t, stepAmount));
        }
#endif
		
		#endregion Methods
	}
}
