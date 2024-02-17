using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Services;

public class UrlShorteningService
{
    public const int NumberOfCharsInShortLink = 7;
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly Random random = new Random();
    private readonly ApplicationDbContext dbContext;

    public UrlShorteningService(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<string> GenerateUniqueCode()
    {
        var codechars = new char[NumberOfCharsInShortLink];

        while (true)
        {
            for (var i = 0; i < NumberOfCharsInShortLink; i++)
            {
                var randomIndex = random.Next(Alphabet.Length - 1);
                codechars[i] = Alphabet[randomIndex];
            }

            var code = new string(codechars);

            if (! (await dbContext.ShortenedUrls.AnyAsync(s => s.Code == code)))
            {
                return code;
            }
        }
    }
}
