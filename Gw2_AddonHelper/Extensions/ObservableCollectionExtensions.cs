﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Extensions
{
    public static class ObservableCollectionExtensions
    {

        public static void AddRange<T>(this ObservableCollection<T> obsCollection, IEnumerable<T> items)
        {
            if (obsCollection == null) throw new ArgumentNullException("obsCollection");
            if (items == null) throw new ArgumentNullException("items");

            foreach (var item in items) obsCollection.Add(item);
        }
    }
}