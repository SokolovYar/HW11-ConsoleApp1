using Newtonsoft.Json;
using System.ComponentModel;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Exchange.Run();
Exchange.ChangeMenu();
Console.WriteLine(Exchange.Change("USD", 100));


//Exchange - статический класс. Считывание из файла/сервера и проверка свежести осуществляется при создании
public static class Exchange
{
    private static List<Currency>? _currencies;
    private static string pathToFile = "temp.json";
    private static string ServerPath = @"https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";

    static Exchange()
    {  
        _currencies = new List<Currency>(3);
        if (File.Exists(pathToFile))
        {
            using (StreamReader file = File.OpenText(pathToFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                _currencies = (List<Currency> ?) serializer.Deserialize(file, typeof(List<Currency>));
            }
            if (!isFresh()) GetJsonFromServer(ServerPath);
        }
        else
        {
            GetJsonFromServer(ServerPath);
        }
    }

    public static void Run()
    { }

    public static async void GetJsonFromServer(string serverPath)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage? response = await client.GetAsync(serverPath);
        if (response.IsSuccessStatusCode)
        {
            string jsonString = await response.Content.ReadAsStringAsync();
            List<Currency>? data = JsonConvert.DeserializeObject<List<Currency>>(jsonString);


            foreach (var it in data)
                if (it.cc == "USD" || it.cc == "EUR" || it.cc == "RUB") _currencies.Add(it);
            SerializeToFile();
        }
    }

    static private void SerializeToFile()
    {
        using (StreamWriter file = File.CreateText(pathToFile))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, _currencies);
        }
    }

    static private bool isFresh()
    {
        DateTime Temp;
        DateTime.TryParse(_currencies[0].exchangedate, out Temp);
        DateTime _now = DateTime.Today;
        return Temp.AddDays(1) >= _now;
    }

    //реализация через меню
    static public void ChangeMenu()
    {
        Console.WriteLine("Current rate of currencies");
        foreach (var it in _currencies)
            Console.WriteLine(it);

        Console.WriteLine("\n\nChoose type of currency to exchange (1-3): ");
        int choose;
        choose = Convert.ToInt32(Console.ReadLine());
        if (choose !=0)
        {
            Console.WriteLine("\n\nYou choose next currnecy:");
            Console.WriteLine(_currencies[choose-1]);
            Console.WriteLine($"Enter amount of {_currencies[choose - 1].cc} you want to buy: ");
            int amount;
            amount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine($"To buy {_currencies[choose - 1].cc}{amount} you need UAH{_currencies[choose - 1].rate * amount}");
        }
        else  Console.WriteLine("Wrong number has entered!"); 
    }

    //реализация через параметры метода
    static public decimal Change(string Cur, decimal amount)
    {
        foreach (var it in _currencies)
            if (it.cc == Cur)
                return amount * it.rate;
        return -1;
    }
}

public class Currency
{
    public int r030 { get; set; }
    public string txt { get; set; }
    public decimal rate { get; set; }
    public string cc { get; set; }
    public string exchangedate { get; set; }
    public Currency(int r030, string txt, decimal rate, string cc, string exchangedate)
    {
        this.r030 = r030;
        this.txt = txt;
        this.rate = rate;
        this.cc = cc;
        this.exchangedate = exchangedate;
    }   
    public Currency() { }
    public override string ToString()
    {
        return $"{txt}: {rate}UAH for a 1 {cc}";
    }
}

