// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

namespace MasterServerKit
{
    /// <summary>
    /// This class stores a server property. Properties can be used to define custom data
    /// belonging to a specific server (e.g., region, game type, etc.).
    /// </summary>
    [Serializable]
    public class Property
    {
        /// <summary>
        /// The name of this property.
        /// </summary>
        public string name;

        /// <summary>
        /// The value of this property.
        /// </summary>
        public string value;
    }
}
