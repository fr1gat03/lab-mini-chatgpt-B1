using System.Text;
using System.Text.RegularExpressions;
using MiniChatGPT.Contracts;
using Lib.Tokenization.Domain.Model;

namespace Lib.Tokenization.Application
{
    public class WordTokenizer : ITokenizer
    {
        private readonly WordVocabulary _vocabulary;

        public int VocabSize => _vocabulary.Count;

        internal WordTokenizer(WordVocabulary vocabulary)
        {
            _vocabulary = vocabulary;
        }

        private static string NormalizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return string.Empty;
            return word.ToLower().Trim('.', ',', '!', '?', ':', ';', '"', '\'');
        }

        public static WordTokenizer BuildFromText(string text)
        {
            var vocab = new WordVocabulary();

            if (!string.IsNullOrWhiteSpace(text))
            {
                string[] words = Regex.Split(text, @"\s+");
                foreach (string rawWord in words)
                {
                    string normalized = NormalizeWord(rawWord);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        vocab.Add(normalized);
                    }
                }
            }

            return new WordTokenizer(vocab);
        }

        public int[] Encode(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Array.Empty<int>();

            string[] words = Regex.Split(text, @"\s+");
            int[] tempTokens = new int[words.Length];
            int validCount = 0;

            foreach (string rawWord in words)
            {
                string normalized = NormalizeWord(rawWord);
                if (string.IsNullOrEmpty(normalized)) continue;

                int tokenId = _vocabulary.GetId(normalized);
        
                if (tokenId < 0) 
                    throw new InvalidDataException($"Критична помилка: згенеровано невалідний токен з ID = {tokenId}.");

                tempTokens[validCount] = tokenId;
                validCount++;
            }

            int[] result = new int[validCount];
            Array.Copy(tempTokens, result, validCount);
    
            return result;
        }

        public string Decode(ReadOnlySpan<int> tokens)
        {
            if (tokens.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tokens.Length; i++)
            {
                string word = _vocabulary.GetItem(tokens[i]);

                sb.Append(word ?? "<UNK>");

                if (i < tokens.Length - 1) sb.Append(" ");
            }

            return sb.ToString();
        }

        public object GetPayloadForCheckpoint()
        {
            string[] words = new string[_vocabulary.Count];
            for (int i = 0; i < _vocabulary.Count; i++)
            {
                words[i] = _vocabulary.GetItem(i);
            }

            return new { Words = words };
        }

        public string GetContractFingerprint()
        {
            return "Lib.Tokenization:v1.0:WordTokenizer";
        }
    }
}