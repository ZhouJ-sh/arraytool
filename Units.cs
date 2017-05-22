using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MapCloudOffice3.Common
{
    public class Unit
    {
        public override string ToString()
        {
            return Label;
        }
        public string Label { get; set; }
        public double Factor { get; set; }
    }

    public class UnitConverter : IMultiValueConverter
    {
        public UnitConverter()
        {

        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (2 == values.Length && values[0] is double && values[1] is Unit)
            {
                Unit selected = values[1] as Unit;
                Selected = selected;
                return UnitEnumerable.Convert((double)values[0], selected);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            double outValue = 0.0;
            if (value is string)
            {
                if (double.TryParse((string)value, out outValue))
                {
                    outValue *= Selected.Factor;
                }
            }
            else if (value is double)
            {
                outValue = (double)value * Selected.Factor;
            }
            return new object[2] { outValue, Selected };
        }

        public Unit Selected { get; set; }

    }

    public class UnitEnumerable : IEnumerable<Unit>
    { 
        public static double Convert(double v, Unit u)
        {
            return v / u.Factor;
        }

        private void CollectInfo()
        {
            if (mUnitsInfo.Count != mUnits.Count)
            {
                foreach (var u in mUnits)
                {
                    mUnitsInfo.Add(new Tuple<double, Unit>(GetPower(u.Factor), u));
                }
                mUnitsInfo.Sort((x, y) => (int)(1000 * (y.Item1 - x.Item1)));
            }
        }

        private double GetPower(double v)
        {
            var log10 = Math.Log10(v);
            if (double.IsInfinity(log10))
            {
                log10 = 0;
            }
            return log10;
        }

        public Unit ChooseUnit(double v)
        {
            CollectInfo();
            using (var unit = mUnitsInfo.GetEnumerator())
            {
                var hasNext = unit.MoveNext();
                var power = GetPower(v);
                while (unit.Current.Item1 > power && hasNext)
                {
                    hasNext = unit.MoveNext();
                }

                return unit.Current.Item2;
            }
        }

        public IEnumerator<Unit> GetEnumerator()
        {
            return mUnits.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mUnits.GetEnumerator();
        }

        private List<Tuple<double, Unit>> mUnitsInfo = new List<Tuple<double, Unit>>();
        private List<Unit> mUnits = new List<Unit>();
        public List<Unit> Units
        {
            get
            {
                return mUnits;
            }
        }
    }

}
