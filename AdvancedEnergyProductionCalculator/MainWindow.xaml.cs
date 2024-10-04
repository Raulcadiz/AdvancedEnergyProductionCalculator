using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace AdvancedEnergyProductionCalculator
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, CountryEnergyData> countriesData;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCountryData();
            InitializeComboBox();
            InitializeChart();
        }

        private void InitializeCountryData()
        {
            countriesData = new Dictionary<string, CountryEnergyData>
            {
                {"Brasil", new CountryEnergyData(3000, 15000, 100000, 2000)},
                {"México", new CountryEnergyData(5000, 6000, 12000, 1000)},
                {"Chile", new CountryEnergyData(3000, 1500, 6500, 2500)},
                {"Argentina", new CountryEnergyData(1000, 2500, 11000, 500)},
                {"Colombia", new CountryEnergyData(100, 2000, 11800, 1000)},
                {"Perú", new CountryEnergyData(300, 400, 5000, 200)}
            };
        }

        private void InitializeComboBox()
        {
            foreach (var country in countriesData.Keys)
            {
                countryComboBox.Items.Add(country);
            }
        }

        private void InitializeChart()
        {
            productionChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Solar",
                    Values = new ChartValues<double> { 0, 0, 0 }
                },
                new ColumnSeries
                {
                    Title = "Eólica",
                    Values = new ChartValues<double> { 0, 0, 0 }
                },
                new ColumnSeries
                {
                    Title = "Hidroeléctrica",
                    Values = new ChartValues<double> { 0, 0, 0 }
                },
                new ColumnSeries
                {
                    Title = "Geotérmica",
                    Values = new ChartValues<double> { 0, 0, 0 }
                }
            };

            productionChart.AxisX.Add(new Axis
            {
                Title = "Periodo",
                Labels = new[] { "Día", "Mes", "Año" }
            });

            productionChart.AxisY.Add(new Axis
            {
                Title = "Producción (MWh)",
                LabelFormatter = value => value.ToString("N0")
            });
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (countryComboBox.SelectedItem == null || !DateTime.TryParse(datePicker.Text, out DateTime selectedDate))
            {
                MessageBox.Show("Por favor, seleccione un país y una fecha válida.");
                return;
            }

            string selectedCountry = countryComboBox.SelectedItem.ToString();
            var countryData = countriesData[selectedCountry];

            var dailyProduction = CalculateProduction(countryData, selectedDate, 1);
            var monthlyProduction = CalculateProduction(countryData, selectedDate, 30);
            var yearlyProduction = CalculateProduction(countryData, selectedDate, 365);

            UpdateResults(dailyProduction, monthlyProduction, yearlyProduction);
            UpdateChart(dailyProduction, monthlyProduction, yearlyProduction);
        }

        private EnergyProduction CalculateProduction(CountryEnergyData data, DateTime date, int days)
        {
            double solarProduction = CalculateSolarProduction(data.SolarCapacity, date, days);
            double windProduction = CalculateWindProduction(data.WindCapacity, date, days);
            double hydroProduction = CalculateHydroProduction(data.HydroCapacity, date, days);
            double geoProduction = CalculateGeoProduction(data.GeoCapacity, days);

            return new EnergyProduction(solarProduction, windProduction, hydroProduction, geoProduction);
        }

        private double CalculateSolarProduction(double capacity, DateTime date, int days)
        {
            double seasonalFactor = GetSeasonalFactor(date, "Solar");
            double dailyHours = 12 * seasonalFactor; // Simplified: assumes 12 hours of daylight adjusted by season
            return capacity * dailyHours * 0.2 * days; // 0.2 is a simplified efficiency factor
        }

        private double CalculateWindProduction(double capacity, DateTime date, int days)
        {
            double seasonalFactor = GetSeasonalFactor(date, "Wind");
            return capacity * 24 * 0.35 * seasonalFactor * days; // 0.35 is a simplified capacity factor
        }

        private double CalculateHydroProduction(double capacity, DateTime date, int days)
        {
            double seasonalFactor = GetSeasonalFactor(date, "Hydro");
            return capacity * 24 * 0.5 * seasonalFactor * days; // 0.5 is a simplified capacity factor
        }

        private double CalculateGeoProduction(double capacity, int days)
        {
            return capacity * 24 * 0.8 * days; // 0.8 is a simplified capacity factor for geothermal
        }

        private double GetSeasonalFactor(DateTime date, string energyType)
        {
            int month = date.Month;
            bool isSouthernHemisphere = true; // Adjust this based on the country

            if (isSouthernHemisphere)
            {
                if (month >= 12 || month <= 2) return energyType == "Solar" ? 1.3 : energyType == "Wind" ? 1.1 : 0.8; // Summer
                if (month >= 3 && month <= 5) return energyType == "Solar" ? 1.0 : energyType == "Wind" ? 1.2 : 1.0; // Fall
                if (month >= 6 && month <= 8) return energyType == "Solar" ? 0.7 : energyType == "Wind" ? 1.3 : 1.2; // Winter
                return energyType == "Solar" ? 1.1 : energyType == "Wind" ? 1.0 : 1.1; // Spring
            }
            else
            {
                if (month >= 12 || month <= 2) return energyType == "Solar" ? 0.7 : energyType == "Wind" ? 1.3 : 1.2; // Winter
                if (month >= 3 && month <= 5) return energyType == "Solar" ? 1.1 : energyType == "Wind" ? 1.0 : 1.1; // Spring
                if (month >= 6 && month <= 8) return energyType == "Solar" ? 1.3 : energyType == "Wind" ? 1.1 : 0.8; // Summer
                return energyType == "Solar" ? 1.0 : energyType == "Wind" ? 1.2 : 1.0; // Fall
            }
        }

        private void UpdateResults(EnergyProduction daily, EnergyProduction monthly, EnergyProduction yearly)
        {
            dayTextBlock.Text = $"Producción diaria: {daily.Total:N2} MWh";
            monthTextBlock.Text = $"Producción mensual: {monthly.Total:N2} MWh";
            yearTextBlock.Text = $"Producción anual: {yearly.Total:N2} MWh";
        }

        private void UpdateChart(EnergyProduction daily, EnergyProduction monthly, EnergyProduction yearly)
        {
            UpdateChartSeries(0, daily.Solar, monthly.Solar, yearly.Solar);
            UpdateChartSeries(1, daily.Wind, monthly.Wind, yearly.Wind);
            UpdateChartSeries(2, daily.Hydro, monthly.Hydro, yearly.Hydro);
            UpdateChartSeries(3, daily.Geo, monthly.Geo, yearly.Geo);
        }

        private void UpdateChartSeries(int seriesIndex, double daily, double monthly, double yearly)
        {
            if (productionChart.Series.ElementAtOrDefault(seriesIndex) is ColumnSeries series)
            {
                series.Values = new ChartValues<double> { daily, monthly, yearly };
            }
        }
    }

    public class CountryEnergyData
    {
        public double SolarCapacity { get; }
        public double WindCapacity { get; }
        public double HydroCapacity { get; }
        public double GeoCapacity { get; }

        public CountryEnergyData(double solar, double wind, double hydro, double geo)
        {
            SolarCapacity = solar;
            WindCapacity = wind;
            HydroCapacity = hydro;
            GeoCapacity = geo;
        }
    }

    public class EnergyProduction
    {
        public double Solar { get; }
        public double Wind { get; }
        public double Hydro { get; }
        public double Geo { get; }
        public double Total => Solar + Wind + Hydro + Geo;

        public EnergyProduction(double solar, double wind, double hydro, double geo)
        {
            Solar = solar;
            Wind = wind;
            Hydro = hydro;
            Geo = geo;
        }
    }
} 