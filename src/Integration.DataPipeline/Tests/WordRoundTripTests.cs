using Lib.Corpus;
using Lib.Corpus.Configuration;
using Lib.Corpus.Infrastructure;
using Lib.Tokenization.Application;
using MiniChatGPT.Contracts;
using NUnit.Framework;

namespace Integration.DataPipeline;

public class FakeFileSystem : IFileSystem
{
    public bool FileExists { get; set; } = true;
    public string FileContent { get; set; } = string.Empty;
    public bool Exists(string path) => FileExists;
    public string ReadAllText(string path) => FileContent;
}

[TestFixture]
public class WordRoundTripTests
{
    [Test]
    public void WordTokenizer_CorpusRoundTrip_Matches()
    {
        var fakeFs = new FakeFileSystem { FileExists = true, FileContent = "Текст має бути однаковим." };
        var loader = new CorpusLoader(fakeFs);
        var options = new CorpusLoadOptions(Lowercase: true, ValidationFraction: 0.0);

        var corpus = loader.Load("input.txt", options);
        ITokenizer tokenizer = WordTokenizer.BuildFromText(corpus.TrainText);

        var encoded = tokenizer.Encode(corpus.TrainText);
        var decoded = tokenizer.Decode(encoded);

        Assert.That(decoded, Is.EqualTo("текст має бути однаковим"));
    }
}