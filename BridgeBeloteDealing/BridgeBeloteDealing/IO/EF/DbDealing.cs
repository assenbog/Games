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
    
    public partial class DbDealing
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DbDealing()
        {
            this.DbCards = new HashSet<DbCard>();
        }
    
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public System.DateTime TimeStamp { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DbCard> DbCards { get; set; }
        public virtual DbSortOrder DbSortOrder { get; set; }
    }
}
