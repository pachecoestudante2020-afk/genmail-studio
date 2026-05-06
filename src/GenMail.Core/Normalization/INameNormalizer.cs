using GenMail.Core.Models;

namespace GenMail.Core.Normalization;

public interface INameNormalizer
{
    NormalizedName Normalize(string input);
}
