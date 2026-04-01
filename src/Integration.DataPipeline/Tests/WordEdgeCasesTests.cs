using NUnit.Framework;
using Lib.Corpus;
using Lib.Tokenization.Application;
using MiniChatGPT.Contracts;
using Integration.DataPipeline.Mocks; 

namespace Integration.DataPipeline.Tests; 

[TestFixture]
public class WordEdgeCasesTests
{
    [Test]
    public void WordTokenizer_WithEmptyAndSingleWord_HandlesGracefully()
    {
        var mockFs = new MockFileSystem();
        var loader = new CorpusLoader(mockFs);
        var factory = new WordTokenizerFactory();

        mockFs.AddFile("empty.txt", "");
        var emptyCorpus = loader.Load("empty.txt");
        ITokenizer emptyTokenizer = factory.BuildFromText(emptyCorpus.TrainText); 
        
        Assert.That(emptyTokenizer.VocabSize, Is.EqualTo(1));

        mockFs.AddFile("single.txt", "Привіт");
        var singleCorpus = loader.Load("single.txt");
        ITokenizer singleTokenizer = factory.BuildFromText(singleCorpus.TrainText);
        
        var encoded = singleTokenizer.Encode("Привіт");
        Assert.That(singleTokenizer.Decode(encoded), Is.EqualTo("привіт"));
    }
}