using NUnit.Framework;
using Lib.Corpus;
using Lib.Corpus.Configuration;
using Lib.Tokenization.Application;
using MiniChatGPT.Contracts;
using Integration.DataPipeline.Mocks; 

namespace Integration.DataPipeline.Tests; 

[TestFixture]
public class WordRoundTripTests
{
    [Test]
    public void WordTokenizer_CorpusRoundTrip_Matches()
    {
        var mockFs = new MockFileSystem();
        mockFs.AddFile("input.txt", "Текст має бути однаковим."); 
        
        var loader = new CorpusLoader(mockFs);
        var options = new CorpusLoadOptions(Lowercase: true, ValidationFraction: 0.0);
        var corpus = loader.Load("input.txt", options);

        var factory = new WordTokenizerFactory();
        ITokenizer tokenizer = factory.BuildFromText(corpus.TrainText);

        var encoded = tokenizer.Encode(corpus.TrainText);   
        var decoded = tokenizer.Decode(encoded);

        Assert.That(decoded, Is.EqualTo("текст має бути однаковим"));
    }
}