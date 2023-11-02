﻿using System;
using System.Collections.Generic;
namespace CyberScope.Automator.Providers
{
    public class KeyLengthSortedDecendingDictionary : SortedDictionary<string, string>
    {
        private class StringLengthComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (y == null) throw new ArgumentNullException(nameof(y));
                var lengthComparison = x.Length.CompareTo(y.Length) * -1;
                return lengthComparison == 0 ? string.Compare(x, y, StringComparison.Ordinal) : lengthComparison;
            }
        }
        public KeyLengthSortedDecendingDictionary() : base(new StringLengthComparer()) { }
    }
}