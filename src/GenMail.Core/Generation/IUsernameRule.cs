using GenMail.Core.Models;

namespace GenMail.Core.Generation;

public interface IUsernameRule
{
    string Id { get; }

    string Render(NormalizedName name);
}
