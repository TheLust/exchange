using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Exchange
{
    public partial class Main : Form
    {

        private const string API_KEY = "b07996ddbb3a1b9bd96826ae";
        private const string BASE_URL = "https://v6.exchangerate-api.com/v6/";

        private Dictionary<string, string> _currencies;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _currencies = GetCurrencies().Result;
            foreach (var value in _currencies)
            {
                FromComboBox.Items.Add(value);
                ToComboBox.Items.Add(value);
            }
            FromComboBox.ValueMember = "Key";
            FromComboBox.DisplayMember = "Value";
            ToComboBox.ValueMember = "Key";
            ToComboBox.DisplayMember = "Value";
        }

        private async Task<Dictionary<string, string>> GetCurrencies()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(BASE_URL + API_KEY + "/codes").ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    JObject responseData = JObject.Parse(json);

                    if (responseData["result"].ToString() != "success")
                    {
                        throw new InvalidOperationException("API request was not successful.");
                    }

                    JArray supportedCodesArray = responseData["supported_codes"] as JArray;

                    Dictionary<string, string> currencyCodes = new Dictionary<string, string>();

                    foreach (var codeInfo in supportedCodesArray)
                    {
                        string code = codeInfo[0].ToString();
                        string name = codeInfo[1].ToString();
                        currencyCodes.Add(code, name);
                    }

                    return currencyCodes;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP request error: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw;
                }
            }
        }

        private async Task<double> GetExchangeRate(string baseCurrency, string targetCurrency)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(BASE_URL + API_KEY + "/pair/" + baseCurrency + "/" + targetCurrency).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    JObject responseData = JObject.Parse(json);

                    if (responseData["result"].ToString() != "success")
                    {
                        throw new InvalidOperationException("API request was not successful.");
                    }

                    double exchangeRate = Convert.ToDouble(responseData["conversion_rate"]);
                    return exchangeRate;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP request error: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw;
                }
            }
        }

        private void ExchangeButton_Click(object sender, EventArgs e)
        {
            if (FromComboBox.SelectedItem == null || ToComboBox.SelectedItem == null && string.IsNullOrWhiteSpace(AmountTextBox.Text)) { return; }

            try {
                string from = ((KeyValuePair<string, string>)FromComboBox.SelectedItem).Key;
                string to = ((KeyValuePair<string, string>)ToComboBox.SelectedItem).Key;
                ResultLabel.Text = (Convert.ToDouble(AmountTextBox.Text) * GetExchangeRate(from, to).Result).ToString();
            } catch (Exception)
            {
                return;
            }
        }
    }
}
