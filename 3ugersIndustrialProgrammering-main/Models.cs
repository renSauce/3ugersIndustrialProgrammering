using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemLogin;

public class AccountService(AppDbContext db, PasswordHasher hasher)
{
    public async Task NewAccountAsync(string username, string password, bool isAdmin = false)
    {
        var (salt, saltedPasswordHash) = hasher.Hash(password);
        db.Add(new Account
        {
            Username = username,
            Salt = salt,
            SaltedPasswordHash = saltedPasswordHash,
            isAdmin = isAdmin
        });
        await db.SaveChangesAsync();
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        return db.Accounts.AnyAsync(a => a.Username == username);
    }

    public async Task<bool> CredentialsCorrectAsync(string username, string password)
    {
        var account = await db.Accounts.FirstAsync(a => a.Username == username);
        return hasher.PasswordCorrect(password, account.Salt, account.SaltedPasswordHash);
    }

    public Task<bool> UserIsAdminAsync(string username)
    {
        return db.Accounts.Where(a => a.Username == username).Select(a => a.isAdmin).FirstAsync();
    }

    public Task<Account> GetAccountAsync(string username)
    {
        return db.Accounts.FirstAsync(a => a.Username == username);
    }
}

public class PasswordHasher(
    int saltLength = 128 / 8,
    int hashIterations = 600_000
    // salt and iterations according to https://en.wikipedia.org/wiki/PBKDF2
)
{
    public bool PasswordCorrect(string password, byte[] salt, byte[] saltedPasswordHash)
    {
        return CryptographicOperations.FixedTimeEquals(Hash(salt, password), saltedPasswordHash);
    }

    private byte[] Hash(byte[] salt, string password)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            hashIterations,
            HashAlgorithmName.SHA256,
            256 / 8 // Due to SHA256
        );
    }

    public (byte[] Salt, byte[] Hash) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(saltLength);
        return (salt, Hash(salt, password));
    }
}

public class AppDbContext(string dbPath = "../../../database.sqlite") : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}

public class Account
{
    [Key] public string Username { get; set; }

    public byte[] Salt { get; set; }
    public byte[] SaltedPasswordHash { get; set; }
    public bool isAdmin { get; set; }
}