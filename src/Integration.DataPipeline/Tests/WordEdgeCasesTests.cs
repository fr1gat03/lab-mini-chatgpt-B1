using Lib.Corpus;
using Lib.Corpus.Infrastructure;
using Lib.Tokenization.Application;
using MiniChatGPT.Contracts;
using NUnit.Framework;

namespace Integration.DataPipeline;

[TestFixture]
public class WordEdgeCasesTests
{
    private class EdgeCaseFileSystem : IFileSystem
    {
        public bool Exists(string path) => true;
        public string ReadAllText(string path) => FileContent;
        public string FileContent { get; set; } = string.Empty;
    }

    [Test]
    public void WordTokenizer_WithEmptyAndSingleWord_HandlesGracefully()
    {
        var fakeFs = new EdgeCaseFileSystem();
        var loader = new CorpusLoader(fakeFs);

        fakeFs.FileContent = "";
        var emptyCorpus = loader.Load("empty.txt");
        ITokenizer emptyTokenizer = WordTokenizer.BuildFromText(emptyCorpus.TrainText);
        Assert.That(emptyTokenizer.VocabSize, Is.EqualTo(1));

        fakeFs.FileContent = "Привіт";
        var singleCorpus = loader.Load("single.txt");
        ITokenizer singleTokenizer = WordTokenizer.BuildFromText(singleCorpus.TrainText);
        
        var encoded = singleTokenizer.Encode("Привіт");
        Assert.That(singleTokenizer.Decode(encoded), Is.EqualTo("привіт"));
    }
}