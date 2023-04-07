using System.Globalization;
using CsvHelper;
using IAD_Lab2;

var trainSet = new List<Message>();
ReadFile("train.csv", trainSet);

var testSet = new List<Message>();
ReadFile("test.csv", testSet);

var bayesClassifier = new BayesClassifier(trainSet);
Console.WriteLine(TestModel(bayesClassifier, testSet));

double TestModel(BayesClassifier classifier, ICollection<Message> set)
{
    var successfulOperations = set.Count(message =>
        (message.Type == "spam" && classifier.IsSpam(message.Text)) ||
        (message.Type == "ham" && !classifier.IsSpam(message.Text)));

    return (double) successfulOperations / set.Count;
}

void ReadFile(string path, ICollection<Message> set)
{
    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    while (csv.Read())
    {
        set.Add(csv.GetRecord<Message>());
    }
}