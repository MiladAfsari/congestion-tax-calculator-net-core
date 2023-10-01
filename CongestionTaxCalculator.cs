using congestion.calculator;
using congestion.calculator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

public class CongestionTaxCalculator
{
    private List<Tuple<DateTime, int>> dailyTolls;
    public const int MAX_TOLL_FEE = 60;
    public CongestionTaxCalculator()
    {
        dailyTolls = new List<Tuple<DateTime, int>>();
    }
    /**
         * Calculate the total toll fee for one day
         *
         * @param vehicle - the vehicle
         * @param dates   - date and time of all passes on one day
         * @return - the total congestion tax for that day
         */
    public int GetTax(Vehicle vehicle, DateTime[] dates)
    {
        if (vehicle == null || dates == null || dates.Length == 0)
        {
            throw new ArgumentException("Vehicle and dates cannot be null or empty");
        }

        Array.Sort(dates);
        DateTime intervalStart = dates[0];
        int totalTollFee = 0;

        foreach (DateTime currentDateTime in dates)
        {
            int currentTollFee = GetTollFee(currentDateTime, vehicle);
            int intervalStartTollFee = GetTollFee(intervalStart, vehicle);

            double minutesDifference = (currentDateTime - intervalStart).TotalMinutes;

            if (minutesDifference <= 60)
            {
                if (totalTollFee > 0) totalTollFee -= intervalStartTollFee;
                if (currentTollFee >= intervalStartTollFee) intervalStartTollFee = currentTollFee;
                totalTollFee += intervalStartTollFee;
            }
            else
            {
                totalTollFee += currentTollFee;
                intervalStart = currentDateTime;
            }
        }

        if (totalTollFee > MAX_TOLL_FEE) totalTollFee = MAX_TOLL_FEE;
        return totalTollFee;
    }
    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        int toll = CalculateToll(date);

        toll = AdjustForLastToll(date, toll);

        int totalToll = dailyTolls.Sum(t => t.Item2);

        return totalToll >= 60 ? 60 : totalToll;
    }
    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        String vehicleType = vehicle.GetVehicleType();

        return vehicleType.Equals(TollFreeVehicles.Motorcycle.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Buss.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Military.ToString());
    }
    private Boolean IsTollFreeDate(DateTime date)
    {
        // check if date is weekend
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;

        // check if year is not 2013
        if (date.Year != 2013)
            return false;

        // check if date is toll free based on month and day
        switch (date.Month)
        {
            case 1:
                return date.Day == 1 || date.Day == 2;
            case 3:
                return date.Day >= 27 && date.Day <= 29;
            case 4:
                return date.Day == 1 || date.Day == 29 || date.Day == 30;
            case 5:
                return date.Day >= 1 && date.Day <= 9;
            case 6:
                return (date.Day >= 4 && date.Day <= 6) || date.Day == 20 || date.Day == 21;
            case 7:
                return true; // all July is toll free
            case 11:
                return date.Day == 1 || date.Day == 2;
            case 12:
                return (date.Day >= 23 && date.Day <= 26) || date.Day == 30 || date.Day == 31;
            default:
                return false;
        }
    }

    #region Private Methods
    private int CalculateToll(DateTime date)
    {
        int hour = date.Hour;
        int minute = date.Minute;

        if (hour == 6 && minute <= 29) return 8;
        if (hour == 6 || (hour == 7 && minute <= 59)) return 13;
        if (hour == 8 && minute <= 29) return 18;
        if ((hour == 8 && minute >= 30) || (hour >= 9 && hour < 15)) return 13;
        if (hour == 15 && minute <= 29) return 8;
        if ((hour == 15 && minute >= 30) || (hour == 16 && minute <= 59)) return 13;
        if (hour == 17 || (hour == 18 && minute <= 29)) return 18;
        if (hour == 18 && minute >= 30) return 13;

        return 0;
    }
    private int AdjustForLastToll(DateTime date, int toll)
    {
        if (dailyTolls.Any() && (date - dailyTolls.Last().Item1).TotalMinutes <= 60)
        {
            int lastToll = dailyTolls.Last().Item2;
            if (toll > lastToll)
            {
                dailyTolls.Add(new Tuple<DateTime, int>(date, toll - lastToll));
                return toll - lastToll;
            }
        }
        else
        {
            dailyTolls.Add(new Tuple<DateTime, int>(date, toll));
        }

        return toll;
    }
    #endregion
}