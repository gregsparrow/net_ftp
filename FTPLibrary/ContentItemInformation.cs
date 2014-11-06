using System;
using System.Collections.Generic;
using System.Text;

namespace FTPLibrary
{
    /// <summary>
    /// Container for item information
    /// </summary>
    public class ContentItemInformation
    {
        private ContentItemType type;
        private int rights, linksCount;
        private string owner, group, name;
        private long size;
        private DateTime lastChange;
        private DateTime created;

        /// <summary>
        /// Get or Set the type of item
        /// </summary>
        public ContentItemType ItemType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        /// <summary>
        /// Get or Set item's permition
        /// </summary>
        public int Rights
        {
            get
            {
                return rights;
            }
            set
            {
                rights = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int LinksCount
        {
            get
            {
                return linksCount;
            }
            set
            {
                linksCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Owner
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Group
        {
            get
            {
                return group;
            }
            set
            {
                group = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LastChange
        {
            get
            {
                return lastChange;
            }
            set
            {
                lastChange = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Created
        {
            get
            {
                return created;
            }
            set
            {
                created = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

    }
}
