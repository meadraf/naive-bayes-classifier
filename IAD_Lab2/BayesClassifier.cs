namespace IAD_Lab2;

public class BayesClassifier
{
    private readonly List<Message> _messages;
    private readonly Dictionary<string, int> _wordCount = new();
    private readonly Dictionary<string, TypeCount> _typeCount = new();
    private readonly Dictionary<string, WordTypeProbability> _typeProbabilities = new();
    private readonly Dictionary<string, WordTypeProbability> _normTypeProbabilities = new();

    public BayesClassifier(IEnumerable<Message> messages)
    {
        _messages = new List<Message>(messages);
        CountWords();
        ClassifyWordsByType();
        CalculateTypeProbabilities();
        NormProbabilities();
    }

    public bool IsSpam(string message)
    {
        var words = message.Split(' ');
        var spamProbability = 0.5;
        var hamProbability = 0.5;
        foreach (var word in words)
        {
            _normTypeProbabilities.TryGetValue(word, out var wordTypeProbability);

            if (wordTypeProbability is null)
                continue;

            spamProbability *= wordTypeProbability.SpamProbability;
            hamProbability *= wordTypeProbability.HamProbability;
        }

        return spamProbability > hamProbability;
    }
    
    private Dictionary<string, int> CountWords()
    {
        foreach (var word in _messages.Select(item => item.Text.Split(' ')).SelectMany(words => words))
        {
            if (_wordCount.ContainsKey(word))
                _wordCount[word]++;
            else
                _wordCount.Add(word, 1);
        }

        return _wordCount;
    }

    private Dictionary<string, TypeCount> ClassifyWordsByType()
    {
        foreach (var message in _messages)
        {
            var words = message.Text.Split(' ');

            switch (message.Type)
            {
                case "spam":
                    AddSpamWords(words);
                    break;
                case "ham":
                    AddHamWords(words);
                    break;
            }
        }

        return _typeCount;
    }

    private Dictionary<string, WordTypeProbability> CalculateTypeProbabilities()
    {
        foreach (var word in _typeCount)
        {
            var spamProbability = (double) word.Value.Spam / (word.Value.Spam + word.Value.Ham);
            var hamProbability = (double) word.Value.Ham / (word.Value.Spam + word.Value.Ham);
            _typeProbabilities.Add(word.Key,
                new WordTypeProbability {HamProbability = hamProbability, SpamProbability = spamProbability});
        }

        return _typeProbabilities;
    }

    private Dictionary<string, WordTypeProbability> NormProbabilities()
    {
        foreach (var item in _typeProbabilities)
        {
            var normSpamProbability = (_wordCount[item.Key] * _typeProbabilities[item.Key].SpamProbability + 0.5) /
                                      (_wordCount[item.Key] + 1);
            var normHamProbability = (_wordCount[item.Key] * _typeProbabilities[item.Key].HamProbability + 0.5) /
                                     (_wordCount[item.Key] + 1);

            _normTypeProbabilities.Add(item.Key,
                new WordTypeProbability {HamProbability = normHamProbability, SpamProbability = normSpamProbability});
        }

        return _normTypeProbabilities;
    }
    
    private void AddHamWords(IEnumerable<string> words)
    {
        foreach (var word in words)
        {
            if (_typeCount.TryGetValue(word, out var wordType))
                wordType.Ham++;
            else
                _typeCount.Add(word, new TypeCount {Ham = 1, Spam = 0});
        }
    }

    private void AddSpamWords(IEnumerable<string> words)
    {
        foreach (var word in words)
        {
            if (_typeCount.TryGetValue(word, out var wordType))
                wordType.Spam++;
            else
                _typeCount.Add(word, new TypeCount {Ham = 0, Spam = 1});
        }
    }
}