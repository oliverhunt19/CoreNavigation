using UnitsNet;

namespace RoutePlanning
{
    public struct CostPerUnit<T_Unit>
        where T_Unit : IQuantity
    {

        private GBP UnitValue { get; }

        private Enum QuantityUnit { get; }



        public CostPerUnit(GBP value, T_Unit t_Unit)
        {
            QuantityUnit = t_Unit.Unit;
            UnitValue = value / (double)t_Unit.Value;
        }


        public GBP CostForQuanity(T_Unit unit)
        {
            double unitValue = unit.As(QuantityUnit);
            return UnitValue * unitValue;

        }
    }
}
