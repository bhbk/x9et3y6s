namespace Bhbk.Lib.Identity.Domain.Encryption
{
    public interface IConfigEncryptionProvider
    {
        string Prefix { get; }
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
        bool CanDecrypt(string prefixedValue);
    }
}
