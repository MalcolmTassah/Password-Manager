// ToDo:
//      1. Random charcter generator --DONE
//        1.1. Option for length and complexity (uppercase, lowercase, numbers, symbols) --DONE
//      2. Store passwords in a secure file
//        2.1. Option to retrieve, update, and delete existing passwords -- DONE
//      3. Store passwords in a dictionary with associated usernames



using System;
using System.Data.SQLite;              // Allows interaction with SQLite database
using System.IO;                       // For reading/writing files like master.hash
using System.Text;                     // For encoding strings (used in encryption)
using System.Security.Cryptography;    // Provides tools for encryption/hashing like AES and SHA256


public class PasswordGenerator
{
    public static string GeneratePassword(int length, bool includeUppercase, bool includeLowercase, bool includeNumbers, bool includeSymbols)
    {
        if (length < 6 || length > 32) // if less than 6 chars or greater than 32 chars
        {
            throw new ArgumentException("Password length must be between 6 and 32 characters.");
        }

        string characters = "";
        if (includeLowercase) characters += "abcdefghijklmnopqrstuvwxyz";   // if true incl lowercase letters
        if (includeUppercase) characters += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";   // if true incl uppercase letters
        if (includeNumbers) characters += "0123456789";                     // if true incl numbers
        if (includeSymbols) characters += @"!@#$%^&*()_+~`|}{[]\:;?><,./-="; // if true incl symbols

        if (string.IsNullOrEmpty(characters)) // if input is empty
        {
            throw new ArgumentException("At least one character type must be selected.");
        }

        StringBuilder password = new StringBuilder();
        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            int index = random.Next(characters.Length);  //  Randomly select a character from the characters string
            password.Append(characters[index]);          // Append the selected character to the password
        }

        return password.ToString();
    }

    public static void InitializeDatabase()
    {
        string dbPath = "passwords.db"; //Creates path for passwords database

        if (!File.Exists(dbPath)) // Checks if database already exists so it doesnt create a new one each time
        {
            SQLiteConnection.CreateFile(dbPath);
        }

        using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;")) //tells SQLite where to find database
        {
            connection.Open();
            string createTableQuery = @"CREATE TABLE IF NOT EXISTS Passwords (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        Website TEXT, 
                                        Username TEXT, 
                                        Password TEXT)";// the Id line assigns a unique id to each password - and the other lines are used to store the credentials
            using (var command = new SQLiteCommand(createTableQuery, connection))//creates table in database
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public static void SavePassword(string website, string username, string password)
    {
        using (var connection = new SQLiteConnection("Data Source=passwords.db;Version=3;"))// this line opens a connection to database
        {
            connection.Open();
            string insertQuery = "INSERT INTO Passwords (Website, Username, Password) VALUES (@Website, @Username, @Password)";
            using (var command = new SQLiteCommand(insertQuery, connection))// this tells computer to insert new row into database
            {
                command.Parameters.AddWithValue("@Website", website);
                command.Parameters.AddWithValue("@Username", username); //These lines make sure that SQLite treats user input as data and not code
                                                                        //EX: if an unauthorized user tries to enter '1'='1' the computer would read it as true
                                                                        //and allow access but they cant do that with the placeholders because it just stores
                                                                        //it as a string instead of actually executing it, that way they cant delete all stored data either
                string encryptedPassword = SecurityManager.Encrypt(password); // Encrypt password before saving
                command.Parameters.AddWithValue("@Password", encryptedPassword);

                command.ExecuteNonQuery();
            }
        }
    }

    public static void RetrievePasswords()
    {
        using (var connection = new SQLiteConnection("Data Source=passwords.db;Version=3;"))
        {
            connection.Open();
            string selectQuery = "SELECT * FROM Passwords"; //gets stored passwords
            using (var command = new SQLiteCommand(selectQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    //loops through to print all data for each website and prints it
                    string decrypted = SecurityManager.Decrypt(reader["Password"].ToString()); // Decrypt password before displaying
                    Console.WriteLine($"Website: {reader["Website"]}, Username: {reader["Username"]}, Password: {decrypted}");
                }
            }
        }
    }

    public static void UpdatePassword(string website, string username, string newPassword)
    {
        using (var connection = new SQLiteConnection("Data Source=passwords.db;Version=3;"))
        {
            connection.Open();
            string updateQuery = "UPDATE Passwords SET Password = @NewPassword WHERE Website = @Website AND Username = @Username"; // tells database what to change and where
            using (var command = new SQLiteCommand(updateQuery, connection))
            {
                string encryptedPassword = SecurityManager.Encrypt(newPassword); // Encrypt updated password
                command.Parameters.AddWithValue("@NewPassword", encryptedPassword);

                command.Parameters.AddWithValue("@Website", website);
                command.Parameters.AddWithValue("@Username", username);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0) // checks if password was actually updated (the number of rows that changed is > 0 means it updated successfully)
                    Console.WriteLine("Password updated successfully!");
                else
                    Console.WriteLine("No matching entry found.");
            }
        }
    }

    public static void DeletePassword(string website, string username)
    {
        using (var connection = new SQLiteConnection("Data Source=passwords.db;Version=3;"))
        {
            connection.Open();
            string deleteQuery = "DELETE FROM Passwords WHERE Website = @Website AND Username = @Username";
            using (var command = new SQLiteCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@Website", website);
                command.Parameters.AddWithValue("@Username", username);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                    Console.WriteLine("Password deleted successfully!");
                else
                    Console.WriteLine("No matching entry found.");
            }
        }
    }


    public static void Main(string[] args)
    {
        
        // Verify master password before allowing access
        if (!SecurityManager.VerifyMasterPassword())
        {
            Console.WriteLine("Incorrect master password. Access denied.");
            return;
        }

  
        InitializeDatabase(); // makes sure the database is set up

        int length = 0;
        bool includeUppercase = false;
        bool includeLowercase = false;
        bool includeNumbers = false;
        bool includeSymbols = false;

        Console.Write("\nWould you like to update or delete a password? (update(u)/delete(d)/add(a)): ");
        string choice = Console.ReadLine().ToLower();

        if (choice == "u")
        {
            Console.Write("Enter website name: ");
            string website = Console.ReadLine();
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter new password: ");
            string newPassword = Console.ReadLine();

            UpdatePassword(website, username, newPassword);
        }
        else if (choice == "d")
        {
            Console.Write("Enter website name: ");
            string website = Console.ReadLine();
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            DeletePassword(website, username);
        }

        else if (choice == "a")
           {
            Console.Write("Enter password length (6-32): ");
            while (!(int.TryParse(Console.ReadLine(), out length) && length >= 6 && length <= 32))
            {
                Console.WriteLine("Invalid input. Please enter a number between 6 and 32.");
            }

            Console.Write("Include uppercase letters? (y/n): ");
            includeUppercase = Console.ReadLine().ToLower() == "y";

            Console.Write("Include lowercase letters? (y/n): ");
            includeLowercase = Console.ReadLine().ToLower() == "y";

            Console.Write("Include numbers? (y/n): ");
            includeNumbers = Console.ReadLine().ToLower() == "y";

            Console.Write("Include symbols? (y/n): ");
            includeSymbols = Console.ReadLine().ToLower() == "y";

            Console.Write("Enter website name: ");
            string website = Console.ReadLine();

            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            try
            {
                string password = GeneratePassword(length, includeUppercase, includeLowercase, includeNumbers, includeSymbols);
                Console.WriteLine($"Generated password: {password}");

                SavePassword(website, username, password);
                Console.WriteLine("Password saved successfully!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.Write("\nWould you like to view all stored passwords? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                RetrievePasswords();
            }
           }
    }
}


public static class SecurityManager
{
    private const string MasterFile = "master.dat";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int Iterations = 600000;

    private static byte[] encryptionKey;

    // Derives an AES-256 key from the master password.
    private static byte[] DeriveKey(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize
        );
    }

    // Encrypts a password using AES-GCM.
    public static string Encrypt(string plainText)
    {
        if (encryptionKey == null)
            throw new InvalidOperationException("Log in first.");

        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] plaintext = Encoding.UTF8.GetBytes(plainText);
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[TagSize];

        using (AesGcm aes = new AesGcm(encryptionKey, TagSize))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        // Store nonce + authentication tag + encrypted password.
        byte[] result = new byte[NonceSize + TagSize + ciphertext.Length];

        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, result,
            NonceSize + TagSize, ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    // Decrypts a stored password using AES-GCM.
    public static string Decrypt(string encryptedText)
    {
        if (encryptionKey == null)
            throw new InvalidOperationException("Log in first.");

        byte[] data = Convert.FromBase64String(encryptedText);

        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Invalid encrypted data.");

        byte[] nonce = data[..NonceSize];
        byte[] tag = data[NonceSize..(NonceSize + TagSize)];
        byte[] ciphertext = data[(NonceSize + TagSize)..];
        byte[] plaintext = new byte[ciphertext.Length];

        using (AesGcm aes = new AesGcm(encryptionKey, TagSize))
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }

        return Encoding.UTF8.GetString(plaintext);
    }

    // Verifies the master password and initializes the encryption key.
    public static bool VerifyMasterPassword()
    {
        if (!File.Exists(MasterFile))
        {
            Console.Write("Create a master password: ");
            string password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Master password cannot be empty.");
                return false;
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = DeriveKey(password, salt);

            // Store an encrypted verification value.
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] verification = Encoding.UTF8.GetBytes("PasswordManager");
            byte[] ciphertext = new byte[verification.Length];
            byte[] tag = new byte[TagSize];

            using (AesGcm aes = new AesGcm(key, TagSize))
            {
                aes.Encrypt(nonce, verification, ciphertext, tag);
            }

            // File format: salt + nonce + tag + ciphertext
            byte[] fileData = new byte[
                SaltSize + NonceSize + TagSize + ciphertext.Length
            ];

            int offset = 0;

            Buffer.BlockCopy(salt, 0, fileData, offset, SaltSize);
            offset += SaltSize;

            Buffer.BlockCopy(nonce, 0, fileData, offset, NonceSize);
            offset += NonceSize;

            Buffer.BlockCopy(tag, 0, fileData, offset, TagSize);
            offset += TagSize;

            Buffer.BlockCopy(ciphertext, 0, fileData, offset,
                ciphertext.Length);

            File.WriteAllBytes(MasterFile, fileData);

            CryptographicOperations.ZeroMemory(key);

            Console.WriteLine("Master password created. Restart to log in.");
            Environment.Exit(0);
            return false;
        }

        Console.Write("Enter master password: ");
        string input = Console.ReadLine();

        if (input == null)
            return false;

        byte[] storedData = File.ReadAllBytes(MasterFile);

        if (storedData.Length < SaltSize + NonceSize + TagSize)
        {
            Console.WriteLine("Invalid master password file.");
            return false;
        }

        byte[] storedSalt = storedData[..SaltSize];

        byte[] storedNonce =
            storedData[SaltSize..(SaltSize + NonceSize)];

        byte[] storedTag =
            storedData[(SaltSize + NonceSize)..
                (SaltSize + NonceSize + TagSize)];

        byte[] storedCiphertext =
            storedData[(SaltSize + NonceSize + TagSize)..];

        byte[] derivedKey = DeriveKey(input, storedSalt);
        byte[] decrypted = new byte[storedCiphertext.Length];

        try
        {
            using (AesGcm aes = new AesGcm(derivedKey, TagSize))
            {
                aes.Decrypt(
                    storedNonce,
                    storedCiphertext,
                    storedTag,
                    decrypted
                );
            }

            bool valid = CryptographicOperations.FixedTimeEquals(
                decrypted,
                Encoding.UTF8.GetBytes("PasswordManager")
            );

            if (!valid)
            {
                CryptographicOperations.ZeroMemory(derivedKey);
                return false;
            }

            encryptionKey = derivedKey;
            return true;
        }
        catch (CryptographicException)
        {
            CryptographicOperations.ZeroMemory(derivedKey);
            return false;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(decrypted);
        }
    }
}


        
