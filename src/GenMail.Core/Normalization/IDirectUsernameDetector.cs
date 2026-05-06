namespace GenMail.Core.Normalization;

public interface IDirectUsernameDetector
{
    bool IsDirectUsername(string input);
}
