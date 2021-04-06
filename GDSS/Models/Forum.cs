//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GDSS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Forum
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Forum()
        {
            this.ForumComments = new HashSet<ForumComment>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int DiscussionID { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<System.DateTime> PostedOn { get; set; }
    
        public virtual Discussion Discussion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ForumComment> ForumComments { get; set; }

        public ForumComment ForumComment
        {
            get => default;
            set
            {
            }
        }
    }
}
