﻿using System;
using Parcel.Toolbox.DataSource;

namespace Parcel.SetupTest.DataSourceTest001
{
    class Program
    {
        static void Main(string[] args)
        {
            DataSourceHelper.YahooFinance(new YahooFinanceParameter()
            {
                InputSymbol = "AAPL",
                // InputAPIKey = APIKey.YahooFinance // Yahoo REST API is useless compared to its website
            });
        }
    }
}