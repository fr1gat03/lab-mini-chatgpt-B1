using System.Text;
using MiniChatGPT.Contracts;
using Lib.Tokenization.Domain.Model;

namespace Lib.Tokenization.Application
{
    public class CharTokenizer : ITokenizer
    {
        private readonly Vocabulary<char> _vocabulary;

        public int VocabSize => _vocabulary.Count; 

        internal CharTokenizer(Vocabulary<char> vocabulary)
        {
            _vocabulary = vocabulary;
        }

        public static CharTokenizer BuildFromText(string text)
        {
            var vocab = new Vocabulary<char>();
            
            if (!string.IsNullOrEmpty(text))
            {
                foreach (char c in text)
                {
                    vocab.Add(c);
                }
            }
            return new CharTokenizer(vocab);
        }

        public int[] Encode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<int>();
            }

            int[] tokens = new int[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                int tokenId = _vocabulary.GetId(text[i]);

                if (tokenId < 0)
                {
                    throw new InvalidDataException($"Критична помилка: згенеровано невалідний токен з ID = {tokenId}.");
                } 
            
                tokens[i] = tokenId; 
            }
            
            return tokens;
        }

        public string Decode(ReadOnlySpan<int> tokens)
        {
            StringBuilder sb = new StringBuilder(tokens.Length);
            foreach (int token in tokens)
            {
                sb.Append(_vocabulary.GetItem(token)); 
            }
            return sb.ToString();
        }

        public object GetPayloadForCheckpoint()
        {
            char[] chars = new char[_vocabulary.Count];
            for (int i = 0; i < _vocabulary.Count; i++)
            {
                chars[i] = _vocabulary.GetItem(i);
            }
            return new { Chars = chars };
        }
        
        public string GetContractFingerprint()
        {
            return "Lib.Tokenization:v1.0:CharTokenizer"; 
        }
    }
}