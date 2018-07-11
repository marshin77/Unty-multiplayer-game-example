// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace MasterServerKit
{
    /// <summary>
    /// Miscellaneous string utilities.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Returns true if the specified string is null, empty or consists exclusively of whitespace
        /// characters.
        /// </summary>
        /// <param name="s">String to check.</param>
        /// <returns>True if the specified string is null, empty or consists exclusively of whitespace
        /// characters and false otherwise.</returns>
        public static bool IsNullOrWhitespace(this string s)
        {
            return string.IsNullOrEmpty(s) || s.Trim().Length == 0;
        }
    }
}
