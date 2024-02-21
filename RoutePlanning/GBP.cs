using System.Diagnostics.CodeAnalysis;
using UnitsNet;

namespace RoutePlanning
{
    public enum GBPUnit
    {
        Pound,
        Pence
    }

    public struct GBP : IEquatable<GBP>
    {
        public static GBPUnit BaseUnit { get; }

        static GBP()
        {
            BaseUnit = GBPUnit.Pound;
        }

        public GBP()
        {
            Value = 0.0;
            Unit = GBPUnit.Pound;
        }


        public GBP(double value, GBPUnit unit)
        {
            Value = value;
            Unit = unit;
        }

        public static GBP From(double value, GBPUnit unit)
        {
            return new GBP(value, unit);
        }

        public static GBP FromPence(double value)
        {
            return From(value, GBPUnit.Pence);
        }

        public static GBP FromPound(double value)
        {
            return From(value, GBPUnit.Pound);
        }

        public double Value { get; }

        public GBPUnit Unit { get; }


        public double As(GBPUnit unit)
        {
            return ToUnit(unit).Value;
        }

        public GBP ToUnit(GBPUnit unit)
        {
            if (TryToUnit(unit, out var value))
            {
                return value!.Value;
            }
            else if (Unit != BaseUnit)
            {
                var inBaseUnits = ToUnit(BaseUnit);
                return inBaseUnits.ToUnit(unit);
            }
            else
            {
                // No possible conversion
                throw new NotImplementedException($"Can not convert {Unit} to {unit}.");
            }
        }


        public static GBP operator +(GBP gbp)
        {
            return new GBP(gbp.Value, gbp.Unit);
        }

        public static GBP operator -(GBP gbp)
        {
            return new GBP(-gbp.Value, gbp.Unit);
        }


        public static GBP operator +(GBP gbp1, GBP gbp2)
        {
            return new GBP(gbp1.Value + gbp2.As(gbp1.Unit), gbp1.Unit);
        }

        public static GBP operator -(GBP gbp1, GBP gbp2)
        {
            return new GBP(gbp1.Value - gbp2.As(gbp1.Unit), gbp1.Unit);
        }

        public static GBP operator *(double m1, GBP gBP)
        {
            return new GBP(gBP.Value * m1, gBP.Unit);
        }

        public static GBP operator *(GBP gBP, double m1)
        {
            return new GBP(gBP.Value * m1, gBP.Unit);
        }

        public static GBP operator /(GBP gBP, double m1)
        {
            return new GBP(gBP.Value / m1, gBP.Unit);
        }


        /// <summary>
        ///     Attempts to convert this <see cref="Luminosity"/> to another <see cref="Luminosity"/> with the unit representation <paramref name="unit" />.
        /// </summary>
        /// <param name="unit">The unit to convert to.</param>
        /// <param name="converted">The converted <see cref="Luminosity"/> in <paramref name="unit"/>, if successful.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool TryToUnit(GBPUnit unit, [NotNullWhen(true)] out GBP? converted)
        {
            if (Unit == unit)
            {
                converted = this;
                return true;
            }

            GBP? convertedOrNull = (Unit, unit) switch
            {
                // LuminosityUnit -> BaseUnit
                (GBPUnit.Pence, GBPUnit.Pound) => new GBP(Value / 100, GBPUnit.Pound),

                // BaseUnit -> LuminosityUnit
                (GBPUnit.Pound, GBPUnit.Pence) => new GBP(Value * 100, GBPUnit.Pence),

                _ => null
            };

            if (convertedOrNull is null)
            {
                converted = default;
                return false;
            }

            converted = convertedOrNull.Value;
            return true;
        }

        public override string ToString()
        {
            return ToString("N3");
        }

        public string ToString(string format)
        {
            return $"£{As(GBPUnit.Pound).ToString(format)}";
        }

        public bool Equals(GBP other)
        {
            return As(BaseUnit).Equals(other.As(BaseUnit));
        }
    }
}
