// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Security.Cryptography;

namespace MasterServerKit
{
    /// <summary>
    /// Miscellaneous crypto utilities. We never store passwords directly in the database, but a
    /// combination of their salt and hash instead. This class contains useful methods for generating
    /// these combinations and checking input passwords against stored ones.
    /// </summary>
    public static class CryptoUtils
    {
        /// <summary>
        /// Length of a password's salt.
        /// </summary>
        public static readonly int saltLength = 16;

        /// <summary>
        /// Length of a password's hash.
        /// </summary>
        public static readonly int hashLength = 20;

        /// <summary>
        /// Number of iterations performed when calculating the hash of a password.
        /// </summary>
        public static readonly int iterations = 10000;

        /// <summary>
        /// Generates the salt + hash combination for the specified password.
        /// </summary>
        /// <param name="password">Password for which to generate the salt + hash combination.</param>
        /// <returns>The salt + hash combination of the specified password.</returns>
        public static string GenerateCombinedSaltAndHash(string password)
        {
            // Create the salt with a cryptographic PRNG.
            var salt = new byte[saltLength];
            new RNGCryptoServiceProvider().GetBytes(salt);

            // Create the password's hash value.
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            var hash = pbkdf2.GetBytes(hashLength);

            // Combine the salt and hash bytes for later use.
            var combinedSaltAndHash = new byte[saltLength + hashLength];
            Array.Copy(salt, 0, combinedSaltAndHash, 0, saltLength);
            Array.Copy(hash, 0, combinedSaltAndHash, saltLength, hashLength);

            // Turn the combined salt + hash into a string for storage.
            return Convert.ToBase64String(combinedSaltAndHash);
        }

        /// <summary>
        /// Checks if the specified input password is equal to the specified stored password.
        /// </summary>
        /// <param name="passwordToCheck">Input password.</param>
        /// <param name="storedPassword">Stored password.</param>
        /// <returns>True if the specified password is equal to the specified stored password;
        /// false otherwise.</returns>
        public static bool CheckPasswordsAreEqual(string passwordToCheck, string storedPassword)
        {
            var storedPasswordHash = Convert.FromBase64String(storedPassword);

            // Get the salt of the stored password.
            var salt = new byte[saltLength];
            Array.Copy(storedPasswordHash, 0, salt, 0, saltLength);

            // Create the input password's hash value.
            var pbkdf2 = new Rfc2898DeriveBytes(passwordToCheck, salt, iterations);
            var hash = pbkdf2.GetBytes(hashLength);

            // Compare the input password with the stored one.
            for (var i = 0; i < hashLength; i++)
            {
                if (storedPasswordHash[i + saltLength] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
