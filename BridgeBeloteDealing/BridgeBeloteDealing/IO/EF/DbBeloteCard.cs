//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BridgeBeloteDealing.IO.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class DbBeloteCard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DbBeloteCard()
        {
            this.DbCards = new HashSet<DbCard>();
        }
    
        public int Value { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SuitSortOrder { get; set; }
        public int NoTrumpSortOrder { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DbCard> DbCards { get; set; }
    }
}
