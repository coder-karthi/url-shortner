using System.Text;

namespace URLShortner.Services;

public class Base62Service
{
    private const string Characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly Random random = new();

    public string GenerateShortCode(int length = 6)
    {
        var result = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            var index = random.Next(Characters.Length);
            result.Append(Characters[index]);
        }

        return result.ToString();
    }
}